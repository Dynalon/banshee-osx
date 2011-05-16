//
// PlayerEngine.cs
//
// Author:
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2010 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Mono.Unix;

using Gst;
using Gst.PbUtils;
using Gst.BasePlugins;

using Hyena;
using Hyena.Data;

using Banshee.Base;
using Banshee.Streaming;
using Banshee.MediaEngine;
using Banshee.ServiceStack;
using Banshee.Configuration;
using Banshee.Preferences;

namespace Banshee.GStreamerSharp
{
    public class PlayerEngine : Banshee.MediaEngine.PlayerEngine
    {
        private class AudioSinkBin : Bin
        {
            Element hw_audio_sink;
            Element volume;
            Element rgvolume;
            Element first;
            GhostPad visible_sink;
            object pipeline_lock = new object ();

            public AudioSinkBin (string elementName) : base(elementName)
            {
                hw_audio_sink = SelectAudioSink ();
                Add (hw_audio_sink);
                first = hw_audio_sink;

                volume = FindVolumeProvider (hw_audio_sink);
                if (volume != null) {
                    // If the sink provides its own volume property we assume that it will
                    // also save that value across program runs.  Pulsesink has this behaviour.
                    VolumeNeedsSaving = false;
                } else {
                    volume = ElementFactory.Make ("volume", "volume");
                    VolumeNeedsSaving = true;
                    Add (volume);
                    volume.Link (hw_audio_sink);
                    first = volume;
                }

                visible_sink = new GhostPad ("sink", first.GetStaticPad ("sink"));
                AddPad (visible_sink);
            }

            static Element FindVolumeProvider (Element sink)
            {
                Element volumeProvider = null;
                // Sinks which automatically select between a number of possibilities
                // (such as autoaudiosink and gconfaudiosink) need to be at least in
                // the Ready state before they'll contain an actual sink.
                sink.SetState (State.Ready);

                if (sink.HasProperty ("volume")) {
                    volumeProvider = sink;
                    Log.DebugFormat ("Sink {0} has native volume.", volumeProvider.Name);
                } else {
                    var sinkBin = sink as Bin;
                    if (sinkBin != null) {
                        foreach (Element e in sinkBin.ElementsRecurse) {
                            if (e.HasProperty ("volume")) {
                                volumeProvider = e;
                                Log.DebugFormat ("Found volume provider {0} in {1}.",
                                    volumeProvider.Name, sink.Name);
                            }
                        }
                    }
                }
                return volumeProvider;
            }

            static Element SelectAudioSink ()
            {
                Element audiosink = null;

                // Default to GConfAudioSink, which should Do The Right Thing.
                audiosink = ElementFactory.Make ("gconfaudiosink", "audiosink");
                if (audiosink == null) {
                    // Try DirectSoundSink, which should work on Windows
                    audiosink = ElementFactory.Make ("directsoundsink", "audiosink");
                    if (audiosink != null) {
                        // The unmanaged code sets the volume on the directsoundsink here.
                        // Presumably this fixes a problem, but there's no reference as to what it is.
                        audiosink["volume"] = 1.0;
                    } else {
                        audiosink = ElementFactory.Make ("autoaudiosink", "audiosink");
                        if (audiosink == null) {
                            // As a last-ditch effort try ALSA.
                            audiosink = ElementFactory.Make ("alsasink", "audiosink");
                        }
                    }
                }
                return audiosink;
            }

            public bool ReplayGainEnabled {
                get { return rgvolume != null; }
                set {
                    if (value && rgvolume == null) {
                        visible_sink.SetBlocked (true, InsertReplayGain);
                        Log.Debug ("Enabled ReplayGain volume scaling.");
                    } else if (!value && rgvolume != null) {
                        visible_sink.SetBlocked (false, RemoveReplayGain);
                        Log.Debug ("Disabled ReplayGain volume scaling.");
                    }
                }
            }

