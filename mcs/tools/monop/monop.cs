//
// monop -- a semi-clone of javap
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	John Luke  (john.luke@gmail.com)
//
// (C) 2004 Ben Maurer
// (C) 2004 John Luke
//


using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;

class MonoP {
	static string assembly;
	static BindingFlags default_flags = 
		BindingFlags.Instance |
		BindingFlags.Static |
		BindingFlags.Public;
	
	// very common namespaces, all in corlib
	static readonly string [] v_common_ns = {
		"System",
		"System.Collections",
		"System.Reflection",
		"System.Text",
		"System.IO"
	};
	
	static readonly string [] common_assemblies = {
		"System.Xml.dll",
		"System.Web.dll",
		"gtk-sharp.dll",
		"glib-sharp.dll"
	};
	
	static readonly string [] common_ns = {
		"System.Xml",
		"System.Web",
		"Gtk",
		"GLib"
	};
	
	static Type GetType (string tname, bool ignoreCase)
	{
		Type t;
		if (assembly != null) {
			Assembly a = GetAssembly (assembly, true);
			t = a.GetType (tname, false, ignoreCase);
		} else {
			t = Type.GetType (tname, false, ignoreCase);
		}

		return t;
	}	

	static string [] GetAssemblyNamesFromGAC ()
	{
		Process p = new Process ();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.FileName = "gacutil";
		p.StartInfo.Arguments = "-l";
		p.Start ();

		string s;
		ArrayList names = new ArrayList ();
		StreamReader output = p.StandardOutput;

		while ((s = output.ReadLine ()) != null)
			names.Add (s);

		p.WaitForExit ();

		string [] retval = new string [names.Count - 2];
		names.CopyTo (1, retval, 0, retval.Length); // skip the first and last line
		return retval;
	}

	static Assembly GetAssembly (string assembly, bool exit)
	{
		Assembly a = null;

		try {
			// if it exists try to use LoadFrom
			if (File.Exists (assembly))
				a = Assembly.LoadFrom (assembly);
			// if it looks like a fullname try that
			else if (assembly.Split (',').Length == 4)
				a = Assembly.Load (assembly);
			// see if MONO_PATH has it
			else
				a = LoadFromMonoPath (assembly);
		} catch {
			// ignore exception it gets handled below
		}

		// last try partial name
		// this (apparently) is exception safe
		if (a == null)
			a = Assembly.LoadWithPartialName (assembly);

		if (a == null && exit) {
			Console.WriteLine ("Could not load {0}", MonoP.assembly);
			Environment.Exit (1);
		}

		return a;
	}

	static Assembly LoadFromMonoPath (string assembly)
	{
		// ; on win32, : everywhere else
		char sep = (Path.DirectorySeparatorChar == '/' ? ':' : ';');
		string[] paths = Environment.GetEnvironmentVariable ("MONO_PATH").Split (sep);
		foreach (string path in paths)
		{	
			string apath = Path.Combine (path, assembly);
			if (File.Exists (apath))
				return Assembly.LoadFrom (apath);	
		}
		return null;
	}

	static Type GetType (string tname)
	{
		return GetType (tname, false);
	}
	
	static void PrintTypes (string assembly)
	{
		Assembly a = GetAssembly (assembly, true);

		Console.WriteLine ();
		Console.WriteLine ("Assembly Information:");

		object[] cls = a.GetCustomAttributes (typeof (CLSCompliantAttribute), false);
		if (cls.Length > 0)
		{
			CLSCompliantAttribute cca = cls[0] as CLSCompliantAttribute;
			if (cca.IsCompliant)
				Console.WriteLine ("[CLSCompliant]");
		}

		foreach (string ai in a.ToString ().Split (','))
			Console.WriteLine (ai.Trim ());
			
		Console.WriteLine ();
		Type [] types = a.GetExportedTypes ();

		foreach (Type t in types)
			Console.WriteLine (t.FullName);

		Console.WriteLine ("\nTotal: {0} types.", types.Length);
	}
	
