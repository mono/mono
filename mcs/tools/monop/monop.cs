//
// monop -- a semi-clone of javap
//
// TODO:
//   Dump all attributes.
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	John Luke  (john.luke@gmail.com)
//
// (C) 2004 Ben Maurer
// (C) 2004 John Luke
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
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.IO;
using IKVM.Reflection;
using System.Text;
using Mono.CSharp;
using Type=IKVM.Reflection.Type;

class MonoP {
	static Universe universe = new Universe(UniverseOptions.EnableFunctionPointers | UniverseOptions.ResolveMissingMembers | UniverseOptions.DisablePseudoCustomAttributeRetrieval);
	static Assembly mscorlib;
	static Type obsolete_attribute;
	static string assembly;
	
	// very common namespaces, all in corlib
	static readonly string [] v_common_ns = {
		"System",
		"System.Collections",
		"System.Reflection",
		"System.Text",
		"System.IO",
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
		"Foundation",
		"CoreFoundation",
		"CoreGraphics",
		"UIKit",
		"Gtk",
		"GLib",
	};
	
	static Type GetType (string tname, bool ignoreCase)
	{
		Type t;
		if (assembly != null) {
			Assembly a = GetAssembly (assembly, true);
			t = a.GetType (tname, false, ignoreCase);
		} else {
			t = mscorlib.GetType (tname, false, ignoreCase);
		}
		return t;
	}
	
	static string SearchTypes (string name, ref Type retval, out int count)
	{
		StringBuilder sb = new StringBuilder ();
		Type current = null;
		count = 0;

		string [] assemblies = GetKnownAssemblyNames ();
		for (int i = 0; i < assemblies.Length; i++) {
			Console.WriteLine ("Loading {0}", assemblies[i]);
			Assembly a = GetAssembly (assemblies [i], false);
			if (a == null)
				continue;

			Type [] types = a.GetTypes ();
			for (int j = 0; j < types.Length; j++) {
				Type t = types [j];
				if (t.IsPublic == false)
					continue;
				
				if (t.Name == name || t.Name.ToLower ().IndexOf (name.ToLower ()) > 0) {
					current = t;
					count ++;
					sb.Append (t.FullName + " from " + a.Location + "\n");
				}
			}
		}

		if (count == 0) 
			return null;

		if (count == 1) {
			retval = current;
			return String.Empty;
		}
		
		return sb.ToString ();
	}

	static string [] GetKnownAssemblyNames ()
	{
		Console.WriteLine (options.PublicDir);
		if (options.Style == "xios" || options.Style == "xand"){
			return Directory.GetFiles (options.PublicDir, "*.dll");
		}
		
		Process p = new Process ();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.FileName = "gacutil";
		p.StartInfo.Arguments = "-l";
		try {
			p.Start ();
		}
		catch {
			Console.WriteLine ("WARNING: gacutil could not be found.");
			return new string[0];
		}

		string s;
		ArrayList names = new ArrayList ();
		StreamReader output = p.StandardOutput;

		while ((s = output.ReadLine ()) != null)
			names.Add (s);

		p.WaitForExit ();
		
		int length = names.Count - 1;
		string [] retval = new string [length];
		retval [0] = typeof (Object).Assembly.FullName;		
		names.CopyTo (1, retval, 1, length - 1); // skip the first and last line
		return retval;
	}