            void InsertReplayGain (Pad pad, bool blocked)
            {
                lock (pipeline_lock) {
                    if (rgvolume == null) {
                        rgvolume = ElementFactory.Make ("rgvolume", "rgvolume");
                        Add (rgvolume);
                        rgvolume.SyncStateWithParent ();
                        visible_sink.SetTarget (rgvolume.GetStaticPad ("sink"));
                        rgvolume.Link (first);
                        first = rgvolume;
                    }
                }
                visible_sink.SetBlocked (false, (_, __) => { });
            }

            void RemoveReplayGain (Pad pad, bool blocked)
            {
                lock (pipeline_lock) {
                    if (rgvolume != null) {
                        first = rgvolume.GetStaticPad ("src").Peer.Parent as Element;
                        rgvolume.Unlink (first);
                        rgvolume.SetState (State.Null);
                        Remove (rgvolume);
                        rgvolume = null;
                        visible_sink.SetTarget (first.GetStaticPad ("sink"));
                    }
                }
                visible_sink.SetBlocked (false, (_, __) => { });
            }


            public bool VolumeNeedsSaving { get; private set; }
            public double Volume {
                get {
                    return (double)volume["volume"];
                }
                set {
                    if (value < 0 || value > 10.0) {
                        throw new ArgumentOutOfRangeException ("value", "Volume must be between 0 and 10.0");
                    }
                    Log.DebugFormat ("Setting volume to {0:0.00}", value);
                    volume["volume"] = value;
                }
            }
        }


        PlayBin2 playbin;
        AudioSinkBin audio_sink;
        uint iterate_timeout_id = 0;
        List<string> missing_details = new List<string> ();
        ManualResetEvent next_track_set;

        public PlayerEngine ()
        {
            Log.InformationFormat ("GStreamer# {0} Initializing; {1}.{2}",
                typeof (Gst.Version).Assembly.GetName ().Version, Gst.Version.Description, Gst.Version.Nano);

            // Setup the gst plugins/registry paths if running Windows
            if (PlatformDetection.IsWindows) {
                var gst_paths = new string [] { Hyena.Paths.Combine (Hyena.Paths.InstalledApplicationPrefix, "gst-plugins") };
                Environment.SetEnvironmentVariable ("GST_PLUGIN_PATH", String.Join (";", gst_paths));
                Environment.SetEnvironmentVariable ("GST_PLUGIN_SYSTEM_PATH", "");
                Environment.SetEnvironmentVariable ("GST_DEBUG", "1");

                string registry = Hyena.Paths.Combine (Hyena.Paths.ApplicationData, "registry.bin");
                if (!System.IO.File.Exists (registry)) {
                    System.IO.File.Create (registry).Close ();
                }

                Environment.SetEnvironmentVariable ("GST_REGISTRY", registry);

                //System.Environment.SetEnvironmentVariable ("GST_REGISTRY_FORK", "no");
                Log.DebugFormat ("GST_PLUGIN_PATH = {0}", Environment.GetEnvironmentVariable ("GST_PLUGIN_PATH"));
            }

            Gst.Application.Init ();
            playbin = new PlayBin2 ();

            next_track_set = new ManualResetEvent (false);

            audio_sink = new AudioSinkBin ("audiobin");

            playbin["audio-sink"] = audio_sink;

            if (audio_sink.VolumeNeedsSaving) {
                // Remember the volume from last time
                Volume = (ushort)PlayerEngineService.VolumeSchema.Get ();
            }

            playbin.AddNotification ("volume", OnVolumeChanged);
            playbin.Bus.AddWatch (OnBusMessage);
            playbin.AboutToFinish += OnAboutToFinish;

            OnStateChanged (PlayerState.Ready);
        }

        protected override bool DelayedInitialize {
            get {
                return true;
            }
        }

        protected override void Initialize ()
        {
            base.Initialize ();
            InstallPreferences ();
            audio_sink.ReplayGainEnabled = ReplayGainEnabledSchema.Get ();
        }

        public override void Dispose ()
        {
            UninstallPreferences ();
            base.Dispose ();
        }

