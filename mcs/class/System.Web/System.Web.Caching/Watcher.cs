// 
// System.Web.Caching.Watcher
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// 
// Use this until we have a FileSystemWatcher...

using System;
using System.Collections;
using System.IO;
using System.Threading;

namespace System.Web.Caching
{
	class Watcher
	{
		static Hashtable watches;
		static Watcher worker;

		bool running;
		Thread t;

		private Watcher ()
		{
			watches = new Hashtable ();
			t = new Thread (new ThreadStart (Run));
			t.IsBackground = true;
			t.Start ();
		}

		static int CalculateSleep ()
		{
			if (watches == null || watches.Count == 0)
				return 5000;

			int result = (int) Math.Pow (10, Math.Log (watches.Count, 10) + 1.75);
			return (result < 500) ? 500 : result;
		}
		
		void Run ()
		{
			ArrayList notified = new ArrayList ();
			while (true) {
				lock (watches) {
					foreach (string key in watches.Keys) {
						Watch w = (Watch) watches [key];
						if (!w.CheckIfChanged ())
							continue;

						w.ChangedEvent (this, EventArgs.Empty);
						notified.Add (key);
					}

					foreach (string s in notified)
						watches.Remove (s);

					notified.Clear ();
				}
				Thread.Sleep (CalculateSleep ());
			}
		}

		static void EnsureWorker ()
		{
			if (worker == null)
				worker = new Watcher ();
		}

		static public void AddWatch (string filename, EventHandler eh)
		{
			EnsureWorker ();
			lock (watches) {
				if (!watches.Contains (filename)) {
					watches.Add (filename, new Watch (filename, eh));
				} else {
					((Watch) watches [filename]).AddEvent (eh);
				}
			}
		}

		class Watch
		{
			string filename;
			DateTime begin;
			EventHandler eh;
			bool is_dir;
			bool changed;
			IsThere exists;
			GetTime getTime;

			static IsThere fileExists;
			static GetTime fileGetTime;
			static IsThere dirExists;
			static GetTime dirGetTime;

			delegate bool IsThere (string filename);
			delegate DateTime GetTime (string filename);

			static Watch ()
			{
				fileExists = new IsThere (File.Exists);
				fileGetTime = new GetTime (File.GetLastWriteTime);
				dirExists = new IsThere (Directory.Exists);
				dirGetTime = new GetTime (Directory.GetLastWriteTime);
			}

			public Watch (string filename, EventHandler eh)
			{
				this.eh = eh;
				this.filename = filename;
				is_dir = Directory.Exists (filename);
				changed = !(is_dir || File.Exists (filename));
				if (is_dir) {
					exists = dirExists;
					getTime = dirGetTime;
				} else {
					exists = fileExists;
					getTime = fileGetTime;
				}
				if (!changed)
					begin = getTime (filename);
			}

			public void AddEvent (EventHandler eh)
			{
				this.eh += eh;
			}
			
			public bool CheckIfChanged ()
			{
				if (changed)
					return true;
				
				if (!exists (filename)) {
					changed = true;
					return true;
				}
					
				DateTime current = getTime (filename);
				changed = current > begin;
				return changed;
			}

			public EventHandler ChangedEvent {
				get { return eh; }
			}
		}
	}
}

