//
// CompositeTrackSourceContents.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
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
using System.Reflection;
using System.Collections.Generic;

using Gtk;
using Mono.Unix;

using Hyena.Data;
using Hyena.Data.Gui;
using Hyena.Widgets;

using Banshee.Sources;
using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Collection.Database;
using Banshee.Configuration;
using Banshee.Gui;
using Banshee.Collection.Gui;

using ScrolledWindow=Gtk.ScrolledWindow;

namespace Banshee.Sources.Gui
{
    public class CompositeTrackSourceContents : FilteredListSourceContents, ITrackModelSourceContents
    {
        private QueryFilterView<string> genre_view;
        private ArtistListView artist_view;
        private ArtistListView albumartist_view;
        private AlbumListView album_view;
        private TrackListView track_view;

        private InterfaceActionService action_service;
        private ActionGroup configure_browser_actions;

        private static string menu_xml = @"
            <ui>
              <menubar name=""MainMenu"">
                <menu name=""ViewMenu"" action=""ViewMenuAction"">
                  <placeholder name=""BrowserViews"">
                    <menu name=""BrowserListsMenu"" action=""BrowserListsMenuAction"">
                        <menuitem name=""Artist"" action=""ArtistAction"" />
                        <menuitem name=""AlbumArtist"" action=""AlbumArtistAction"" />
                        <separator />
                        <menuitem name=""Genre"" action=""GenreAction"" />
                    </menu>
                    <separator />
                  </placeholder>
                </menu>
              </menubar>
            </ui>
        ";

        public CompositeTrackSourceContents () : base ("albumartist")
        {
            if (ServiceManager.Contains ("InterfaceActionService")) {
                action_service = ServiceManager.Get<InterfaceActionService> ();

                if (action_service.FindActionGroup ("BrowserConfiguration") == null) {
                    configure_browser_actions = new ActionGroup ("BrowserConfiguration");

                    configure_browser_actions.Add (new ActionEntry [] {
                        new ActionEntry ("BrowserListsMenuAction", null,
                            Catalog.GetString ("Configure Browser"), null,
                            Catalog.GetString ("Configure the filters available in the browser"), null)
                    });

                    configure_browser_actions.Add (new RadioActionEntry [] {
                        new RadioActionEntry ("ArtistAction", null,
                            Catalog.GetString ("Use all available artists"), null,
                            Catalog.GetString ("Use all available artists in the browser filter list"), 0),

                        new RadioActionEntry ("AlbumArtistAction", null,
                            Catalog.GetString ("Use album artists only"), null,
                            Catalog.GetString ("Use only album artists, not the ones with only single tracks"), 1),
                    }, ArtistListViewType.Get ().Equals ("artist") ? 0 : 1 , null);

                    configure_browser_actions.Add (new ToggleActionEntry [] {
                        new ToggleActionEntry ("GenreAction", null,
                            Catalog.GetString ("Show Genre filter"), null,
                            Catalog.GetString ("Show a list of genres to filter by"), null, GenreListShown.Get ())});

                    action_service.AddActionGroup (configure_browser_actions);
                    action_service.UIManager.AddUiFromString (menu_xml);
                }

                (action_service.FindAction("BrowserConfiguration.ArtistAction") as RadioAction).Changed += OnArtistFilterChanged;
                //(action_service.FindAction("BrowserConfiguration.AlbumArtistAction") as RadioAction).Changed += OnArtistFilterChanged;
                action_service.FindAction("BrowserConfiguration.GenreAction").Activated += OnGenreFilterChanged;;
            }
        }

        private void OnGenreFilterChanged (object o, EventArgs args)
        {
            ToggleAction action = (ToggleAction)o;

            ClearFilterSelections ();

            GenreListShown.Set (action.Active);

            Widget genre_view_widget = (Widget)genre_view;
            genre_view_widget.Parent.Visible = GenreListShown.Get ();
        }

        private void OnArtistFilterChanged (object o, ChangedArgs args)
        {
            Widget new_artist_view = args.Current.Value == 0 ? artist_view : albumartist_view;
            Widget old_artist_view = args.Current.Value == 1 ? artist_view : albumartist_view;

            List<ScrolledWindow> new_filter_list = new List<ScrolledWindow> ();
            List<ScrolledWindow> old_filter_list = new List<ScrolledWindow> (filter_scrolled_windows);
            foreach (ScrolledWindow fw in old_filter_list)
            {
                bool contains = false;
                foreach (Widget child in fw.AllChildren)
                    if (child == old_artist_view)
                        contains = true;
                if (contains)
                {
                    Widget view_widget = (Widget)new_artist_view;
                    if (view_widget.Parent == null)
                            SetupFilterView (new_artist_view as ArtistListView);

                    ScrolledWindow win = (ScrolledWindow)view_widget.Parent;

                    new_filter_list.Add (win);
                } else
                    new_filter_list.Add (fw);
            }

            filter_scrolled_windows = new_filter_list;

            ClearFilterSelections ();

            if (BrowserPosition.Get ().Equals ("left")) {
                LayoutLeft ();
            } else {
                LayoutTop ();
            }

            ArtistListViewType.Set (args.Current.Value == 1 ? "albumartist" : "artist");
        }