        void OnAboutToFinish (object o, Gst.GLib.SignalArgs args)
        {
            // This is needed to make Shuffle-by-* work.
            // Shuffle-by-* uses the LastPlayed field to determine what track in the grouping to play next.
            // Therefore, we need to update this before requesting the next track.
            //
            // This will be overridden by IncrementLastPlayed () called by
            // PlaybackControllerService's EndOfStream handler.
            CurrentTrack.UpdateLastPlayed ();

            next_track_set.Reset ();
            OnEventChanged (PlayerEvent.RequestNextTrack);

            if (!next_track_set.WaitOne (1000, false)) {
                Log.Warning ("[Gapless]: Timed out while waiting for next track to be set.");
                next_track_set.Set ();
            }
        }

        public override void SetNextTrackUri (SafeUri uri, bool maybeVideo)
        {
            if (next_track_set.WaitOne (0, false)) {
                // We've been asked to set the next track, but have taken too
                // long to get here.  Bail for now, and the EoS handling will
                // pick up the pieces.
                return;
            }
            playbin.Uri = uri.AbsoluteUri;
            next_track_set.Set ();
        }

        private bool OnBusMessage (Bus bus, Message msg)
        {
            switch (msg.Type) {
                case MessageType.Eos:
                    StopIterating ();
                    Close (false);
                    OnEventChanged (PlayerEvent.EndOfStream);
                    OnEventChanged (PlayerEvent.RequestNextTrack);
                    break;

                case MessageType.StateChanged:
                    if (msg.Src == playbin) {
                        State old_state, new_state, pending_state;
                        msg.ParseStateChanged (out old_state, out new_state, out pending_state);
                        HandleStateChange (old_state, new_state, pending_state);
                    }
                    break;

                case MessageType.Buffering:
                    int buffer_percent;
                    msg.ParseBuffering (out buffer_percent);
                    HandleBuffering (buffer_percent);
                    break;

                case MessageType.Tag:
                    Pad pad;
                    TagList tag_list;
                    msg.ParseTag (out pad, out tag_list);

                    HandleTag (pad, tag_list);
                    break;

                case MessageType.Error:
                    Enum error_type;
                    string err_msg, debug;
                    msg.ParseError (out error_type, out err_msg, out debug);

                    HandleError (error_type, err_msg, debug);
                    break;

                case MessageType.Element:
                    if (MissingPluginMessage.IsMissingPluginMessage (msg)) {
                        string detail = MissingPluginMessage.GetInstallerDetail (msg);

                        if (detail == null)
                            return false;

                        if (missing_details.Contains (detail)) {
                            Log.DebugFormat ("Ignoring missing element details, already prompted ('{0}')", detail);
                            return false;
                        }

                        Log.DebugFormat ("Saving missing element details ('{0}')", detail);
                        missing_details.Add (detail);

                        Log.Error ("Missing GStreamer Plugin", MissingPluginMessage.GetDescription (msg), true);

                        InstallPluginsContext install_context = new InstallPluginsContext ();
                        Install.InstallPlugins (missing_details.ToArray (), install_context, OnInstallPluginsReturn);
                    } else if (msg.Src == playbin && msg.Structure.Name == "playbin2-stream-changed") {
                        HandleStreamChanged ();
                    }
                    break;
            }

            return true;
        }

        private void OnInstallPluginsReturn (InstallPluginsReturn status)
        {
            Log.InformationFormat ("GStreamer plugin installer returned: {0}", status);
            if (status == InstallPluginsReturn.Success || status == InstallPluginsReturn.InstallInProgress) {
            }
        }

        private void OnVolumeChanged (object o, Gst.GLib.NotifyArgs args)
        {
            OnEventChanged (PlayerEvent.Volume);
        }

        private void HandleStreamChanged ()
        {
            // Set the current track as fully played before signaling EndOfStream.
            ServiceManager.PlayerEngine.IncrementLastPlayed (1.0);
            OnEventChanged (PlayerEvent.EndOfStream);
            OnEventChanged (PlayerEvent.StartOfStream);
        }