	static Assembly GetAssembly (string assembly, bool exit)
	{
		Assembly a = null;

		// if -r:~/foo.dll syntax is used the shell misses it
		if (assembly.StartsWith ("~/"))
			assembly = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), assembly.Substring (2));

		try {
			// if it exists try to use LoadFrom
			if (File.Exists (assembly))
				a = universe.LoadFile (assembly);
			else
				a = LoadFromMonoPath (assembly);
		} catch {
			// ignore exception it gets handled below
		}

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
		foreach (string path in paths) {	
			string apath = Path.Combine (path, assembly);
			if (File.Exists (apath))
				return universe.LoadFile (apath);	
		}
		return null;
	}

	static Type GetType (string tname)
	{
		return GetType (tname, false);
	}

	static void PrintRefs (string assembly)
	{
		Assembly a = GetAssembly (assembly, true);
		foreach (AssemblyName an in a.GetReferencedAssemblies ())
			Console.WriteLine (an);
	}

	static void PrintTypes (string assembly, bool show_private, bool filter_obsolete)
	{
		Assembly a = GetAssembly (assembly, true);

		Console.WriteLine ();
		Console.WriteLine ("Assembly Information:");

		foreach (string ai in a.ToString ().Split (','))
			Console.WriteLine (ai.Trim ());
			
		Console.WriteLine ();
		Type [] types = show_private ? a.GetTypes () : a.GetExportedTypes ();
		Array.Sort (types, new TypeSorter ());

		int obsolete_count = 0;
		foreach (Type t in types) {
			if (filter_obsolete && t.IsDefined (obsolete_attribute, false))
				obsolete_count++;
			else
				Console.WriteLine (t.FullName);
		}

		Console.WriteLine ("\nTotal: {0} types.", types.Length - obsolete_count);
	}
	
	internal static void Completion (string prefix)
	{
		foreach (Type t in mscorlib.GetExportedTypes ()) {
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

	static void ShowAll (string assembly, bool show_private, bool filter_obsolete)
	{
		Assembly a = GetAssembly (assembly, true);

		foreach (string ai in a.ToString ().Split (','))
			Console.WriteLine (ai.Trim ());
			
		Console.WriteLine ();
		Type [] types = show_private ? a.GetTypes () : a.GetExportedTypes ();
		Array.Sort (types, new TypeSorter ());
		
		var sw = new StreamWriter (Console.OpenStandardOutput (), Console.Out.Encoding);				
		foreach (Type t in types) {
			if (filter_obsolete && t.IsDefined (obsolete_attribute, false))
				continue;

			new Outline (universe, mscorlib, t, sw, true, show_private, filter_obsolete).OutlineType ();
		}
		sw.Flush ();
	}

	static Options options = new Options ();
	
	static int Main (string [] args)
	{
		if (!options.ProcessArgs (args))
			return 1;
		
		if (options.Style == null)
			mscorlib = universe.LoadFile (typeof (int).Assembly.Location);
		else
			mscorlib = universe.LoadFile (Path.Combine (options.PublicDir, "mscorlib.dll"));
						      
		obsolete_attribute = mscorlib.GetType ("System.ObsoleteAttribute");
		
		if (options.AssemblyReference != null) {
			assembly = options.AssemblyReference;

			if (options.ShowAll){
				ShowAll (assembly, options.ShowPrivate, options.FilterObsolete);
				return 0;
			} else {
				if (options.Type == null) {
					if (options.PrintRefs)
						PrintRefs (assembly);
					else
						PrintTypes (assembly, options.ShowPrivate, options.FilterObsolete);
					return 0;
				}
			}
		}
		
		string message = null;
		string tname = options.Type;
		Type t = null;
		int count;


		if (options.Search) {
			string matches = SearchTypes (tname, ref t, out count);

			if (count == 0)
				goto notfound;

			if (count == 1)
				goto found;
			
			if (count > 1){
				Console.WriteLine ("Found " + count + " types that match:");
				Console.WriteLine (matches);
				return 0;
			}
		}
			
		t = GetType (tname);

		if (t == null) {
			// Try some very common ones, dont load anything
			foreach (string ns in v_common_ns) {
				t = GetType (ns + "." + tname, true);
				if (t != null)
					goto found;
			}
		}

		if (t == null) {
			foreach (string assm in GetKnownAssemblyNames ()) {
				try {
					Assembly a = GetAssembly (assm, false);
					if (a == null)
						continue;
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
				} catch (Exception e){
					Console.WriteLine ("Failure: " + e);
				}
			}
		}

	notfound:
		if (t == null) {
			Console.WriteLine ("Could not find {0}", tname);
			return 1;
		}
	found:
		//
		// This gets us nice buffering
		//
		StreamWriter sw = new StreamWriter (Console.OpenStandardOutput (), Console.Out.Encoding);				
		new Outline (universe, mscorlib, t, sw, options.DeclaredOnly, options.ShowPrivate, options.FilterObsolete).OutlineType ();
		sw.Flush ();

		if (message != null)
			Console.WriteLine (message);
		return 0;
	}
}

