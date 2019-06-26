//
// mono-shlib-cop.cs: Check unmanaged dependencies
//
// Compile as:
//    mcs mono-shlib-cop.cs ../../class/Mono.Options/Mono.Options/Options.cs -r:Mono.Posix
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//  Jonathan Pryor (jpryor@novell.com)
//
// (C) 2005 Jonathan Pryor
// (C) 2008 Novell, Inc.
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
//      - DllImporting a .so which may be a symlink (which typically requires the
//        -devel packages on Linux distros, thus bloating installation and
//        angering users)
//      - DllImporting a symbol which doesn't exist in the target library
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
//      - Check an assembly with AssemblyChecker; store results in AssemblyCheckInfo.
//      - Print results.
//
// TODO:
//    - AppDomain use
//    - Make -r work correctly (-r:Mono.Posix should read Mono.Posix from the
//      GAC and inspect it.)
//
#define TRACE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

using Mono.Options;
using Mono.Unix;

[assembly: AssemblyTitle ("mono-shlib-cop")]
[assembly: AssemblyCopyright ("(C) 2005 Jonathan Pryor")]
[assembly: AssemblyDescription ("Looks up shared library dependencies of managed code")]

namespace Mono.Unmanaged.Check {
	[Serializable]
	sealed class MessageInfo {
		public string Type;
		public string Member;
		public string Message;

		public MessageInfo (string type, string member, string message)
		{
			Type = type;
			Member = member;
			Message = message;
		}

		public override bool Equals (object value)
		{
			MessageInfo other = value as MessageInfo;
			if (other == null)
				return false;

			return Type == other.Type && Member == other.Member && 
				Message == other.Message;
		}

		public override int GetHashCode ()
		{
			return Type.GetHashCode () ^ Member.GetHashCode () ^ 
				Message.GetHashCode ();
		}
	}

	sealed class MessageCollection : MarshalByRefObject {
		private ArrayList InnerList = new ArrayList ();

		public MessageCollection ()
		{
		}

		public int Add (MessageInfo value)
		{
			if (!InnerList.Contains (value))
				return InnerList.Add (value);
			return InnerList.IndexOf (value);
		}

		public void AddRange (MessageInfo[] value)
		{
			foreach (MessageInfo v in value)
				Add (v);
		}

		public void AddRange (MessageCollection value)
		{
			foreach (MessageInfo v in value)
				Add (v);
		}