        private void HandleError (Enum domain, string error_message, string debug)
        {
            Close (true);

            error_message = error_message ?? Catalog.GetString ("Unknown Error");

            if (domain is ResourceError) {
                ResourceError domain_code = (ResourceError)domain;
                if (CurrentTrack != null) {
                    switch (domain_code) {
                    case ResourceError.NotFound:
                        CurrentTrack.SavePlaybackError (StreamPlaybackError.ResourceNotFound);
                        break;
                    default:
                        break;
                    }
                }
                Log.Error (String.Format ("GStreamer resource error: {0}", domain_code), false);
            } else if (domain is StreamError) {
                StreamError domain_code = (StreamError)domain;
                if (CurrentTrack != null) {
                    switch (domain_code) {
                    case StreamError.CodecNotFound:
                        CurrentTrack.SavePlaybackError (StreamPlaybackError.CodecNotFound);
                        break;
                    default:
                        break;
                    }
                }

                Log.Error (String.Format ("GStreamer stream error: {0}", domain_code), false);
            } else if (domain is CoreError) {
                CoreError domain_code = (CoreError)domain;
                if (CurrentTrack != null) {
                    switch (domain_code) {
                    case CoreError.MissingPlugin:
                        CurrentTrack.SavePlaybackError (StreamPlaybackError.CodecNotFound);
                        break;
                    default:
                        break;
                    }
                }

                if (domain_code != CoreError.MissingPlugin) {
                    Log.Error (String.Format ("GStreamer core error: {0}", (CoreError)domain), false);
                }
            } else if (domain is LibraryError) {
                Log.Error (String.Format ("GStreamer library error: {0}", (LibraryError)domain), false);
            }

            OnEventChanged (new PlayerEventErrorArgs (error_message));
        }

        private void HandleBuffering (int buffer_percent)
        {
            OnEventChanged (new PlayerEventBufferingArgs (buffer_percent / 100.0));
        }

        private void HandleStateChange (State old_state, State new_state, State pending_state)
        {
            StopIterating ();
            if (CurrentState != PlayerState.Loaded && old_state == State.Ready && new_state == State.Paused && pending_state == State.Playing) {
                OnStateChanged (PlayerState.Loaded);
            } else if (old_state == State.Paused && new_state == State.Playing && pending_state == State.VoidPending) {
                if (CurrentState == PlayerState.Loaded) {
                    OnEventChanged (PlayerEvent.StartOfStream);
                }
                OnStateChanged (PlayerState.Playing);
                StartIterating ();
            } else if (CurrentState == PlayerState.Playing && old_state == State.Playing && new_state == State.Paused) {
                OnStateChanged (PlayerState.Paused);
            }
        }

        private void HandleTag (Pad pad, TagList tag_list)
        {
            foreach (string tag in tag_list.Tags) {
                if (String.IsNullOrEmpty (tag)) {
                    continue;
                }

                if (tag_list.GetTagSize (tag) < 1) {
                    continue;
                }

                List tags = tag_list.GetTag (tag);

                foreach (object o in tags) {
                    OnTagFound (new StreamTag () { Name = tag, Value = o });
                }
            }
        }

        private bool OnIterate ()
        {
            // Actual iteration.
            OnEventChanged (PlayerEvent.Iterate);
            // Run forever until we are stopped
            return true;
        }

        private void StartIterating ()
        {
            if (iterate_timeout_id > 0) {
                GLib.Source.Remove (iterate_timeout_id);
                iterate_timeout_id = 0;
            }

            iterate_timeout_id = GLib.Timeout.Add (200, OnIterate);
        }

        private void StopIterating ()
        {
            if (iterate_timeout_id > 0) {
                GLib.Source.Remove (iterate_timeout_id);
                iterate_timeout_id = 0;
            }
        }

        protected override void OpenUri (SafeUri uri, bool maybeVideo)
        {
            if (playbin.CurrentState == State.Playing || playbin.CurrentState == State.Paused) {
                playbin.SetState (Gst.State.Ready);
            }

            playbin.Uri = uri.AbsoluteUri;
        }