	static void Completion (string prefix)
	{
		foreach (Type t in typeof (object).Assembly.GetExportedTypes ()) {
			if (t.Name.StartsWith (prefix)) {
				if (Array.IndexOf (v_common_ns, t.Namespace) != -1) {
					Console.WriteLine (t.Name);
					return;
				}
			}
			
			if (t.FullName.StartsWith (prefix)) {
				Console.WriteLine (t.FullName);
			}
		}
		
		foreach (string assm in common_assemblies) {
			try {
				
				Assembly a = GetAssembly (assm, true);
				foreach (Type t in a.GetExportedTypes ()) {
					
					if (t.Name.StartsWith (prefix)) {
						if (Array.IndexOf (common_ns, t.Namespace) != -1) {
							Console.WriteLine (t.Name);
							return;
						}
					}
					
					if (t.FullName.StartsWith (prefix)) {
						Console.WriteLine (t.FullName);
					}
				}
				
			} catch {
			}
		}
		
	}

	static void PrintUsage ()
	{
		Console.WriteLine ("Usage is: monop [option] [-c] [-r:Assembly] [class-name]");
	}

	static void PrintHelp ()
	{
		PrintUsage ();
		Console.WriteLine ("");
		Console.WriteLine ("options:");
		Console.WriteLine ("\t--declared-only,-d\tOnly show members declared in the Type");
		Console.WriteLine ("\t--help,-h\t\tShow this information");
		Console.WriteLine ("\t--private,-p\t\tShow private members");
	}
	
	static void Main (string [] args)
	{
		if (args.Length < 1) {
			PrintUsage ();
			return;
		}

		if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
		{
			PrintHelp ();
			return;

		}
		
		int i = 0;
		if (args [0].StartsWith ("-r:") || args [0].StartsWith ("/r:")){
			i++;
			assembly = args [0].Substring (3);
			
			if (args.Length == 1) {
				PrintTypes (assembly);
				return;
			}
		}
		
		if (args [0] == "-c") {
			Completion (args [1]);
			return;
		}

		if (args [i] == "--private" || args [i] == "-p") {
				default_flags |= BindingFlags.NonPublic;
				i++;
		}

		if (args [i] == "--declared-only" || args [i] == "-d") {
				default_flags |= BindingFlags.DeclaredOnly;
				i++;
		}

		if (args.Length < i + 1) {
			PrintUsage ();
			return;
		}

		string tname = args [i];
		Type t = GetType (tname);

		if (t == null) {
			// Try some very common ones, dont load anything
			foreach (string ns in v_common_ns) {
				t = GetType (ns + "." + tname, true);
				if (t != null)
					goto found;
			}
		}

		string message = null;
		if (t == null) {
			foreach (string assm in GetAssemblyNamesFromGAC ()) {
				try {
					Assembly a = GetAssembly (assm, false);
					t = a.GetType (tname, false, true);
					if (t != null) {
						message = String.Format ("{0} is included in the {1} assembly.",
								t.FullName, 
								t.Assembly.GetName ().Name);
						goto found;
					}
					foreach (string ns in common_ns) {
						t = a.GetType (ns + "." + tname, false, true);
						if (t != null) {
							message = String.Format ("{0} is included in the {1} assembly.",
								t.FullName, 
								t.Assembly.GetName ().Name);
							goto found;
						}
					}
				} catch {
				}
			}
		}
		
		if (t == null) {
			Console.WriteLine ("Could not find {0}", tname);
			return;
		}
	found:
		//
		// This gets us nice buffering
		//
		StreamWriter sw = new StreamWriter (Console.OpenStandardOutput (), Console.Out.Encoding);
		new Outline (t, sw).OutlineType (default_flags);
		sw.Flush ();

		if (message != null)
			Console.WriteLine (message);
	}
}

