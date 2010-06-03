//
// System.IO.IsolatedStorage.IsolatedQuotaGroup
//
// Authors
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if MOONLIGHT

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace System.IO.IsolatedStorage {

	internal static class IsolatedStorage {

		// NOTE: both the 'site' and 'application' share the same quota
		internal const long DefaultQuota = 1024 * 1024;
		// Since we can extend more than AvailableFreeSize we need to substract the "safety" value out of it
		private const int SafetyZone = 1024;


		static string site_root;
		static string site_config;
		static long site_quota;

		// this is similar to Silverlight hierarchy because differing too much would cause
		// problems with the 260 character maximum allowed for paths

		// Considering a 10 characters user name the following platform will allow:
		// 109 characters path, under Windows Vista (Silverlight 2)
		// 77 characters path, under Windows XP (Silverlight 2)
		// 100 characters path, under Mac OSX (Silverlight 2)
		// 159 characters path, under Linux (Moonlight 2)

		// 1234567890123456789012345678901234567890123	= 43 + 28 + 1 + 28 +1 = 101
		//          1         2         3         4   
		// /home/1234567890/.local/share/moonlight/is/{site-hash:28}/{app-hash:28}/

		static IsolatedStorage ()
		{
			string isolated_root = GetIsolatedStorageRoot ();
			// enable/disable osilated storage - requires restart
			Enabled = !File.Exists (Path.Combine (isolated_root, "disabled"));

			// from System.Windows.Application we made "xap_uri" correspond to
			//	 Application.Current.Host.Source.AbsoluteUri
			string app = (AppDomain.CurrentDomain.GetData ("xap_uri") as string);
			if (app.StartsWith ("file://")) {
				// every path is a different site, the XAP is the application
				Site = Path.GetDirectoryName (app.Substring (7));
			} else {
				// for http[s] the "Site Identity" is built using:
				// * the protocol (so http and https are different);
				// * the host (so beta.moonlight.com is different from www.moonlight.com); and
				// * the port (so 8080 is different from 80) but
				//	** 80 and none are identical for HTTP
				//	** 443 and none are identical for HTTPS
				Site = app.Substring (0, app.IndexOf ('/', 8));
			}

			// the "Site Identity"
			string site_hash = Hash (Site);
			site_root = TryDirectory (Path.Combine (isolated_root, site_hash));
			SetupSite (site_root);
			SitePath = TryDirectory (Path.Combine (site_root, site_hash));

			// the "Application Identity"
			string app_hash = Hash (app);
			SetupApplication (app, app_hash, site_root);
			ApplicationPath = TryDirectory (Path.Combine (site_root, app_hash));
		}

		static string GetIsolatedStorageRoot ()
		{
			// http://freedesktop.org/Standards/basedir-spec/basedir-spec-0.6.html
                        string xdg_data_home = Environment.GetEnvironmentVariable ("XDG_DATA_HOME");
                        if (String.IsNullOrEmpty (xdg_data_home)) {
                                xdg_data_home = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
                        }

			string moonlight = TryDirectory (Path.Combine (xdg_data_home, "moonlight"));
			return TryDirectory (Path.Combine (moonlight, "is"));
		}

		static void SetupSite (string dir)
		{
			site_quota = DefaultQuota;
			// read configuration file (e.g. quota) if it exists, otherwise write it
			site_config = Path.Combine (dir, "config");
			if (File.Exists (site_config)) {
				LoadConfiguration ();
			} else {
				SaveConfiguration ();
			}
		}

		static void LoadConfiguration ()
		{
			// read quota, the rest is not useful to us
			using (StreamReader sr = new StreamReader (site_config)) {
				string line = sr.ReadLine ();
				while (line != null) {
					if (line.StartsWith ("QUOTA = ")) {
						if (!Int64.TryParse (line.Substring (8, line.Length - 8), out site_quota))
							Quota = DefaultQuota;
					}
					line = sr.ReadLine ();
				}
			}
		}

		static void SaveConfiguration ()
		{
			using (StreamWriter sw = new StreamWriter (site_config)) {
				sw.WriteLine ("URI = {0}", Site);
				sw.WriteLine ("QUOTA = {0}", Quota);
			}
		}

		static void SetupApplication (string app, string app_hash, string dir)
		{
			// save the application information (for the management UI)
			string config = Path.Combine (dir, app_hash + ".info");
			if (File.Exists (config))
				return;

			using (StreamWriter sw = new StreamWriter (config)) {
				sw.WriteLine ("URI = {0}", app);
			}
		}

		// goal: uniform length directory name
		// non-goal: security by obsfucation
		static string Hash (string name)
		{
			string id;
			using (SHA1Managed hash = new SHA1Managed ()) {
				byte[] digest = hash.ComputeHash (Encoding.UTF8.GetBytes (name));
				id = Convert.ToBase64String (digest);
			}
			return (id.IndexOf ('/') == -1) ? id : id.Replace ('/', '-');
		}

		static string TryDirectory (string path)
		{
			try {
				Directory.CreateDirectory (path);
				return path;
			} catch {
				return null;
			}
		}

		static internal void Remove (string dir)
		{
			try {
				Directory.Delete (dir, true);
			}
			finally {
				TryDirectory (dir);
			}
		}

		static internal bool CanExtend (long request)
		{
			return (request <= AvailableFreeSpace + SafetyZone);
		}

		static public string ApplicationPath {
			get; private set;
		}

		static public string SitePath {
			get; private set;
		}

		static public long AvailableFreeSpace {
			get { return Quota - Current - SafetyZone; }
		}

		[DllImport ("moon")]
		extern static long isolated_storage_get_current_usage (string root);

		static public long Current {
			get { return isolated_storage_get_current_usage (site_root); }
		}

		static public long Quota { 
			get { return site_quota; }
			set {
				site_quota = value;
				SaveConfiguration ();
			}
		}

		static public string Site {
			get; private set;
		}

		// it is possible, from the UI, to completely disable IsolatedStorage
		static public bool Enabled { get; private set; }
	}
}

#endif

