//
// Mono.AssemblyLinker.AssemblyLinker
//
// Author(s):
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.AssemblyLinker
{
	class ModuleInfo {
		public string fileName;
		public string target;
	}

	class ResourceInfo {
		public string name;
		public string fileName;
		public string target;
		public bool isEmbedded;
		public bool isPrivate;
	}

	enum Target {
		Dll,
		Exe,
		Win
	}

	public class AssemblyLinker {

		ArrayList inputFiles = new ArrayList ();
		ArrayList resources = new ArrayList ();
		ArrayList cattrs = new ArrayList ();
		string outputFile;
		bool fullPaths;
		string outFile;
		string entryPoint;
		string win32IconFile;
		string win32ResFile;
		Target target;

		public static int Main (String[] args) {
			return new AssemblyLinker ().DynMain (args);
		}

		private int DynMain (String[] args) {
			ParseArgs (args);

			DoIt ();

			return 0;
		}

		private void ParseArgs (string[] args) {

			if (args.Length == 0)
				Usage ();

			foreach (string str in args) {
				if ((str [0] != '-') && (str [0] != '/')) {
					string[] parts = str.Split (',');
					ModuleInfo mod = new ModuleInfo ();
					mod.fileName = parts [0];
					if (parts.Length > 1)
						mod.target = parts [1];
					inputFiles.Add (mod);
					continue;
				}

				string arg;
				string opt = GetCommand (str, out arg);

				ResourceInfo res;
				switch (opt) {
				case "help":
				case "?":
					Usage ();
					break;

				case "embed": {
					if (arg == null)
						ReportMissingFileSpec (opt);
					res = new ResourceInfo ();
					res.isEmbedded = true;
					String[] parts = arg.Split (',');
					res.fileName = parts [0];
					if (parts.Length > 1)
						res.name = parts [1];
					if (parts.Length > 2) {
						switch (parts [2]) {
						case "public":
							break;
						case "private":
							res.isPrivate = true;
							break;
						default:
							ReportInvalidArgument (opt, parts [2]);
							break;
						}
					}
					resources.Add (res);
					break;
				}

				case "link": {
					if (arg == null)
						ReportMissingFileSpec (opt);
					res = new ResourceInfo ();
					String[] parts = arg.Split (',');
					res.fileName = parts [0];
					if (parts.Length > 1)
						res.name = parts [1];
					if (parts.Length > 2)
						res.target = parts [2];
					if (parts.Length > 3) {
						switch (parts [3]) {
						case "public":
							break;
						case "private":
							res.isPrivate = true;
							break;
						default:
							ReportInvalidArgument (opt, parts [3]);
							break;
						}
					}
					resources.Add (res);
					break;
				}

				case "algid":
					if (arg == null)
						ReportMissingArgument (opt);
					try {
						int val = Int32.Parse (arg);
						AddCattr (typeof (AssemblyAlgorithmIdAttribute), typeof (uint), val);
					}
					catch (Exception) {
						ReportInvalidArgument (opt, arg);
					}
					break;

				case "base":
					ReportNotImplemented (opt);
					break;

				case "baseaddress":
					ReportNotImplemented (opt);
					break;

				case "bugreport":
					ReportNotImplemented (opt);
					break;

				case "comp":
				case "company":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyCompanyAttribute), arg);
					break;

				case "config":
				case "configuration":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyConfigurationAttribute), arg);
					break;

				case "copy":
				case "copyright":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyCopyrightAttribute), arg);
					break;

				case "c":
				case "culture":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyCultureAttribute), arg);
					break;

				case "delay":
				case "delaysign":
					ReportNotImplemented (opt);
					break;

				case "descr":
				case "description":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyDescriptionAttribute), arg);
					break;

				case "e":
				case "evidence":
					if (arg == null)
						ReportMissingFileSpec (opt);
					res = new ResourceInfo ();
					res.name = "Security.Evidence";
					res.fileName = arg;
					res.isEmbedded = true;
					res.isPrivate = true;
					resources.Add (res);
					break;

				case "fileversion":
					if (arg == null)
						ReportMissingText (opt);

					AddCattr (typeof (AssemblyFileVersionAttribute), arg);
					break;

				case "flags":
					if (arg == null)
						ReportMissingArgument (opt);
					try {
						int val = Int32.Parse (arg);
						AddCattr (typeof (AssemblyFlagsAttribute), typeof (uint), val);
					}
					catch (Exception) {
						ReportInvalidArgument (opt, arg);
					}
					break;

				case "fullpaths":
					fullPaths = true;
					break;

				case "keyf":
				case "keyfile":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyKeyFileAttribute), arg);
					break;
					
				case "keyn":
				case "keyname":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyKeyNameAttribute), arg);
					break;

				case "main":
					if (arg == null)
						ReportMissingText (opt);
					entryPoint = arg;
					break;

				case "nologo":
					break;

				case "out":
					if (arg == null)
						ReportMissingFileSpec (opt);
					outFile = arg;
					break;

				case "prod":
				case "product":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyProductAttribute), arg);
					break;

				case "productv":
				case "productversion":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyInformationalVersionAttribute), arg);
					break;

				case "t":
				case "target":
					if (arg == null)
						ReportMissingText (opt);
					switch (arg) {
					case "lib":
					case "library":
						target = Target.Dll;
						break;
					case "exe":
						target = Target.Exe;
						break;
					case "win":
					case "winexe":
						Report (0, "target:win is not implemented");
						break;
					default:
						ReportInvalidArgument (opt, arg);
						break;
					}
					break;

				case "template":
					if (arg == null)
						ReportMissingFileSpec (opt);
					ReportNotImplemented (opt);
					break;

				case "title":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyTitleAttribute), arg);
					break;

				case "trade":
				case "trademark":
					if (arg == null)
						ReportMissingText (opt);
					AddCattr (typeof (AssemblyTrademarkAttribute), arg);
					break;

				case "v":
				case "version":
					// This option conflicts with the standard UNIX meaning
					if (arg == null) {
						Version ();
						break;
					}
					AddCattr (typeof (AssemblyVersionAttribute), arg);
					break;

				case "win32icon":
					if (arg == null)
						ReportMissingFileSpec (opt);
					win32IconFile = arg;
					break;

				case "win32res":
					if (arg == null)
						ReportMissingFileSpec (opt);
					win32ResFile = arg;
					break;

				default:
					Report (1013, String.Format ("Unrecognized command line option: '{0}'", opt));
					break;
				}
			}

			if ((inputFiles.Count == 0) && (resources.Count == 0))
				Report (1016, "No valid input files were specified");

			if (outFile == null)
				Report (1017, "No target filename was specified");

			if (target == Target.Dll && (entryPoint != null))
				Report (1035, "Libraries cannot have an entry point");

			if (target == Target.Exe && (entryPoint == null))
				Report (1036, "Entry point required for executable applications");
		}

		private string GetCommand (string str, out string command_arg) {
			if ((str [0] == '-') && (str.Length > 1) && (str [1] == '-'))
				str = str.Substring (1);

			int end_index = str.IndexOfAny (new char[] {':', '='}, 1);
			string command = str.Substring (1,
											end_index == -1 ? str.Length - 1 : end_index - 1);
			
			if (end_index != -1) {
				command_arg = str.Substring (end_index+1);
				if (command_arg == String.Empty)
					command_arg = null;
			} else {
				command_arg = null;
			}
				
			return command.ToLower ();
		}

		private void AddCattr (Type attrType, Type arg, object value) {
			cattrs.Add (new CustomAttributeBuilder (attrType.GetConstructor (new Type [] { arg }), new object [] { value }));
		}

		private void AddCattr (Type attrType, object value) {
			AddCattr (attrType, typeof (string), value);
		}

		private void AddResource (ResourceInfo res) {
			foreach (ResourceInfo res2 in resources) {
				if (res.name == res2.name) {

				}
			}
			resources.Add (res);
		}

		private void PrintVersion () {
			Console.WriteLine ("Mono Assembly Linker (al.exe) version " + Assembly.GetExecutingAssembly ().GetName ().Version.ToString ());
		}

		private void Version () {
			PrintVersion ();
			Environment.Exit (0);
		}

		private void Usage () {
			PrintVersion ();
			
			foreach (string s in usage)
				Console.WriteLine (s);
			Environment.Exit (0);
		}

		private void Report (int errorNum, string msg) {
			Console.WriteLine (String.Format ("ALINK: error A{0:0000}: {1}", errorNum, msg));
			Environment.Exit (1);
		}

		private void ReportWarning (int errorNum, string msg) {
			Console.WriteLine (String.Format ("ALINK: warning A{0:0000}: {1}", errorNum, msg));
		}

		private void ReportInvalidArgument (string option, string value) {
			Report (1012, String.Format ("'{0}' is not a valid setting for option '{1}'", value, option));
		}

		private void ReportMissingArgument (string option) {
			Report (1003, String.Format ("Compiler option '{0}' must be followed by an argument", option));
		}

		private void ReportNotImplemented (string option) {
			Report (0, String.Format ("Compiler option '{0}' is not implemented", option));
		}

		private void ReportMissingFileSpec (string option) {
			Report (1008, String.Format ("Missing file specification for '{0}' command-line option", option));
		}

		private void ReportMissingText (string option) {
			Report (1010, String.Format ("Missing ':<text>' for '{0}' option", option));
		}

		private void DoIt () {
			AssemblyName aname = new AssemblyName ();
			aname.Name = Path.GetFileNameWithoutExtension (outFile);

			string fileName = Path.GetFileName (outFile);

			AssemblyBuilder ab;

			if (fileName != outFile)
				ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aname, AssemblyBuilderAccess.Save, Path.GetDirectoryName (outFile));
			else
				ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aname, AssemblyBuilderAccess.Save);

			foreach (CustomAttributeBuilder cb in cattrs)
				ab.SetCustomAttribute (cb);

			/*
			 * Emit modules
			 */

			foreach (ModuleInfo mod in inputFiles) {
				MethodInfo mi = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (mi == null)
					Report (0, "Cannot add modules on this runtime: try the Mono runtime instead.");

				if (mod.target != null) {
					File.Copy (mod.fileName, mod.target, true);
					mod.fileName = mod.target;
				}

				bool isAssembly = false;
				try {
					AssemblyName.GetAssemblyName (mod.fileName);
					isAssembly = true;
				}
				catch (Exception) {
				}

				if (isAssembly)
					ReportWarning (1020, "Ignoring included assembly '" + mod.fileName + "'");
				else
					mi.Invoke (ab, new object [] { mod.fileName });
			}

			/*
			 * Set entry point
			 */

			string mainClass = entryPoint.Substring (0, entryPoint.LastIndexOf ('.'));
			string mainMethod = entryPoint.Substring (entryPoint.LastIndexOf ('.') + 1);

			MethodInfo mainMethodInfo = null;

			try {
				Type mainType = ab.GetType (mainClass);
				if (mainType != null)
					mainMethodInfo = mainType.GetMethod (mainMethod);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
			if (mainMethodInfo != null)
				ab.SetEntryPoint (mainMethodInfo);
			else
				Report (1037, "Unable to find the entry point method '" + entryPoint + "'");

			/*
			 * Emit resources
			 */

			ab.DefineVersionInfoResource ();

			if (win32IconFile != null) {
				try {
					MethodInfo mi = typeof (AssemblyBuilder).GetMethod ("DefineIconResource", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
					if (mi == null)
						Report (0, "Cannot embed win32 icons on this runtime: try the Mono runtime instead.");
					mi.Invoke (ab, new object [] {  win32IconFile });
				}
				catch (Exception ex) {
					Report (1031, "Error reading icon '" + win32IconFile + "' --" + ex);
				}
			}

			if (win32ResFile != null) {
				try {
					ab.DefineUnmanagedResource (win32ResFile);
				}
				catch (Exception ex) {
					Report (1019, "Metadata failure creating assembly -- " + ex);
				}
			}

			foreach (ResourceInfo res in resources) {
				if (res.name == null)
					res.name = Path.GetFileName (res.fileName);

				foreach (ResourceInfo res2 in resources)
					if ((res != res2) && (res.name == res2.name))
						Report (1046, String.Format ("Resource identifier '{0}' has already been used in this assembly", res.name));

				if (res.isEmbedded) {
					MethodInfo mi = typeof (AssemblyBuilder).GetMethod ("EmbedResourceFile", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,
						null, CallingConventions.Any, new Type [] { typeof (string), typeof (string) }, null);
					if (mi == null)
						Report (0, "Cannot embed resources on this runtime: try the Mono runtime instead.");
					mi.Invoke (ab, new object [] { res.name, res.fileName });
				}
				else {
					if (res.target != null) {
						File.Copy (res.fileName, res.target, true);
						res.fileName = res.target;
					}

					ab.DefineResource (res.name, "", res.fileName, 
									   res.isPrivate ? ResourceAttributes.Private : ResourceAttributes.Public);
				}
			}

			try {
				ab.Save (fileName);
			}
			catch (Exception ex) {
				Report (1019, "Metadata failure creating assembly -- " + ex);
			}
		}

		string[] usage = {
			"Usage: al [options] [sources]",
			"Options: ('/out' must be specified)",
			"",
			"  /? or /help               Display this usage message",
			"  @<filename>               Read response file for more options",
			"  /algid:<id>               Algorithm used to hash files (in hexadecimal)",
			"  /base[address]:<addr>     Base address for the library",
			"  /bugreport:<filename>     Create a 'Bug Report' file",
			"  /comp[any]:<text>         Company name",
			"  /config[uration]:<text>   Configuration string",
			"  /copy[right]:<text>       Copyright message",
			"  /c[ulture]:<text>         Supported culture",
			"  /delay[sign][+|-]         Delay sign this assembly",
			"  /descr[iption]:<text>     Description",
			"  /e[vidence]:<filename>    Security evidence file to embed",
			"  /fileversion:<version>    Optional Win32 version (overrides assembly version)",
			"  /flags:<flags>            Assembly flags  (in hexadecimal)",
			"  /fullpaths                Display files using fully-qualified filenames",
			"  /keyf[ile]:<filename>     File containing key to sign the assembly",
			"  /keyn[ame]:<text>         Key container name of key to sign assembly",
			"  /main:<method>            Specifies the method name of the entry point",
			"  /nologo                   Suppress the startup banner and copyright message",
			"  /out:<filename>           Output file name for the assembly manifest",
			"  /prod[uct]:<text>         Product name",
			"  /productv[ersion]:<text>  Product version",
			"  /t[arget]:lib[rary]       Create a library",
			"  /t[arget]:exe             Create a console executable",
			"  /t[arget]:win[exe]        Create a Windows executable",
			"  /template:<filename>      Specifies an assembly to get default options from",
			"  /title:<text>             Title",
			"  /trade[mark]:<text>       Trademark message",
			"  /v[ersion]:<version>      Version (use * to auto-generate remaining numbers)",
			"  /win32icon:<filename>     Use this icon for the output",
			"  /win32res:<filename>      Specifies the Win32 resource file",
			"",
			"Sources: (at least one source input is required)",
			"  <filename>[,<targetfile>] add file to assembly",
			"  /embed[resource]:<filename>[,<name>[,Private]]",
			"                            embed the file as a resource in the assembly",
			"  /link[resource]:<filename>[,<name>[,<targetfile>[,Private]]]",
			"                            link the file as a resource to the assembly",
		};

	}
}
