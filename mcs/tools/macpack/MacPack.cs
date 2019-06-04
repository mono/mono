//
// Author: Geoff Norton
// de-MonoOptionification: miguel.
//
// Copyright (C) 2004-2005 Geoff Norton.
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
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mac {

	public class PackOptions {
		public string appname;
		public string output;
		public string assembly;
		public string icon;
		public string[] resource;
		public int mode;
	}
	public class Pack {

		private PackOptions opts;

		public Pack () {}

		public Pack (PackOptions opts) {
			this.opts = opts;
		}

		public bool Generate () {
			if (opts.output == null){
				opts.output = ".";
			}
			
			if (opts.assembly == null){
				Console.Error.WriteLine ("Error: No assembly to macpack was specified");
				Usage ();
				return false;
			}

			if (opts.appname == null){
				string t = Path.ChangeExtension (opts.assembly, null);
				int p = t.IndexOf (Path.DirectorySeparatorChar);
				if (p != -1)
					t = t.Substring (p+1);
				
				opts.appname = t;
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
					try {
						if (Directory.Exists (res)) {
							CopyDirectory (res, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{1}", opts.appname, Path.GetFileName (res))));
						} else {
							File.Copy (res, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{1}", opts.appname, Path.GetFileName (res))));
						}
					} catch  (Exception e){
						Console.Error.WriteLine ("Error while processing {0} (Details: {1})", res, e.GetType ());
					}
				}
			}
			if (opts.icon != null)
				File.Copy (opts.icon, Path.Combine (opts.output, String.Format ("{0}.app/Contents/Resources/{1}", opts.appname, Path.GetFileName (opts.icon))));
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
					script = script.Replace ("%MONO_ARGS%", "");
					break;
				case 1:
					script = script.Replace ("%MWF_MODE%", "1");
					script = script.Replace ("%COCOASHARP_MODE%", "0");
					script = script.Replace ("%X11_MODE%", "0");
					// no WinForms support in 64-bit Mono - pass --arch=32 to the mono executable
					// see XplatUICarbon.Initialize(): 
					//     WARNING: The Carbon driver has not been ported to 64bits, and very few parts 
					//     of Windows.Forms will work properly, or at all
					script = script.Replace ("%MONO_ARGS%", "--arch=32");

					break;
				case 2:
					script = script.Replace ("%MWF_MODE%", "0");
					script = script.Replace ("%COCOASHARP_MODE%", "1");
					script = script.Replace ("%X11_MODE%", "0");
					script = script.Replace ("%MONO_ARGS%", "");
					break;
				case 3:
					script = script.Replace ("%MWF_MODE%", "0");
					script = script.Replace ("%COCOASHARP_MODE%", "0");
					script = script.Replace ("%X11_MODE%", "1");
					script = script.Replace ("%MONO_ARGS%", "");
					break;
			}

			data = Encoding.ASCII.GetBytes (script);
			writer.Write (data, 0, data.Length);
			writer.Close ();
			try {
				chmod (Path.Combine (opts.output,
						     String.Format ("{0}.app/Contents/MacOS/{0}", opts.appname)),
				       Convert.ToUInt32 ("755", 8));
			} catch {
				Console.WriteLine ("WARNING: It was not possible to set the executable permissions on\n" +
						   "the file {0}.app/Contents/MacOS/{0}, the bundle might not work", opts.appname);
			}

			s = Assembly.GetEntryAssembly ().GetManifestResourceStream ("PLIST");
			reader = new BinaryReader (s);
			data = reader.ReadBytes ((int)s.Length);
			reader.Close ();
			writer = new BinaryWriter (File.Create (Path.Combine (opts.output, String.Format ("{0}.app/Contents/Info.plist", opts.appname))));
			string plist = Encoding.UTF8.GetString (data);
			plist = plist.Replace ("%APPNAME%", opts.appname);
			plist = plist.Replace ("%ICONFILE%", Path.GetFileName (opts.icon));
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

		static void Usage ()
		{
			Console.WriteLine ("\n" + 
					   "Usage is:\n" +
					   "macpack [options] assembly\n" +
					   "   -n appname  -appname:appname    Application Name\n" +
					   "   -o output   -output:OUTPUT      Output directory\n" +
					   "   -a assembly                     Assembly to pack\n" +
					   "   -i file     -icon file          Icon filename\n" +
					   "   -r resource1,resource2          Additional files to bundle\n" +
					   "   -m [winforms|cocoa|x11|console] The mode for the application");
		}
		
		static int Main (string [] args) {
			PackOptions options = new PackOptions ();
			ArrayList resources = new ArrayList ();
			
			for (int i = 0; i < args.Length; i++){
				string s = args [i];
				string key, value;
				
				if (s.Length > 2){
					int p = s.IndexOf (':');
					if (p != -1){
						key = s.Substring (0, p);
						value = s.Substring (p + 1);
					} else {
						key = s;
						value = null;
					}
				} else {
					key = s;
					if (i+1 < args.Length)
						value = args [i+1];
					else
						value = null;
				}

				switch (key){
				case "-n": case "-appname":
					options.appname = value;
					break;
				case "-o": case "-output":
					options.output = value;
					break;
				case "-a": case "-assembly":
					options.assembly = value;
					break;
				case "-i": case "-icon":
					options.icon = value;
					break;
				case "-r": case "-resource":
					foreach (string ss in value.Split (new char [] {','}))
						resources.Add (ss);
					break;
				case "-about":
					Console.WriteLine ("MacPack 1.0 by Geoff Norton\n");
					break;
					
				case "-m": case "-mode":
					switch (value){
					case "winforms":
						options.mode = 1;
						break;
					case "x11":
						options.mode = 3;
						break;
					case "console":
						options.mode = 0;
						break;
					case "cocoa":
						options.mode = 2;
						break;
					default:
						try {
							options.mode = Int32.Parse (value);
						} catch {
							Console.Error.WriteLine ("Could not recognize option {0} as the mode", value);
						}
						break;
					}
					break;

				case "-h": case "-help":
					Usage ();
					break;
					
				default:
					options.assembly = key;
					break;
				}
			}
			
			options.resource = (string [])resources.ToArray (typeof (string));
			Pack pack = new Pack (options);
			if (pack.Generate ())
				return 0;
			return -1;
		}
	
		[DllImport ("libc")]
		static extern int chmod (string path, uint mode);
	}
}
