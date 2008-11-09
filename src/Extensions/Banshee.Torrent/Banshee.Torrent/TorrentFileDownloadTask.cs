//
// TorrentFileDownloadTask.cs
//
// Author:
//   Alan McGovern <alan.mcgovern@gmail.com>
//
// Copyright (C) 2008 Alan McGovern
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
using System.IO;
using Migo.TaskCore;
using MonoTorrent.DBus;

namespace Banshee.Torrent
{
	public class TorrentFileDownloadTask : Migo.DownloadCore.HttpFileDownloadTask
	{
		private MonoTorrent.DBus.IDownloader downloader;
		private MonoTorrent.DBus.ITorrent torrent;
		
		public TorrentFileDownloadTask(string remoteUri, string localPath, object userState)
			: base (remoteUri, localPath.Substring(0, localPath.Length - 8), userState)
		{
			Console.WriteLine ("Torrent file download task!");
		}
		
		public override long BytesReceived {
			get 
			{
				if (downloader == null)
                    return 0;
                
				return (long)(downloader.GetProgress () / 100.0 * torrent.GetSize ()); 
			}
		}
		
		public override void CancelAsync ()
		{
			if (downloader == null)
                return;
            
			downloader.Stop ();
			SetStatus (TaskStatus.Cancelled);
            OnTaskCompleted (null, true);
		}

		public override void ExecuteAsync ()
		{
			Console.WriteLine ("Running!");
			SetStatus (TaskStatus.Running);
			TorrentService s = (TorrentService) Banshee.ServiceStack.ServiceManager.Get<TorrentService>("TorrentService");
			downloader = s.Download (this.RemoteUri.ToString(), Path.GetDirectoryName(this.LocalPath));
			torrent = TorrentService.Bus.GetObject <ITorrent> (TorrentService.BusName, this.downloader.GetTorrent ());
			
			this.downloader.StateChanged += delegate {
				if (downloader.GetState () == TorrentState.Seeding)
				{
					Console.WriteLine("Progress");
					SetProgress(100);
					SetStatus (TaskStatus.Succeeded);
					OnTaskCompleted (null, false);
				}
			};
			
			// There are no events on the torrent IDownloader to indicate when the stats have updated
			// I need to manually ping the SetProgress event otherwise migo never notices progress changing
			System.Threading.ThreadPool.QueueUserWorkItem ((System.Threading.WaitCallback)delegate {
				while (base.Progress != 100 && 
				       (Status == TaskStatus.Running || Status == TaskStatus.Paused || Status == TaskStatus.Running))
				{
					
					Console.WriteLine("Progress");
					Hyena.Log.Debug ("Torrent Tick");
					System.Threading.Thread.Sleep (2000);
					SetProgress((int)downloader.GetProgress ());
				}
			});
		}

		public override void Pause ()
		{
			if (downloader == null)
                return;
            
			SetStatus (TaskStatus.Paused);
			downloader.Pause ();
		}
		
		public override void Resume ()
		{
			if (downloader == null)
                return;
            
			SetStatus (TaskStatus.Running);
			downloader.Stop ();
		}

		public override void Stop ()
		{
			if (downloader == null)
                return;
            
			SetStatus (TaskStatus.Stopped);
			OnTaskCompleted (null, false);
			downloader.Stop ();
		}
	}
}
