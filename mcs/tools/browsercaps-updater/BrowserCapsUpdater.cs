//
// BrowserCapsUpdater.cs: updates $prefix/etc/mono/browscap.ini file.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace Mono.ASPNET
{
	class Driver
	{
		static string GetFilePath ()
		{
			Type type = typeof (Environment);
			MethodInfo method = type.GetMethod ("GetMachineConfigPath", BindingFlags.Static |
										    BindingFlags.NonPublic);

			if (method == null) {
				Console.WriteLine ("You need to run this under Mono runtime");
				return null;
			}

			string path = (string) method.Invoke (null, null);
			return Path.Combine (Path.GetDirectoryName (path), "browscap.ini");
		}
		
		static int Main (string [] args)
		{
			string path = GetFilePath ();
			if (path == null)
				return 1;

			Updater updater = new Updater (path);

			if (File.Exists (path)) {
				bool uptodate;
				Console.WriteLine (updater.GetLocalMessage (out uptodate));
				if (uptodate)
					return 0;

				Console.WriteLine ("WARNING: your site may be blocked from updating if you abuse.");
				Console.WriteLine ("You're encouraged to browse and understand " +
						   "http://browsers.GaryKeith.com/");

				string r = "NO";
				while (r != "YES") {
					Console.Write ("Do you want to update your file now? (yes/NO) ");
					r = Console.ReadLine ();
					if (r == null)
						r = "NO";
					else
						r = r.ToUpper ();

					if (r == "NO")
						return 0;
				}
			}

			try {
				updater.Update ();
				Console.WriteLine ("browscap.ini file provided by Gary J. Keith.");
			} catch (Exception e) {
				Console.Error.WriteLine ("Update failed.");
				Console.Error.WriteLine ("Reason: {0}", e.Message);
				return 1;
			}
			
			return 0;
		}
	}

	class Updater
	{
		static string VersionUrl = "http://browsers.garykeith.com/version-number.asp";
		static string BrowscapUrl = "http://browsers.garykeith.com/stream.asp?BrowsCapINI";
		static string UserAgent = "Mono Browser Capabilities Updater 0.1";

		string filename;
		string tempfile;
		string local;
		int nupdates;
		string date;
		public Updater (string filename)
		{
			this.filename = filename;
		}

		public string GetLocalMessage (out bool upToDate)
		{
			StringBuilder sb = new StringBuilder ();
			string localdate = null;
			string remotedate = null;
			using (StreamReader reader = new StreamReader (File.OpenRead (filename))) {
				string str;
				while ((str = reader.ReadLine ()) != null) {
					if (str.StartsWith ("version="))
						break;
				}

				if (str != null) {
					localdate = str.Substring (8);
					sb.AppendFormat ("Your file is dated {0}\r\n", localdate);
				} else {
					sb.AppendFormat ("Couldn't retrieve date from {0}\r\n", filename);
				}
			}

			remotedate = GetRemoteDate ();
			remotedate = remotedate.Replace ("AM ", "");
			remotedate = remotedate.Replace ("PM ", "");
			remotedate = remotedate.Replace ("/", "-");
			sb.AppendFormat ("Remote file is dated {0}", remotedate);
			upToDate = (remotedate == localdate);
			if (upToDate)
				sb.Append ("\r\nThe file IS up to date.");

			return sb.ToString ();
		}

		static string GetRemoteDate ()
		{
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create (VersionUrl);
			request.UserAgent = UserAgent;
			WebResponse resp = request.GetResponse ();
			string str = new StreamReader (resp.GetResponseStream ()).ReadLine ();
			resp.Close ();
			return str;
		}

		public void Update ()
		{
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create (BrowscapUrl);
			request.UserAgent = UserAgent;
			StreamWriter writer = null;
			StreamReader reader = null;
			Console.Write ("Connecting...");
			WebResponse resp = request.GetResponse ();
			string tmppath = null;
			try {
				tmppath = Path.GetTempFileName ();
				reader = new StreamReader (resp.GetResponseStream ());
				Console.WriteLine (" done");
				writer = new StreamWriter (File.OpenWrite (tmppath));
				Console.WriteLine ("Downloading data to {0}", tmppath);
				string str;
				while ((str = reader.ReadLine ()) != null) {
					writer.WriteLine (str);
				}

				writer.Close ();
				writer = null;
				reader.Close ();
				reader = null;

				Console.WriteLine ("Removing old {0}", filename);
				File.Delete (filename);
				Console.WriteLine ("Copying {0} to {1}", tmppath, filename);
				File.Copy (tmppath, filename);
			} finally {
				try {
					File.Delete (tmppath);
				} catch {}

				if (writer != null) {
					try {
						writer.Close ();
					} catch {}
				}

				if (reader != null) {
					try {
						reader.Close ();
					} catch {}
				}
			}
		}
	}
}