        public override void Play ()
        {
            playbin.SetState (Gst.State.Playing);
            OnStateChanged (PlayerState.Playing);
        }

        public override void Pause ()
        {
            playbin.SetState (Gst.State.Paused);
            OnStateChanged (PlayerState.Paused);
        }

        public override void Close (bool fullShutdown)
        {
            playbin.SetState (State.Null);
            base.Close (fullShutdown);
        }

        public override string GetSubtitleDescription (int index)
        {
            return playbin.GetTextTags (index)
             .GetTag (Gst.Tag.LanguageCode)
             .Cast<string> ()
             .FirstOrDefault (t => t != null);
        }

        public override ushort Volume {
            get { return (ushort) Math.Round (audio_sink.Volume * 100.0); }
            set {
                double volume = Math.Min (1.0, Math.Max (0, value / 100.0));
                audio_sink.Volume = volume;
                if (audio_sink.VolumeNeedsSaving) {
                    PlayerEngineService.VolumeSchema.Set (value);
                }
            }
        }

        public override bool CanSeek {
            get { return true; }
        }

        private static Format query_format = Format.Time;
        public override uint Position {
            get {
                long pos;
                playbin.QueryPosition (ref query_format, out pos);
                return (uint) ((ulong)pos / Gst.Clock.MSecond);
            }
            set {
                playbin.Seek (Format.Time, SeekFlags.Accurate, (long)(value * Gst.Clock.MSecond));
            }
        }

        public override uint Length {
            get {
                long duration;
                playbin.QueryDuration (ref query_format, out duration);
                return (uint) ((ulong)duration / Gst.Clock.MSecond);
            }
        }

        private static string [] source_capabilities = { "file", "http", "cdda" };
        public override IEnumerable SourceCapabilities {
            get { return source_capabilities; }
        }

        private static string [] decoder_capabilities = { "ogg", "wma", "asf", "flac", "mp3", "" };
        public override IEnumerable ExplicitDecoderCapabilities {
            get { return decoder_capabilities; }
        }

        public override string Id {
            get { return "gstreamer-sharp"; }
        }

        public override string Name {
            get { return Catalog.GetString ("GStreamer# 0.10"); }
        }

        public override bool SupportsEqualizer {
            get { return false; }
        }

        public override VideoDisplayContextType VideoDisplayContextType {
            get { return VideoDisplayContextType.Unsupported; }
        }

        public override int SubtitleCount {
            get { return playbin.NText; }
        }

        public override int SubtitleIndex {
            set {
                if (value >= 0 && value < SubtitleCount) {
                    playbin.CurrentText = value;
                }
            }
        }

        public override SafeUri SubtitleUri {
            set { playbin.Suburi = value.AbsoluteUri; }
            get { return new SafeUri (playbin.Suburi); }
        }

        private PreferenceBase replaygain_preference;

        private void InstallPreferences ()
        {
            PreferenceService service = ServiceManager.Get<PreferenceService> ();
            if (service == null) {
                return;
            }

            replaygain_preference = service["general"]["misc"].Add (new SchemaPreference<bool> (ReplayGainEnabledSchema,
                Catalog.GetString ("_Enable ReplayGain correction"),
                Catalog.GetString ("For tracks that have ReplayGain data, automatically scale (normalize) playback volume"),
                delegate { audio_sink.ReplayGainEnabled = ReplayGainEnabledSchema.Get (); }
            ));
        }

        private void UninstallPreferences ()
        {
            PreferenceService service = ServiceManager.Get<PreferenceService> ();
            if (service == null) {
                return;
            }

            service["general"]["misc"].Remove (replaygain_preference);
            replaygain_preference = null;
        }

        public static readonly SchemaEntry<bool> ReplayGainEnabledSchema = new SchemaEntry<bool> (
            "player_engine", "replay_gain_enabled",
            false,
            "Enable ReplayGain",
            "If ReplayGain data is present on tracks when playing, allow volume scaling"
        );

    }
}
