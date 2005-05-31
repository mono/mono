//
// mono-shlib-cop.cs: Check unmanaged dependencies
//
// Compile as:
//    mcs mono-shlib-cop.cs -r:Mono.Posix -r:Mono.GetOptions
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005 Jonathan Pryor
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

//
// About:
//    mono-shlib-cop is designed to inspect an assembly and report about
//    potentially erroneous practices.  In particular, this includes:
//      - DllImporting a .so which is a symlink (which typically requires the
//        -devel packages on Linux distros, thus bloating installation and
//        angering users)
//      - etc.
//
// Implementation:
//    - Each assembly needs to be loaded into an AppDomain so that we can
//      adjust the ApplicationBase path (which will allow us to more reliably
//      load assemblies which depend upon assemblies in the same directory).
//      We can share AppDomains (1/directory), but we (alas) can't use a
//      single AppDomain for the entire app.
//    - Thus, algorithm:
//      - Create AppDomain with ApplicationBase path set to directory assembly
//        resides in
//      - Create an AssemblyChecker instance within the AppDomain
//      - Check an assembly with AssemblyChecker; get back AssemblyCheckResults.
//      - Merge results together (to eliminate duplicate messages)
//      - Print results.
//
// TODO:
//    - AppDomain use
//    - Message merging
//    - dllmap loading (need to read $prefix/etc/mono/config and
//      assembly.config files to look up potential maps)
//    - dllmap caching?  (Is it possible to avoid reading the .config file
//      into each AppDomain at least once?  OS file caching may keep perf from
//      dieing with all the potential I/O.)
//    - Make -r work correctly (-r:Mono.Posix should read Mono.Posix from the
//      GAC and inspect it.)
//
#define TRACE

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Mono.GetOptions;
using Mono.Unix;

[assembly: AssemblyTitle ("mono-shlib-cop")]
[assembly: AssemblyCopyright ("(C) 2005 Jonathan Pryor")]
[assembly: AssemblyDescription ("Looks up shared library dependencies of managed code")]
[assembly: Mono.Author ("Jonathan Pryor")]
[assembly: Mono.UsageComplement ("[ASSEMBLY]+ [-r:ASSEMBLY_REF]+")]
[assembly: Mono.ReportBugsTo ("jonpryor@vt.edu")]

namespace Mono.Unmanaged.Check {
	[Serializable]
	sealed class AssemblyCheckResults {
		private ArrayList errors = new ArrayList ();
		private ArrayList warnings = new ArrayList ();

		public string[] Errors {
			get {return (string[]) errors.ToArray (typeof(string));}
		}
		public string[] Warnings {
			get {return (string[]) warnings.ToArray (typeof(string));}
		}

		internal AssemblyCheckResults ()
		{
		}

		internal AssemblyCheckResults (string[] errors, string[] warnings)
		{
			this.errors.AddRange (errors);
			this.warnings.AddRange (warnings);
		}

		internal void Check (Type type)
		{
			BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | 
				BindingFlags.Public | BindingFlags.NonPublic;

			foreach (MemberInfo mi in type.GetMembers (bf)) {
				CheckMember (type, mi);
			}
		}

		private void CheckMember (Type type, MemberInfo mi)
		{
			DllImportAttribute[] dia = null;
			switch (mi.MemberType) {
				case MemberTypes.Constructor: case MemberTypes.Method: {
					MethodBase mb = (MethodBase) mi;
					dia = new DllImportAttribute[]{GetDllImportInfo (mb)};
					break;
				}
				case MemberTypes.Event: {
					EventInfo ei = (EventInfo) mi;
					MethodBase add = ei.GetAddMethod (true);
					MethodBase remove = ei.GetRemoveMethod (true);
					dia = new DllImportAttribute[]{
						GetDllImportInfo (add), GetDllImportInfo (remove)};
					break;
				}
				case MemberTypes.Property: {
					PropertyInfo pi = (PropertyInfo) mi;
					MethodInfo[] accessors = pi.GetAccessors (true);
					if (accessors == null)
						break;
					dia = new DllImportAttribute[accessors.Length];
					for (int i = 0; i < dia.Length; ++i)
						dia [i] = GetDllImportInfo (accessors [i]);
					break;
				}
			}
			if (dia == null)
				return;

			foreach (DllImportAttribute d in dia) {
				if (d == null)
					continue;
				CheckLibrary (type, d.Value);
			}
		}

		private static DllImportAttribute GetDllImportInfo (MethodBase method)
		{
			if (method == null)
				return null;

			if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0)
				return null;

			try {
			// .NET 2.0 synthesizes pseudo-attributes such as DllImport
			DllImportAttribute dia = (DllImportAttribute) Attribute.GetCustomAttribute (method, 
						typeof(DllImportAttribute), false);
			if (dia != null)
				return dia;

			// We're not on .NET 2.0; assume we're on Mono and use some internal
			// methods...
			Type MonoMethod = Type.GetType ("System.Reflection.MonoMethod", false);
			if (MonoMethod == null) {
				return null;
			}
			MethodInfo GetDllImportAttribute = 
				MonoMethod.GetMethod ("GetDllImportAttribute", 
						BindingFlags.Static | BindingFlags.NonPublic);
			if (GetDllImportAttribute == null) {
				return null;
			}
			IntPtr mhandle = method.MethodHandle.Value;
			return (DllImportAttribute) GetDllImportAttribute.Invoke (null, 
					new object[]{mhandle});
			}
			catch (Exception e) {
				Trace.WriteLine ("Exception getting DllImportAttribute: " + e.ToString());
				return null;
			}
		}
		
