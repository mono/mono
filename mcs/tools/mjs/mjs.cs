//
// driver.cs: Guides the compilation process through the different phases.
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) Copyright 2005, 2006, Novell Inc. (http://www.novell.com)
//

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
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Vsa;
using Microsoft.JScript;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using Microsoft.JScript.Vsa;
using System.Reflection.Emit;
using Mono.CSharp;

namespace Mono.JScript {
	
	class Driver {
		//
		// Assemblies references to be linked. Initialized with
		// mscorlib.dll
		//
		private static ArrayList references;

		// Lookup paths
		private static ArrayList link_paths;

		// jscript files
		private static ArrayList files;
			
		private static string first_source;

		private static bool want_debugging_support = false;

		private static string output_file;
		private static int warning_level = -1;

		private static Assembly [] assemblies = new Assembly [0];

		private static bool StdLib = true;

		private static void Usage ()
		{
			Console.WriteLine ("Mono JScript compiler\n" +
					   "Copyright (C) 2003 - 2004 Cesar Lopez Nataren\n" +
					   "Copyright (C) 2004 - 2006 Novell Inc (http://novell.com)\n\n" +
					   "mjs [options] source-file\n" +
					   "   /about              About the Mono JScript compiler\n" +
					   "   /lib:PATH1,PATH2    Adds the paths to the assembly link path\n" +
					   "   /nostdlib[+|-]      Does not load core libraries\n" +
					   "   /out:<file>         Specify name of binary output file\n" +
					   "   /pkg:P1[,Pn]        References packages P1..Pn\n" +
					   "   /r[eference]:ASS    Reference the specified assembly\n" +

					   "\n" +
					   "Resources:\n" +
					   "   @file               Read response file for more options\n\n" +
					   "Options can be of the form -option or /option");
		}

		private static void About ()
		{
			Console.WriteLine (
					   "The Mono JScript compiler is:\n" +
					   "Copyright (C) 2003 - 2004 Cesar Lopez Nataren\n" +
					   "Copyright (C) 2004 - 2006 Novell Inc.\n\n" +
					   "The compiler source code is released under the terms of both the MIT X11 and MPL\n" +
					   "The compiler was written by Cesar Lopez Nataren");
			Environment.Exit (0);
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		private static void LoadReferences ()
		{
			foreach (string r in references)
				LoadAssembly (r, false);
		}

		private static void LoadAssembly (string assembly, bool soft)
		{
			Assembly a;
			string total_log = "";

			try {
				char [] path_chars = { '/', '\\' };

				if (assembly.IndexOfAny (path_chars) != -1) {
					a = Assembly.LoadFrom (assembly);
				} else {
					string ass = assembly;
					if (ass.EndsWith (".dll") || ass.EndsWith (".exe"))
						ass = assembly.Substring (0, assembly.Length - 4);
					a = Assembly.Load (ass);
				}
				AddAssembly (a);

			} catch (FileNotFoundException){
				foreach (string dir in link_paths){
					string full_path = Path.Combine (dir, assembly);
					if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
						full_path += ".dll";

					try {
						a = Assembly.LoadFrom (full_path);
						AddAssembly (a);
						return;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				if (!soft)
					Console.WriteLine ("Cannot find assembly `" + assembly + "'" );
			} catch (BadImageFormatException f) {
				Console.WriteLine ("Cannot load assembly (bad file format)" + f.FusionLog);
			} catch (FileLoadException f){
				Console.WriteLine ("Cannot load assembly " + f.FusionLog);
			} catch (ArgumentNullException){
				Console.WriteLine ("Cannot load assembly (null argument)");
			}
		}	

		/// <summary>
		///   Registers an assembly to load types from.
		/// </summary>
		private static void AddAssembly (Assembly a)
		{
			foreach (Assembly assembly in assemblies) {
				if (a == assembly)
					return;
			}

			int top = assemblies.Length;
			Assembly [] n = new Assembly [top + 1];

			assemblies.CopyTo (n, 0);

			n [top] = a;
			assemblies = n;
		}

		static string [] LoadArgs (string file)
		{
			StreamReader f;
			ArrayList args = new ArrayList ();
			string line;
			try {
				f = new StreamReader (file);
			} catch {
				return null;
			}

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

			string [] ret_value = new string [args.Count];
			args.CopyTo (ret_value, 0);

			return ret_value;
		}	

		//
		// Returns the directory where the system assemblies are installed
		//
		static string GetSystemDir ()
		{
			return Path.GetDirectoryName (typeof (object).Assembly.Location);

		}

		static void SetOutputFile (string name)
		{
			output_file = name;
		}

		static void Version ()
		{
			string version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			Console.WriteLine ("Mono JScript compiler version {0}", version);
			Environment.Exit (0);
		}
	
		static bool UnixParseOption (string arg, ref string [] args, ref int i)
		{
			switch (arg){
			case "--version":
				Version ();
				return true;
				
			case "/?": case "/h": case "/help":
			case "--help":
				Usage ();
				Environment.Exit (0);
				return true;
			case "-o": 
			case "--output":
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				SetOutputFile (args [++i]);
				return true;
			case "-r":
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}

				references.Add (args [++i]);
				return true;
				
			case "-L":
				if ((i + 1) >= args.Length){
					Usage ();	
					Environment.Exit (1);
				}
				link_paths.Add (args [++i]);
				return true;
				
			case "--about":
				About ();
				return true;
			}
			return false;
		}

		//
		// This parses the -arg and /arg options to the compiler, even if the strings
		// in the following text use "/arg" on the strings.
		//
		static bool CSCParseOption (string option, ref string [] args, ref int i)
		{
			int idx = option.IndexOf (':');
			string arg, value;
			if (idx == -1){
				arg = option;
				value = "";
			} else {
				arg = option.Substring (0, idx);

				value = option.Substring (idx + 1);
			}

			switch (arg){
			case "/nologo":
				return true;

			case "/out":
				if (value == ""){
					Usage ();
					Environment.Exit (1);
				}
				SetOutputFile (value);
				return true;

			case "/pkg":
				string packages;

				if (value == String.Empty) {
					Usage ();
					Environment.Exit (1);
				}
				packages = String.Join (" ", value.Split (new Char [] { ';', ',', '\n', '\r'}));

				ProcessStartInfo pi = new ProcessStartInfo ();
				pi.FileName = "pkg-config";
				pi.RedirectStandardOutput = true;
				pi.UseShellExecute = false;
				pi.Arguments = "--libs " + packages;
				Process p = null;
				try {
					p = Process.Start (pi);
				} catch (Exception e) {
					Console.Error.WriteLine ("Couldn't run pkg-config: " + e.Message);
					Environment.Exit (1);
				}

				if (p.StandardOutput == null){
					Console.Error.WriteLine ("Specified package did not return any information");
					return true;
				}
				string pkgout = p.StandardOutput.ReadToEnd ();
				p.WaitForExit ();
				if (p.ExitCode != 0) {
					Console.Error.WriteLine ("Error running pkg-config. Check the above output.");
					Environment.Exit (1);
				}

				if (pkgout != null){
					string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
						Split (new Char [] { ' ', '\t'});
					args = AddArgs (args, xargs);
				}
				p.Close ();
				return true;

			case "/r":
			case "/reference": {
				if (value == ""){
					Console.WriteLine ("/reference requires an argument");
					Environment.Exit (1);
				}

				string [] refs = value.Split (new char [] { ';', ',' });
				foreach (string r in refs)
					references.Add (r);
				return true;
			}

			case "/lib": {
				string [] libdirs;
				
				if (value == ""){
					Console.WriteLine ("/lib requires an argument");
					Environment.Exit (1);
				}

				libdirs = value.Split (new Char [] { ',' });
				foreach (string dir in libdirs)
					link_paths.Add (dir);
				return true;
			}

			case "/about":
				About ();
				return true;


			case "/nostdlib":
			case "/nostdlib+":
				StdLib = false;
				return true;

			case "/nostdlib-":
				StdLib = true;
				return true;
			case "/target":
				if (value.Length == 0) {
					Console.WriteLine ("fatal error JS2013: Target type is invalid");
					Environment.Exit (1);
				}

				if (string.Compare ("exe", value, true) == 0) {
					// this is the default (and only) supported target
				} else if (string.Compare ("library", value, true) != 0) {
					Console.WriteLine ("mjs currently does not support creating libraries");
					Environment.Exit (1);
				} else {
					Console.WriteLine ("fatal error JS2013: Target '{0}' is invalid."
						+ " Specify 'exe' or 'library'", value);
					Environment.Exit (1);
				}
				return true;
			case "/warn":
				if (value.Length == 0) {
					Console.WriteLine ("fatal error JS2028: No warning level specified"
						+ " with option '{0}'", option);
					Environment.Exit (1);
				}

				try {
					warning_level = int.Parse (value, CultureInfo.InvariantCulture);
				} catch {
				}

				if (warning_level < 0 || warning_level > 4) {
					Console.WriteLine ("fatal error JS2015: Invalid warning level specified"
						+ " with option '{0}'", option);
					Environment.Exit (1);
				}
				return true;
			}
			return false;
		}
	
		static string [] AddArgs (string [] args, string [] extra_args)
		{
			string [] new_args;
			new_args = new string [extra_args.Length + args.Length];

			// if args contains '--' we have to take that into account
			// split args into first half and second half based on '--'
			// and add the extra_args before --
			int split_position = Array.IndexOf (args, "--");
			if (split_position != -1) {
				Array.Copy (args, new_args, split_position);
				extra_args.CopyTo (new_args, split_position);
				Array.Copy (args, split_position, new_args, split_position + extra_args.Length, args.Length - split_position);
			} else {
				args.CopyTo (new_args, 0);
				extra_args.CopyTo (new_args, args.Length);
			}
			return new_args;
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		static void SplitPathAndPattern (string spec, out string path, out string pattern)
		{
			int p = spec.LastIndexOf ('/');
			if (p != -1){
				//
				// Windows does not like /file.cs, switch that to:
				// "\", "file.cs"
				//
				if (p == 0){
					path = "\\";
					pattern = spec.Substring (1);
				} else {
					path = spec.Substring (0, p);
					pattern = spec.Substring (p + 1);
				}
				return;
			}

			p = spec.LastIndexOf ('\\');
			if (p != -1){
				path = spec.Substring (0, p);
				pattern = spec.Substring (p + 1);
				return;
			}

			path = ".";
			pattern = spec;
		}

		static void ProcessFile (string f)
		{
			if (first_source == null)
				first_source = f;

			files.Add (f);
		}

		static void CompileFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern (spec, out path, out pattern);
			if (pattern.IndexOf ('*') == -1){
				ProcessFile (spec);
				return;
			}

			string [] files = null;
			try {
				files = Directory.GetFiles (path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Console.WriteLine ("Source file `" + spec + "' could not be found");
				return;
			} catch (System.IO.IOException){
				Console.WriteLine ("Source file `" + spec + "' could not be found");
				return;
			}
			foreach (string f in files)
				ProcessFile (f);

			if (!recurse)
				return;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories (path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				CompileFiles (d + "/" + pattern, true);
			}
		}

		internal static bool MainDriver (string [] args)
		{
			int i;
			bool parsing_options = true;

			references = new ArrayList ();
			link_paths = new ArrayList ();
			files = new ArrayList ();

			Hashtable response_file_list = null;

			for (i = 0; i < args.Length; i++){
				string arg = args [i];
				if (arg == "")
					continue;
				
				if (arg.StartsWith ("@")){
					string [] extra_args;
					string response_file = arg.Substring (1);

					if (response_file_list == null)
						response_file_list = new Hashtable ();
					
					if (response_file_list.Contains (response_file)){
						Console.WriteLine ("Response file `" + response_file + "' specified multiple times");
						Environment.Exit (1);
					}
					
					response_file_list.Add (response_file, response_file);
						    
					extra_args = LoadArgs (response_file);
					if (extra_args == null){
						Console.WriteLine ("Unable to open response file: " + response_file);
						return false;
					}

					args = AddArgs (args, extra_args);
					continue;
				}

				if (parsing_options){
					if (arg == "--"){
						parsing_options = false;
						continue;
					}
					
					if (arg.StartsWith ("-")){
						if (UnixParseOption (arg, ref args, ref i))
							continue;

						// Try a -CSCOPTION
						string csc_opt = "/" + arg.Substring (1);
						if (CSCParseOption (csc_opt, ref args, ref i))
							continue;
					} else {
						if (arg.StartsWith ("/")){
							if (CSCParseOption (arg, ref args, ref i))
								continue;
						}
					}
				}
				CompileFiles (arg, false); 
			}

			//
			// If there is nothing to put in the assembly, and we are not a library
			//
			if (first_source == null) /* && embedded_resources == null && resources == null) */ {
				Console.WriteLine ("fatal error JS2026: No input sources specified");
				return false;
			}

			//
			// Load Core Library for default compilation
			//
			if (StdLib)
				references.Insert (0, "mscorlib");


			//
			// Load assemblies required
			//
			link_paths.Add (GetSystemDir ());
			link_paths.Add (Directory.GetCurrentDirectory ());
			LoadReferences ();
			return true;
		}
	
		//
		// Entry point
		//
		private static void Main (string [] args) {
			if (args.Length < 1) {
				Usage ();
				Environment.Exit (0);
			}			
			MainDriver (args);
			VsaEngine engine = new VsaEngine ();
			engine.InitVsaEngine ("mjs:com.mono-project", new MonoEngineSite ());
			
			foreach (string asm in references) {
				IVsaReferenceItem item = (IVsaReferenceItem) engine.Items.CreateItem (asm, VsaItemType.Reference, VsaItemFlag.None);
				item.AssemblyName = asm;
			}

			string asm_name = String.Empty;
			
			foreach (Assembly assembly in assemblies) {
				asm_name = assembly.GetName ().FullName;
				IVsaReferenceItem item = (IVsaReferenceItem) engine.Items.CreateItem (asm_name, VsaItemType.Reference, VsaItemFlag.None);
				item.AssemblyName = asm_name;
			}

			foreach (string file in files) {
				IVsaCodeItem item = (IVsaCodeItem) engine.Items.CreateItem (file, VsaItemType.Code, VsaItemFlag.None);
				item.SourceText = GetCodeFromFile (file);
			}
			engine.SetOption ("debug", want_debugging_support);
			engine.SetOption ("link_path", link_paths);
			engine.SetOption ("first_source", first_source);
			engine.SetOption ("assemblies", assemblies);
			engine.SetOption ("out", output_file);
			if (warning_level != -1)
				engine.SetOption ("WarningLevel", warning_level);
			engine.Compile ();
		}

		static string GetCodeFromFile (string fn)
		{
			try {
				StreamReader reader = new StreamReader (fn);
				return reader.ReadToEnd ();
			} catch (FileNotFoundException) {
				throw new JScriptException (JSError.FileNotFound);
			} catch (ArgumentNullException) {
				throw new JScriptException (JSError.FileNotFound);
			} catch (ArgumentException) {
				throw new JScriptException (JSError.FileNotFound);
			} catch (IOException) {
				throw new JScriptException (JSError.NoError);
			} catch (OutOfMemoryException) {
				throw new JScriptException (JSError.OutOfMemory);
			}
		}
	}

	class MonoEngineSite : IVsaSite {
		public void GetCompiledState (out byte [] pe, out byte [] debugInfo)
		{
			throw new NotImplementedException ();
		}

		public object GetEventSourceInstance (string itemName, string eventSourceName)
		{
			throw new NotImplementedException ();
		}

		public object GetGlobalInstance (string name)
		{
			throw new NotImplementedException ();
		}

		public void Notify (string notify, object info)
		{
			throw new NotImplementedException ();
		}

		public bool OnCompilerError (IVsaError error)
		{
			throw new NotImplementedException ();
		}
	}
}
