//
// monop -- a semi-clone of javap
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2004 Ben Maurer
//


using System;
using System.Reflection;
using System.Collections;
using System.CodeDom.Compiler;
using System.IO;

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

	static Assembly GetAssembly (string assembly, bool exit)
	{
		Assembly a;

		try {
			// if is starts with / use the full path
			// otherwise try to load from the GAC
			if (assembly.StartsWith ("/"))
				a = Assembly.LoadFrom (assembly);
			else
				a = Assembly.LoadWithPartialName (assembly);

			// if the above failed try Load
			if (a == null)
				a = Assembly.Load (assembly);

			return a;
		}
		catch {
			if (exit) {
				Console.WriteLine ("Could not load {0}", MonoP.assembly);
				Environment.Exit (1);
			}
			return null;
		}
	}

	static Type GetType (string tname)
	{
		return GetType (tname, false);
	}
	
	static void PrintTypes (string assembly)
	{
		Assembly a = GetAssembly (assembly, true);
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
		Console.WriteLine ("Usage is: monop [-c] [-r:Assembly] [class-name] [option]");
	}

	static void PrintHelp ()
	{
		PrintUsage ();
		Console.WriteLine ("");
		Console.WriteLine ("options:");
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
		
		IndentedTextWriter o = new IndentedTextWriter (Console.Out, "    ");

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

		if (args.Length < i+1){
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

		if (t == null) {
			foreach (string assm in common_assemblies) {
				try {
					Assembly a = GetAssembly (assm, false);
					t = a.GetType (tname, false, true);
					if (t != null)
						goto found;
					foreach (string ns in common_ns) {
						t = a.GetType (ns + "." + tname, false, true);
						if (t != null) {
							Console.WriteLine ("(using class from {0})", ns);
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
	}
}