		[DllImport ("libgmodule-2.0.so")]
		private static extern IntPtr g_module_open (string filename, int flags);
		private static int G_MODULE_BIND_LAZY = 1 << 0;
		private static int G_MODULE_BIND_LOCAL = 1 << 1;
		// private static int G_MODULE_BIND_MASK = 0x03;

		[DllImport ("libgmodule-2.0.so")]
		private static extern int g_module_close (IntPtr handle);

		[DllImport ("libgmodule-2.0.so")]
		private static extern IntPtr g_module_error ();

		[DllImport ("libgmodule-2.0.so")]
		private static extern IntPtr g_module_name (IntPtr h);

		[DllImport ("libgmodule-2.0.so")]
		private static extern IntPtr g_module_build_path (
			string directory, string module_name);

		[DllImport ("libglib-2.0.so")]
		private static extern void g_free (IntPtr mem);

		private void CheckLibrary (Type type, string library)
		{
			Trace.WriteLine ("Trying to load base library: " + library);
			string found = null;
			string error = null;
			string soname = null;
			foreach (string name in GetLibraryNames (type, library)) {
				error = null;
				IntPtr h = g_module_open (name, G_MODULE_BIND_LAZY |
					G_MODULE_BIND_LOCAL);
				try {
					Trace.WriteLine ("    Trying library name: " + name);
					if (h != IntPtr.Zero) {
						found = name;
						soname = Marshal.PtrToStringAnsi (g_module_name (h));
						break;
					}
					error = Marshal.PtrToStringAnsi (g_module_error ());
					Trace.WriteLine ("\tError loading library `" + name + "': " + error);
				}
				finally {
					if (h != IntPtr.Zero)
						g_module_close (h);
				}
			}

			if (found == null) {
				errors.Add ("Unable to load library " + library + ": " + error);
				return;
			}

			Trace.WriteLine ("Able to load library " + library + "; soname=" +
			soname + "; found=" + found);
			// UnixFileInfo f = new UnixFileInfo (soname);
			if (found.EndsWith (".so")) {
				warnings.Add (string.Format ("Type `{0}' depends on potential " + 
					"development library `{1}'.", type.FullName, found));
			}
		}

		private static string[] GetLibraryNames (Type type, string library)
		{
			// TODO: keep in sync with
			// mono/metadata/loader.c:mono_lookup_pinvoke_call
			ArrayList names = new ArrayList ();

			string dll_map = GetDllMapEntry (type);
			if (dll_map != null) 
				names.Add (dll_map);

			names.Add (library);
			int _dll_index = library.LastIndexOf (".dll");
			if (_dll_index >= 0)
				names.Add (library.Substring (0, _dll_index));

			if (!library.StartsWith ("lib"))
				names.Add ("lib" + library);

			IntPtr s = g_module_build_path (null, library);
			if (s != IntPtr.Zero) {
				try {
					names.Add (Marshal.PtrToStringAnsi (s));
				}
				finally {
					g_free (s);
				}
			}

			s = g_module_build_path (".", library);
			if (s != IntPtr.Zero) {
				try {
					names.Add (Marshal.PtrToStringAnsi (s));
				}
				finally {
					g_free (s);
				}
			}

			return (string[]) names.ToArray (typeof(string));
		}

		private static string GetDllMapEntry (Type type)
		{
			// TODO
			return null;
		}
	}

	sealed class AssemblyChecker : MarshalByRefObject {

		public AssemblyCheckResults CheckFile (string file)
		{
			try {
				return Check (Assembly.LoadFile (file));
			}
			catch (FileNotFoundException e) {
				return new AssemblyCheckResults (
					new string[]{"Could not load `" + file + "': " + e.Message},
					new string[]{}
				);
			}
		}

		private AssemblyCheckResults Check (Assembly a)
		{
			AssemblyCheckResults r = new AssemblyCheckResults ();
			foreach (Type t in a.GetTypes ()) {
				r.Check (t);
			}
			return r;
		}

		public AssemblyCheckResults CheckWithPartialName (string partial)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = partial;
			try {
				Assembly a = Assembly.Load (an);
				return Check (a);
			}
			catch (FileNotFoundException e) {
				return new AssemblyCheckResults (
					new string[]{"Could not load assembly reference `" + partial + "': " + e.Message}, 
					new string[]{}
				);
			}
		}
	}

	class MyOptions : Options {
		[Option (int.MaxValue, "Assemblies to load by partial names (e.g. from the GAC)", 'r')]
		public string[] references = new string[]{};

	}

	class Runner {
		public static void Main (string[] args)
		{
#if TEST
			AssemblyCheckResults r = new AssemblyCheckResults ();
			foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
				Trace.WriteLine ("Checking Type: " + t.FullName);
				r.Check (t);
			}

			PrintResults (r);
#else
			MyOptions o = new MyOptions ();
			o.ProcessArgs (args);

			AssemblyChecker checker = new AssemblyChecker ();
			foreach (string assembly in o.RemainingArguments) {
				PrintResults (checker.CheckFile (assembly));
			}

			foreach (string assembly in o.references) {
				PrintResults (checker.CheckWithPartialName (assembly));
			}
#endif
		}

		private static void PrintResults (AssemblyCheckResults acr)
		{
			foreach (string s in acr.Errors)
				Console.WriteLine ("error: " + s);
			foreach (string s in acr.Warnings)
				Console.WriteLine ("warning: " + s);
		}
	}
}

