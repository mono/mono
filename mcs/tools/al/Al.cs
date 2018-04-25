//
// Mono.AssemblyLinker.AssemblyLinker
//
// Author(s):
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Configuration.Assemblies;

using IKVM.Reflection;
using IKVM.Reflection.Emit;
using Mono.Security.Cryptography;

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

	enum DelaySign {
		NotSet,
		Yes,
		No
	}

	public enum Platform {
		AnyCPU,
		AnyCPU32Preferred,
		Arm,
		X86,
		X64,
		IA64
	}

	public class AssemblyLinker {

		ArrayList inputFiles = new ArrayList ();
		ArrayList resources = new ArrayList ();
		ArrayList cattrs = new ArrayList ();
		bool fullPaths;
		string outFile;
		string entryPoint;
		string win32IconFile;
		string win32ResFile;
		string title;
		string description;
		string company;
		string product;
		string copyright;
		string trademark;
		string templateFile;
		bool isTemplateFile = false;
		Target target = Target.Dll;
		Platform platform = Platform.AnyCPU;
		DelaySign delaysign = DelaySign.NotSet;
		string keyfile;
		string keyname;
		string culture;
		Universe universe;

		public static int Main (String[] args) {
			return new AssemblyLinker ().DynMain (args);
		}

		private int DynMain (String[] args) {
			using (universe = new Universe (UniverseOptions.MetadataOnly)) {
				universe.LoadFile (typeof (object).Assembly.Location);
				ParseArgs (args);

				DoIt ();

				return 0;
			}
		}

		private void ParseArgs (string[] args) 
		{
			ArrayList flat_args = new ArrayList ();

			// Process response files
			Hashtable response_files = new Hashtable ();
			foreach (string str in args) {
				if (str [0] != '@') {
					flat_args.Add (str);
					continue;
				}

				if (str.Length == 1)
					ReportMissingFileSpec ("@");

				string resfile_name = Path.GetFullPath (str.Substring (1));
				if (response_files.ContainsKey (resfile_name))
					Report (1006, "Response file '" + resfile_name + "' was already included");
				response_files [resfile_name] = resfile_name;
				LoadArgs (resfile_name, flat_args);
			}

			if (flat_args.Count == 0)
				Usage ();

			foreach (string str in flat_args) {
				if ((str [0] != '-') && (str [0] != '/')) {
					inputFiles.Add (GetModuleInfo (str));
					continue;
				}

				if (!ParseOption(str)) {
					if (RunningOnUnix) {
						// cope with absolute filenames for modules on unix, as
						// they also match the option pattern
						//
						// `/home/test.cs' is considered as a module, however
						// '/test.cs' is considered as error
						if (str.Length > 2 && str.IndexOf ('/', 2) != -1) {
							inputFiles.Add (GetModuleInfo (str));
							continue;
						}
					}

					Report (1013, String.Format ("Unrecognized command line option: '{0}'", str));
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

		private bool ParseOption (string str)
		{
			string arg;
			string opt = GetCommand (str, out arg);

			switch (opt) {
			case "help":
			case "?":
				Usage ();
				return true;

			case "embed": {
					if (arg == null)
						ReportMissingFileSpec (opt);
					ResourceInfo res = new ResourceInfo ();
					res.isEmbedded = true;
					String [] parts = arg.Split (',');
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
					return true;
				}

			case "link": {
					if (arg == null)
						ReportMissingFileSpec (opt);
					ResourceInfo res = new ResourceInfo ();
					String [] parts = arg.Split (',');
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
					return true;
				}

			case "algid":
				if (arg == null)
					ReportMissingArgument (opt);
				try {
					string realArg = arg;
					if (realArg.StartsWith ("0x"))
						realArg = realArg.Substring (2);
					uint val = Convert.ToUInt32 (realArg, 16);
					AddCattr (typeof (System.Reflection.AssemblyAlgorithmIdAttribute), typeof (uint), val);
				} catch (Exception) {
					ReportInvalidArgument (opt, arg);
				}
				return true;

			case "base":
				ReportNotImplemented (opt);
				return true;

			case "baseaddress":
				ReportNotImplemented (opt);
				return true;

			case "bugreport":
				ReportNotImplemented (opt);
				return true;

			case "comp":
			case "company":
				if (arg == null)
					ReportMissingText (opt);
				company = arg;
				return true;

			case "config":
			case "configuration":
				if (arg == null)
					ReportMissingText (opt);
				AddCattr (typeof (System.Reflection.AssemblyConfigurationAttribute), arg);
				return true;

			case "copy":
			case "copyright":
				if (arg == null)
					ReportMissingText (opt);
				copyright = arg;
				return true;

			case "c":
			case "culture":
				if (arg == null)
					ReportMissingText (opt);
				culture = arg;
				return true;

			case "delay":
			case "delaysign":
			case "delay+":
			case "delaysign+":
				delaysign = DelaySign.Yes;
				return true;

			case "delay-":
			case "delaysign-":
				delaysign = DelaySign.No;
				return true;

			case "descr":
			case "description":
				if (arg == null)
					ReportMissingText (opt);
				description = arg;
				return true;

			case "e":
			case "evidence": {
				if (arg == null)
					ReportMissingFileSpec (opt);
				ResourceInfo res = new ResourceInfo ();
				res.name = "Security.Evidence";
				res.fileName = arg;
				res.isEmbedded = true;
				res.isPrivate = true;
				resources.Add (res);
				return true;
			}
			case "fileversion":
				if (arg == null)
					ReportMissingText (opt);

				AddCattr (typeof (System.Reflection.AssemblyFileVersionAttribute), arg);
				return true;

			case "flags":
				if (arg == null)
					ReportMissingArgument (opt);
				try {
					string realArg = arg;
					if (realArg.StartsWith ("0x"))
						realArg = realArg.Substring (2);
					uint val = Convert.ToUInt32 (realArg, 16);
					AddCattr (typeof (System.Reflection.AssemblyFlagsAttribute), typeof (uint), val);
				} catch (Exception) {
					ReportInvalidArgument (opt, arg);
				}
				return true;

			case "fullpaths":
				fullPaths = true;
				return true;

			case "keyf":
			case "keyfile":
				if (arg == null)
					ReportMissingText (opt);
				keyfile = arg;
				return true;

			case "keyn":
			case "keyname":
				if (arg == null)
					ReportMissingText (opt);
				keyname = arg;
				return true;

			case "main":
				if (arg == null)
					ReportMissingText (opt);
				entryPoint = arg;
				return true;

			case "nologo":
				return true;

			case "out":
				if (arg == null)
					ReportMissingFileSpec (opt);
				outFile = arg;
				return true;

		case "platform":
			if (arg == null)
				ReportMissingText (opt);
			switch (arg.ToLowerInvariant ()) {
				case "arm":
					platform = Platform.Arm;
					break;
				case "anycpu":
					platform = Platform.AnyCPU;
					break;
				case "x86":
					platform = Platform.X86;
					break;
				case "x64":
					platform = Platform.X64;
					break;
				case "itanium":
					platform = Platform.IA64;
					break;
				case "anycpu32bitpreferred":
					platform = Platform.AnyCPU32Preferred;
					break;
				default:
					ReportInvalidArgument (opt, arg);
					break;
				}
				return true;

			case "prod":
			case "product":
				if (arg == null)
					ReportMissingText (opt);
				product = arg;
				return true;

			case "productv":
			case "productversion":
				if (arg == null)
					ReportMissingText (opt);
				AddCattr (typeof (System.Reflection.AssemblyInformationalVersionAttribute), arg);
				return true;

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
				return true;

			case "template":
				if (arg == null)
					ReportMissingFileSpec (opt);
				isTemplateFile = true;
				templateFile = Path.Combine (Directory.GetCurrentDirectory (), arg);
				return true;

			case "title":
				if (arg == null)
					ReportMissingText (opt);
				title = arg;
				return true;

			case "trade":
			case "trademark":
				if (arg == null)
					ReportMissingText (opt);
				trademark = arg;
				return true;

			case "v":
			case "version":
				// This option conflicts with the standard UNIX meaning
				if (arg == null) {
					Version ();
					break;
				}
				AddCattr (typeof (System.Reflection.AssemblyVersionAttribute), arg);
				return true;

			case "win32icon":
				if (arg == null)
					ReportMissingFileSpec (opt);
				win32IconFile = arg;
				return true;

			case "win32res":
				if (arg == null)
					ReportMissingFileSpec (opt);
				win32ResFile = arg;
				return true;
			}
			return false;
		}

		private bool RunningOnUnix {
			get {
				// check for Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform == 4) || (platform == 128) || (platform == 6));
			}
		}

		private ModuleInfo GetModuleInfo (string str)
		{
			string [] parts = str.Split (',');
			ModuleInfo mod = new ModuleInfo ();
			mod.fileName = parts [0];
			if (parts.Length > 1)
				mod.target = parts [1];
			return mod;
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

		private void AddCattr (System.Type attrType, System.Type arg, object value) {
			var importedAttrType = universe.Import(attrType);
			var importedArg = universe.Import(arg);

			cattrs.Add (new CustomAttributeBuilder (importedAttrType.GetConstructor (new [] { importedArg }), new [] { value }));
		}

		private void AddCattr (System.Type attrType, object value) {
			AddCattr (attrType, typeof (string), value);
		}

		private void PrintVersion () {
			Console.WriteLine ("Mono Assembly Linker (al.exe) version " + Consts.MonoVersion);
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

		// copied from /mcs/mcs/codegen.cs
		private void SetPublicKey (AssemblyName an, byte[] strongNameBlob) 
		{
			// check for possible ECMA key
			if (strongNameBlob.Length == 16) {
				// will be rejected if not "the" ECMA key
				an.SetPublicKey (strongNameBlob);
			} else {
				// take it, with or without, a private key
				RSA rsa = CryptoConvert.FromCapiKeyBlob (strongNameBlob);
				// and make sure we only feed the public part to Sys.Ref
				byte[] publickey = CryptoConvert.ToCapiPublicKeyBlob (rsa);
					
				// AssemblyName.SetPublicKey requires an additional header
				byte[] publicKeyHeader = new byte [12] { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00 };

				byte[] encodedPublicKey = new byte [12 + publickey.Length];
				Buffer.BlockCopy (publicKeyHeader, 0, encodedPublicKey, 0, 12);
				Buffer.BlockCopy (publickey, 0, encodedPublicKey, 12, publickey.Length);
				an.SetPublicKey (encodedPublicKey);
			}
		}

		private void SetKeyPair (AssemblyName aname)
		{
			if (keyfile != null) {
				if (!File.Exists (keyfile)) {
					Report (1044, String.Format ("Couldn't open '{0}' key file.", keyfile));
				}

				using (FileStream fs = File.OpenRead (keyfile)) {
					byte[] data = new byte [fs.Length];
					try {
						fs.Read (data, 0, data.Length);

						if (delaysign == DelaySign.Yes) {
							SetPublicKey (aname, data);
						} else {
							CryptoConvert.FromCapiPrivateKeyBlob (data);
							aname.KeyPair = new StrongNameKeyPair (data);
						}
					}
					catch (CryptographicException) {
						if (delaysign != DelaySign.Yes) {
							if (data.Length == 16) {
								// error # is different for ECMA key
								Report (1019, "Could not strongname the assembly. " + 
									"ECMA key can only be used to delay-sign assemblies");
							} else {
								Report (1028, String.Format ("Key file {0}' is missing it's private key " +
									"or couldn't be decoded.", keyfile));
							}
						} else {
							Report (1044, String.Format ("Couldn't decode '{0}' key file.", keyfile));
						}
					}
					fs.Close ();
				}
			} else if (keyname != null) {
				// delay-sign doesn't apply to key containers
				aname.KeyPair = new StrongNameKeyPair (keyname);
			}
		}
		
		private void DoIt () {
			AssemblyName aname = new AssemblyName ();
			aname.Name = Path.GetFileNameWithoutExtension (outFile);
			if (culture != null)
				aname.CultureInfo = new CultureInfo (culture);

			string fileName = Path.GetFileName (outFile);

			AssemblyBuilder ab;
			
			/*
			 * Emit Manifest
			 * */

			if (isTemplateFile)
				aname = ReadCustomAttributesFromTemplateFile (templateFile, aname);

			if (!String.IsNullOrEmpty (title))
				AddCattr (typeof (System.Reflection.AssemblyTitleAttribute), title);
			if (!String.IsNullOrEmpty (description))
				AddCattr (typeof (System.Reflection.AssemblyDescriptionAttribute), description);
			if (!String.IsNullOrEmpty (company))
				AddCattr (typeof (System.Reflection.AssemblyCompanyAttribute), company);
			if (!String.IsNullOrEmpty (product))
				AddCattr (typeof (System.Reflection.AssemblyProductAttribute), product);
			if (!String.IsNullOrEmpty (copyright))
				AddCattr (typeof (System.Reflection.AssemblyCopyrightAttribute), copyright);
			if (!String.IsNullOrEmpty (trademark))
				AddCattr (typeof (System.Reflection.AssemblyTrademarkAttribute), trademark);

			SetKeyPair (aname);

			if (fileName != outFile)
				ab = universe.DefineDynamicAssembly (aname, AssemblyBuilderAccess.Save, Path.GetDirectoryName (outFile));
			else
				ab = universe.DefineDynamicAssembly (aname, AssemblyBuilderAccess.Save);

			foreach (CustomAttributeBuilder cb in cattrs)
				ab.SetCustomAttribute (cb);

			/*
			 * Emit modules
			 */

			foreach (ModuleInfo mod in inputFiles) {
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
				ab.__AddModule (universe.OpenRawModule(mod.fileName));
			}

			/*
			 * Set entry point
			 */

			if (entryPoint != null) {
				string mainClass = entryPoint.Substring (0, entryPoint.LastIndexOf ('.'));
				string mainMethod = entryPoint.Substring (entryPoint.LastIndexOf ('.') + 1);

				MethodInfo mainMethodInfo = null;

				try {
					IKVM.Reflection.Type mainType = ab.GetType (mainClass);
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
			}

			/*
			 * Emit resources
			 */

			ab.DefineVersionInfoResource ();

			if (win32IconFile != null) {
				try {
					ab.__DefineIconResource (File.ReadAllBytes (win32IconFile));
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

			ModuleBuilder mainModule = null;

			foreach (ResourceInfo res in resources) {
				if (res.name == null)
					res.name = Path.GetFileName (res.fileName);

				foreach (ResourceInfo res2 in resources)
					if ((res != res2) && (res.name == res2.name))
						Report (1046, String.Format ("Resource identifier '{0}' has already been used in this assembly", res.name));

				if (res.isEmbedded) {
					if (mainModule == null) {
						mainModule = ab.DefineDynamicModule (fileName, fileName, false);
					}

					Stream stream = new MemoryStream (File.ReadAllBytes (res.fileName));

					mainModule.DefineManifestResource (res.name, stream, res.isPrivate ? ResourceAttributes.Private : ResourceAttributes.Public);
				}
				else {
					if (res.target != null) {
						File.Copy (res.fileName, res.target, true);
						res.fileName = res.target;
					}
					
					// AddResourceFile must receive a file name and not a path.
					// Drop directory and give warning if we have a path.
					var resourceFileName = Path.GetFileName(res.fileName);
					if (Path.GetDirectoryName (res.fileName) != null || Path.IsPathRooted(res.fileName)) {
						ReportWarning (99999, 
							String.Format ("Path '{0}' in the resource name is not supported. Using just file name '{1}'",
								res.fileName,
								resourceFileName));
					}

					ab.AddResourceFile (res.name, resourceFileName,
							res.isPrivate ? ResourceAttributes.Private : ResourceAttributes.Public);
				}
			}

			PortableExecutableKinds pekind = PortableExecutableKinds.ILOnly;
			ImageFileMachine machine;

			switch (platform) {
			case Platform.X86:
				pekind |= PortableExecutableKinds.Required32Bit;
				machine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				pekind |= PortableExecutableKinds.PE32Plus;
				machine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				machine = ImageFileMachine.IA64;
				break;
			case Platform.AnyCPU32Preferred:
				pekind |= PortableExecutableKinds.Preferred32Bit;
				machine = ImageFileMachine.I386;
				break;
			case Platform.Arm:
				machine = ImageFileMachine.ARM;
				break;
			case Platform.AnyCPU:
			default:
				machine = ImageFileMachine.I386;
				break;
			}

			try {
				ab.Save (fileName, pekind, machine);
			}
			catch (Exception ex) {
				Report (1019, "Metadata failure creating assembly -- " + ex);
			}
		}

		private AssemblyName ReadCustomAttributesFromTemplateFile (string templateFile, AssemblyName aname)
		{
			// LAMESPEC: according to MSDN, the template assembly must have a
			// strong name but this is not enforced
			var asm = universe.LoadFile (templateFile);

			// Create missing assemblies, we don't want to load them!
			// Code taken from ikdasm
			var names = new HashSet<string> ();
			AssemblyName[] assembly_refs = asm.ManifestModule.__GetReferencedAssemblies ();

			var resolved_assemblies = new Assembly [assembly_refs.Length];
			for (int i = 0; i < resolved_assemblies.Length; i++) {
				string name = assembly_refs [i].Name;

				while (names.Contains (name)) {
					name = name + "_" + i;
				}
				names.Add (name);
				resolved_assemblies [i] = universe.CreateMissingAssembly (assembly_refs [i].FullName);
			}
			asm.ManifestModule.__ResolveReferencedAssemblies (resolved_assemblies);

			foreach (var attr_data in asm.__GetCustomAttributes (null, false)) {
				string asm_name = attr_data.AttributeType.Assembly.GetName ().Name;
				if (asm_name != "mscorlib")
					continue;

				switch (attr_data.AttributeType.FullName) {
					case "System.Reflection.AssemblyKeyFileAttribute": {
						if (keyfile != null)
							// ignore if specified on command line
							continue;

						// / AssemblyKeyFileAttribute .ctor(string keyFile)
						string key_file_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (key_file_value))
							keyfile = Path.Combine (Path.GetDirectoryName (templateFile), key_file_value);
					}
					break;

					case "System.Reflection.AssemblyDelaySignAttribute": {
						if (delaysign != DelaySign.NotSet)
							// ignore if specified on command line
							continue;

						// AssemblyDelaySignAttribute .ctor(bool delaySign)
						bool delay_sign_value = (bool) attr_data.ConstructorArguments [0].Value;
						delaysign = delay_sign_value ? DelaySign.Yes : DelaySign.No;
					}
					break;

					case "System.Reflection.AssemblyKeyNameAttribute": {
						if (keyname != null)
							// ignore if specified on command line
							continue;

						// AssemblyKeyNameAttribute .ctor(string keyName)
						string key_name_value = (string) attr_data.ConstructorArguments [0].Value;

						// ignore null or zero-length keyname
						if (!String.IsNullOrEmpty (key_name_value))
							keyname = key_name_value;
					}
					break;

					case "System.Reflection.AssemblyTitleAttribute": {
						if (title != null)
							// ignore if specified on command line
							continue;

						// AssemblyTitleAttribute .ctor(string title)
						string title_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (title_value))
							title = title_value;
					}
					break;

					case "System.Reflection.AssemblyDescriptionAttribute": {
						if (description != null)
							// ignore if specified on command line
							continue;

						// AssemblyDescriptionAttribute .ctor(string description)
						string description_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (description_value))
							description = description_value;
					}
					break;

					case "System.Reflection.AssemblyProductAttribute": {
						if (product != null)
							// ignore if specified on command line
							continue;

						// AssemblyProductAttribute .ctor(string product)
						string product_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (product_value))
							product = product_value;
					}
					break;

					case "System.Reflection.AssemblyCompanyAttribute": {
						if (company != null)
							// ignore if specified on command line
							continue;

						// AssemblyCompanyAttribute .ctor(string company)
						string company_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (company_value))
							company = company_value;

					}
					break;

					case "System.Reflection.AssemblyCopyrightAttribute": {
						if (copyright != null)
							// ignore if specified on command line
							continue;

						// AssemblyCopyrightAttribute .ctor(string copyright)
						string copyright_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (copyright_value))
							copyright = copyright_value;
					}
					break;

					case "System.Reflection.AssemblyTrademarkAttribute": {
						if (trademark != null)
							// ignore if specified on command line
							continue;

						// AssemblyTrademarkAttribute .ctor(string trademark)
						string trademark_value = (string) attr_data.ConstructorArguments [0].Value;

						if (!String.IsNullOrEmpty (trademark_value))
							trademark = trademark_value;
					}
					break;
				}
			}

			var asm_name_for_template_file = asm.GetName ();
			aname.Version = asm_name_for_template_file.Version;
			aname.HashAlgorithm = asm_name_for_template_file.HashAlgorithm;

			return aname;
		}

		private void LoadArgs (string file, ArrayList args) {
			StreamReader f = null;
			string line;
			try {
				f = new StreamReader (file);

				StringBuilder sb = new StringBuilder ();
			
				while ((line = f.ReadLine ()) != null){
					int t = line.Length;

					for (int i = 0; i < t; i++){
						char c = line [i];
						
						if (c == '"' || c == '\''){
							char end = c;
							
							for (i++; i < t; i++){
								c = line [i];

								if (c == end)
									break;
								sb.Append (c);
							}
						} else if (c == ' '){
							if (sb.Length > 0){
								args.Add (sb.ToString ());
								sb.Length = 0;
							}
						} else
							sb.Append (c);
					}
					if (sb.Length > 0){
						args.Add (sb.ToString ());
						sb.Length = 0;
					}
				}
			} catch (Exception ex) {
				Report (1007, "Error opening response file '" + file + "' -- '" + ex.Message + "'");
			} finally {
				if (f != null)
					f.Close ();
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
			"  /platform:<text>          Limit which platforms this code can run on; must be",
			"                            one of x86, Itanium, x64, arm, anycpu32bitpreferred,",
			"                            or anycpu (the default)",
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