		public bool Contains (MessageInfo value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (MessageInfo[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (MessageInfo value)
		{
			return InnerList.IndexOf (value);
		}

		public void Insert (int index, MessageInfo value)
		{
			InnerList.Insert (index, value);
		}

		public void Remove (MessageInfo value)
		{
			InnerList.Remove (value);
		}

		public IEnumerator GetEnumerator ()
		{
			return InnerList.GetEnumerator ();
		}
	}

	sealed class AssemblyCheckInfo : MarshalByRefObject {
		private MessageCollection errors   = new MessageCollection ();
		private MessageCollection warnings = new MessageCollection ();

		public MessageCollection Errors {
			get {return errors;}
		}

		public MessageCollection Warnings {
			get {return warnings;}
		}

		private XmlDocument[] mono_configs = new XmlDocument [0];
		private IDictionary assembly_configs = new Hashtable ();

		public void SetInstallationPrefixes (IList<string> prefixes)
		{
			mono_configs = new XmlDocument [prefixes.Count];
			for (int i = 0; i < mono_configs.Length; ++i) {
				mono_configs [i] = new XmlDocument ();
				mono_configs [i].Load (Path.Combine (prefixes [i], "etc/mono/config"));
			}
		}

		public string GetDllmapEntry (string assemblyPath, string library)
		{
			string xpath = "/configuration/dllmap[@dll=\"" + library + "\"]";

			XmlDocument d = GetAssemblyConfig (assemblyPath);
			if (d != null) {
				XmlNode map = d.SelectSingleNode (xpath);
				if (map != null)
					return map.Attributes ["target"].Value;
			}
			foreach (XmlDocument config in mono_configs) {
				XmlNode map = config.SelectSingleNode (xpath);
				if (map != null)
					return map.Attributes ["target"].Value;
			}
			return null;
		}

		private XmlDocument GetAssemblyConfig (string assemblyPath)
		{
			XmlDocument d = null;
			if (assembly_configs.Contains (assemblyPath)) {
				d = (XmlDocument) assembly_configs [assemblyPath];
			}
			else {
				string _config = assemblyPath + ".config";
				if (File.Exists (_config)) {
					d = new XmlDocument ();
					d.Load (_config);
				}
				assembly_configs.Add (assemblyPath, d);
			}
			return d;
		}
	}

	sealed class AssemblyChecker : MarshalByRefObject {

		public void CheckFile (string file, AssemblyCheckInfo report)
		{
			try {
				Check (Assembly.LoadFile (Path.GetFullPath (file)), report);
			}
			catch (FileNotFoundException e) {
				report.Errors.Add (new MessageInfo (null, null, 
					"Could not load `" + file + "': " + e.Message));
			}
		}

		public void CheckWithPartialName (string partial, AssemblyCheckInfo report)
		{
			string p = partial;
			Assembly a;
			bool retry;

			do {
				a = Assembly.LoadWithPartialName (p);
				retry = p.EndsWith (".dll");
				if (retry) {
					p = p.Substring (0, p.Length-4);
				}
			} while (a == null && retry);

			if (a == null) {
				report.Errors.Add (new MessageInfo (null, null, 
					"Could not load assembly reference `" + partial + "'."));
				return;
			}

			Check (a, report);
		}

		private void Check (Assembly a, AssemblyCheckInfo report)
		{
			foreach (Type t in a.GetTypes ()) {
				Check (t, report);
			}
		}

		private void Check (Type type, AssemblyCheckInfo report)
		{
			BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | 
				BindingFlags.Public | BindingFlags.NonPublic;

			foreach (MemberInfo mi in type.GetMembers (bf)) {
				CheckMember (type, mi, report);
			}
		}

		private void CheckMember (Type type, MemberInfo mi, AssemblyCheckInfo report)
		{
			DllImportAttribute[] attributes = null;
			MethodBase[] methods = null;
			switch (mi.MemberType) {
				case MemberTypes.Constructor: case MemberTypes.Method: {
					MethodBase mb = (MethodBase) mi;
					attributes = new DllImportAttribute[]{GetDllImportInfo (mb)};
					methods = new MethodBase[]{mb};
					break;
				}
				case MemberTypes.Event: {
					EventInfo ei = (EventInfo) mi;
					MethodBase add = ei.GetAddMethod (true);
					MethodBase remove = ei.GetRemoveMethod (true);
					attributes = new DllImportAttribute[]{
						GetDllImportInfo (add), GetDllImportInfo (remove)};
					methods = new MethodBase[]{add, remove};
					break;
				}
				case MemberTypes.Property: {
					PropertyInfo pi = (PropertyInfo) mi;
					MethodInfo[] accessors = pi.GetAccessors (true);
					if (accessors == null)
						break;
					attributes = new DllImportAttribute[accessors.Length];
					methods = new MethodBase [accessors.Length];
					for (int i = 0; i < accessors.Length; ++i) {
						attributes [i] = GetDllImportInfo (accessors [i]);
						methods [i] = accessors [i];
					}
					break;
				}
			}
			if (attributes == null || methods == null)
				return;

			for (int i = 0; i < attributes.Length; ++i) {
				if (attributes [i] == null)
					continue;
				CheckLibrary (methods [i], attributes [i], report);
			}
		}

		private static DllImportAttribute GetDllImportInfo (MethodBase method)
		{
			if (method == null)
				return null;

			if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0)
				return null;

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
		
		private void CheckLibrary (MethodBase method, DllImportAttribute attribute, 
				AssemblyCheckInfo report)
		{
			string library = attribute.Value;
			string entrypoint = attribute.EntryPoint;
			string type = method.DeclaringType.FullName;
			string mname = method.Name;

			string found = null;
			string error = null;

			Trace.WriteLine ("Trying to load base library: " + library);

			foreach (string name in GetLibraryNames (method.DeclaringType, library, report)) {
				if (LoadLibrary (type, mname, name, entrypoint, report, out error)) {
					found = name;
					break;
				}
			}

			if (found == null) {
				report.Errors.Add (new MessageInfo (
							type, mname,
							"Could not load library `" + library + "': " + error));
				return;
			}

			// UnixFileInfo f = new UnixFileInfo (soname);
			if (found.EndsWith (".so")) {
				report.Warnings.Add (new MessageInfo (type, mname, 
						string.Format ("Library `{0}' might be a development library",
							found)));
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

		[DllImport ("libgmodule-2.0.so")]
		private static extern int g_module_symbol (IntPtr module, 
				string symbol_name, out IntPtr symbol);

		[DllImport ("libglib-2.0.so")]
		private static extern void g_free (IntPtr mem);

		private static string[] GetLibraryNames (Type type, string library, AssemblyCheckInfo report)
		{
			// TODO: keep in sync with
			// mono/metadata/loader.c:mono_lookup_pinvoke_call
			ArrayList names = new ArrayList ();

			string dll_map = report.GetDllmapEntry (type.Assembly.Location, library);
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

		private static bool LoadLibrary (string type, string member, 
				string library, string symbol, AssemblyCheckInfo report, out string error)
		{
			error = null;
			IntPtr h = g_module_open (library, 
					G_MODULE_BIND_LAZY | G_MODULE_BIND_LOCAL);
			try {
				Trace.WriteLine ("    Trying library name: " + library);
				if (h != IntPtr.Zero) {
					string soname = Marshal.PtrToStringAnsi (g_module_name (h));
					Trace.WriteLine ("Able to load library " + library + 
							"; soname=" + soname);
					IntPtr ignore;
					if (g_module_symbol (h, symbol, out ignore) == 0)
						report.Errors.Add (new MessageInfo (
								type, member,
								string.Format ("library `{0}' is missing symbol `{1}'",
									library, symbol)));
					return true;
				}
				error = Marshal.PtrToStringAnsi (g_module_error ());
				Trace.WriteLine ("\tError loading library `" + library + "': " + error);
			}
			finally {
				if (h != IntPtr.Zero)
					g_module_close (h);
			}
			return false;
		}
	}

	class Runner {

		public static void Main (string[] args)
		{
			var references = new List<string> ();
			var prefixes   = new List<string> ();

			List<string> files = new OptionSet {
				{ "p|prefix|prefixes=",
				  "Mono installation prefixes (for $prefix/etc/mono/config)",
				  v => prefixes.Add (v) },
				{ "r|reference|references=",
				  "Assemblies to load by partial names (e.g. from the GAC)",
					v => references.Add (v) },
			}.Parse (args);

			AssemblyChecker checker = new AssemblyChecker ();
			AssemblyCheckInfo report = new AssemblyCheckInfo ();
			if (prefixes.Count == 0) {
				// SystemConfigurationFile is $sysconfdir/mono/VERSION/machine.config
				// We want $sysconfdir
				DirectoryInfo configDir = 
					new FileInfo (RuntimeEnvironment.SystemConfigurationFile).Directory.Parent.Parent.Parent;
				prefixes.Add (configDir.ToString ());
			}
			report.SetInstallationPrefixes (prefixes);
			foreach (string assembly in files) {
				checker.CheckFile (assembly, report);
			}

			foreach (string assembly in references) {
				checker.CheckWithPartialName (assembly, report);
			}

			foreach (MessageInfo m in report.Errors) {
				PrintMessage ("error", m);
			}

			foreach (MessageInfo m in report.Warnings) {
				PrintMessage ("warning", m);
			}
		}

		private static void PrintMessage (string type, MessageInfo m)
		{
			Console.Write ("{0}: ", type);
			if (m.Type != null)
				Console.Write ("in {0}", m.Type);
			if (m.Member != null)
				Console.Write (".{0}: ", m.Member);
			Console.WriteLine (m.Message);
		}
	}
}

