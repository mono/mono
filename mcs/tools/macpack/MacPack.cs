using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

using Mono.GetOptions;

namespace Mac {

	public class PackOptions : Options {
		[Option ("Application Name", 'n')]
		public string appname;

		[Option ("Output location", 'o')]
		public string output;
		
		[Option ("Assembly Location", 'a')]
		public string assembly;
		
		[Option (999, "Resources", 'r')]
		public string[] resource;

		[Option ("Mode", 'm')]
		public int mode;
	}
	public class Pack {

		private PackOptions opts;

		public Pack () {}

		public Pack (PackOptions opts) {
			this.opts = opts;
		}

		public bool Generate () {
			if (opts.output == null || opts.assembly == null || opts.appname == null) {
				opts.DoHelp ();
				return false;
			}
			if (Directory.Exists (Path.Combine (opts.output, String.Format ("{0}.app", opts.appname)))) {
				Console.WriteLine ("ERROR: That application already exists.  Please delete it first");
				return false;
			}
			Directory.CreateDirectory (Path.Combine (opts.output, String.Format ("{0}.app", opts.appname)));
			Directory.CreateDirectory (Path.Combine (opts.output, String.Format ("{0}.app/Contents", opts.appname)));
			Directory.CreateDirectory (Path.Combine (opts.output, String.Format ("{0}.app/Contents/MacOS", opts.appname)));
			Directory.CreateDirectory (Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources", opts.appname)));
			if (opts.resource != null) {
				foreach (string res in opts.resource) { 
					if (Directory.Exists (res)) {
						CopyDirectory (res, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{1}", opts.appname, Path.GetFileName (res))));
					} else {
						File.Copy (res, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{1}", opts.appname, Path.GetFileName (res))));
					}
				}
			}
			if (opts.mode <= 2) {
				File.Copy (opts.assembly, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{0}.exe", opts.appname))); 
			} else {
				File.Copy (opts.assembly, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{0}", opts.appname))); 
			}

			Stream s = Assembly.GetEntryAssembly ().GetManifestResourceStream ("LOADER");
			BinaryReader reader = new BinaryReader (s);
			byte[] data = reader.ReadBytes ((int)s.Length);
			reader.Close ();
			BinaryWriter writer = new BinaryWriter (File.Create (Path.Combine (opts.output, String.Format ("{0}.app/Contents/MacOS/{0}", opts.appname))));
			string script = Encoding.ASCII.GetString (data);
			switch (opts.mode) {
				default:
				case 0:
					script = script.Replace ("%MWF_MODE%", "0");
					script = script.Replace ("%COCOASHARP_MODE%", "0");
					script = script.Replace ("%X11_MODE%", "0");
					break;
				case 1:
					script = script.Replace ("%MWF_MODE%", "1");
					script = script.Replace ("%COCOASHARP_MODE%", "0");
					script = script.Replace ("%X11_MODE%", "0");
					break;
				case 2:
					script = script.Replace ("%MWF_MODE%", "0");
					script = script.Replace ("%COCOASHARP_MODE%", "1");
					script = script.Replace ("%X11_MODE%", "0");
					break;
				case 3:
					script = script.Replace ("%MWF_MODE%", "0");
					script = script.Replace ("%COCOASHARP_MODE%", "0");
					script = script.Replace ("%X11_MODE%", "1");
					break;
			}
			data = Encoding.ASCII.GetBytes (script);
			writer.Write (data, 0, data.Length);
			writer.Close ();
			chmod (Path.Combine (opts.output, String.Format ("{0}.app/Contents/MacOS/{0}", opts.appname)), Convert.ToUInt32 ("755", 8));

			s = Assembly.GetEntryAssembly ().GetManifestResourceStream ("PLIST");
			reader = new BinaryReader (s);
			data = reader.ReadBytes ((int)s.Length);
			reader.Close ();
			writer = new BinaryWriter (File.Create (Path.Combine (opts.output, String.Format ("{0}.app/Contents/Info.plist", opts.appname))));
			string plist = Encoding.UTF8.GetString (data);
			plist = plist.Replace ("%APPNAME%", opts.appname);
			data = Encoding.UTF8.GetBytes (plist);
			writer.Write (data, 0, data.Length);
			writer.Close ();

			return true;
		}

		public static void CopyDirectory (string src, string dest) {
			string [] files;

			if (dest [dest.Length-1] != Path.DirectorySeparatorChar) {
				dest += Path.DirectorySeparatorChar;
			}

			if (!Directory.Exists (dest)) {
				Directory.CreateDirectory (dest);
			}

			files = Directory.GetFileSystemEntries (src);
			
			foreach (string file in files) {
				if (Directory.Exists (file)) {
					CopyDirectory (file, dest + Path.GetFileName (file));
				} else {
					File.Copy (file, dest + Path.GetFileName (file), true);
				}
			}
		}

		static int Main (string [] args) {
			PackOptions options = new PackOptions ();
			options.ProcessArgs (args);
			Pack p = new Pack (options);
			if (p.Generate ())
				return 0;
			return -1;
		}
	
		[DllImport ("libc")]
		static extern int chmod (string path, uint mode);
	}
}