        protected override void InitializeViews ()
        {
            SetupMainView (track_view = new TrackListView ());

            SetupFilterView (genre_view = new QueryFilterView<string> (Catalog.GetString ("Not Set")));
            Widget genre_view_widget = (Widget)genre_view;
            genre_view_widget.Parent.Shown += delegate {
                genre_view_widget.Parent.Visible = GenreListShown.Get ();
            };

            if (ArtistListViewType.Get ().Equals ("artist")) {
                SetupFilterView (artist_view = new ArtistListView ());
                albumartist_view = new ArtistListView ();
            } else {
                SetupFilterView (albumartist_view = new ArtistListView ());
                artist_view = new ArtistListView ();
            }

            SetupFilterView (album_view = new AlbumListView ());
        }

        protected override void ClearFilterSelections ()
        {
            if (genre_view.Model != null) {
                genre_view.Selection.Clear ();
            }

            if (artist_view.Model != null) {
                artist_view.Selection.Clear ();
            }

            if (albumartist_view.Model != null) {
                albumartist_view.Selection.Clear ();
            }

            if (album_view.Model != null) {
                album_view.Selection.Clear ();
            }
        }

        public void SetModels (TrackListModel track, IListModel<ArtistInfo> artist, IListModel<AlbumInfo> album, IListModel<QueryFilterInfo<string>> genre)
        {
            SetModel (track);
            SetModel (artist);
            SetModel (album);
            SetModel (genre);
        }

        IListView<TrackInfo> ITrackModelSourceContents.TrackView {
            get { return track_view; }
        }

        public TrackListView TrackView {
            get { return track_view; }
        }

        public TrackListModel TrackModel {
            get { return (TrackListModel)track_view.Model; }
        }

        protected override bool ActiveSourceCanHasBrowser {
            get {
                if (!(ServiceManager.SourceManager.ActiveSource is ITrackModelSource)) {
                    return false;
                }

                return ((ITrackModelSource)ServiceManager.SourceManager.ActiveSource).ShowBrowser;
            }
        }

#region Implement ISourceContents

        public override bool SetSource (ISource source)
        {
            ITrackModelSource track_source = source as ITrackModelSource;
            IFilterableSource filterable_source = source as IFilterableSource;
            if (track_source == null) {
                return false;
            }

            this.source = source;

            SetModel (track_view, track_source.TrackModel);

            if (filterable_source != null && filterable_source.CurrentFilters != null) {
                foreach (IListModel model in filterable_source.CurrentFilters) {
                    if (model is IListModel<ArtistInfo> && model is DatabaseArtistListModel)
                        SetModel (artist_view, (model as IListModel<ArtistInfo>));
                    else if (model is IListModel<ArtistInfo> && model is DatabaseAlbumArtistListModel)
                        SetModel (albumartist_view, (model as IListModel<ArtistInfo>));
                    else if (model is IListModel<AlbumInfo>)
                        SetModel (album_view, (model as IListModel<AlbumInfo>));
                    else if (model is IListModel<QueryFilterInfo<string>>)
                        SetModel (genre_view, (model as IListModel<QueryFilterInfo<string>>));
                    // else
                    //    Hyena.Log.DebugFormat ("CompositeTrackSourceContents got non-album/artist filter model: {0}", model);
                }
            }

            track_view.HeaderVisible = true;
            return true;
        }

        public override void ResetSource ()
        {
            source = null;
            SetModel (track_view, null);
            SetModel (artist_view, null);
            SetModel (albumartist_view, null);
            SetModel (album_view, null);
            SetModel (genre_view, null);
            track_view.HeaderVisible = false;
        }

#endregion

        public static readonly SchemaEntry<string> ArtistListViewType = new SchemaEntry<string> (
            "artist_list_view", "type",
            "artist",
            "Artist/AlbumArtist List View Type",
            "The type of the Artist/AlbumArtist list view; either 'artist' or 'albumartist'"
        );

        public static readonly SchemaEntry<bool> GenreListShown = new SchemaEntry<bool> (
            "genre_list_view", "shown",
            false,
            "GenreListView Shown",
            "Define if the GenreList filter view is shown or not"
        );
    }
}
