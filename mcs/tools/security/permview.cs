//
// permview.cs: Managed Permission Viewer for .NET assemblies
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

[assembly: AssemblyTitle ("Mono PermView")]
[assembly: AssemblyDescription ("Managed Permission Viewer for .NET assemblies")]

namespace Mono.Tools {

	// There is no "managed way" to get this information using Fx 1.0/1.1
	// so we must reflect inside Mono's corlib to find it. This also means
	// that this won't work under MS runtime. Hopefully this will change
	// with Fx 2.0 and Mono's PermView 2.0 should be working on both runtime.

	class PermView {

		static private void Help () 
		{
			Console.WriteLine ("Usage: permview [options] assembly{0}", Environment.NewLine);
			Console.WriteLine ("where options are:");
			Console.WriteLine (" /OUTPUT filename  Output information into specified file.");
			Console.WriteLine (" /DECL             Show declarative security attributes on classes and methods.");
			Console.WriteLine (" /HELP             Show help informations (this text)");
			Console.WriteLine ();
		}

		static bool declarative = false;

		static void ShowPermissionSet (TextWriter tw, string header, PermissionSet ps)
		{
			if (header != null)
				tw.WriteLine (header);

			if ((ps == null) || (ps.Count == 0)) {
				tw.WriteLine ("\tNone");
			} else {
				tw.WriteLine (ps.ToString ());
			}

			tw.WriteLine ();
		}

		static PermissionSet GetPermissionSet (Assembly a, string name)
		{
			FieldInfo fi = typeof (Assembly).GetField (name, BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi == null)
				throw new NotSupportedException ("Wrong runtime ?");
			return (PermissionSet) fi.GetValue (a);
		}

		static bool ProcessAssemblyOnly (TextWriter tw, Assembly a) 
		{
			Type t = typeof (Assembly);

			// Minimum, Optional and Refuse permission set are only evaluated
			// on demand (delayed as much as possible). A call Resolve will
			// trigger their retrieval from the assembly metadata
			MethodInfo resolve = t.GetMethod ("Resolve", BindingFlags.Instance | BindingFlags.NonPublic);
			if (resolve == null)
				return false;
			resolve.Invoke (a, null);

			ShowPermissionSet (tw, "Minimal Permission Set:", GetPermissionSet (a, "_minimum"));
			ShowPermissionSet (tw, "Optional Permission Set:", GetPermissionSet (a, "_optional"));
			ShowPermissionSet (tw, "Refused Permission Set:", GetPermissionSet (a, "_refuse"));

			return true;
		}

/*		static SecurityAction[] actions = {
			SecurityAction.LinkDemand,
			SecurityAction.InheritanceDemand,
			SecurityAction.Demand,
			(SecurityAction) 13, 				// Hack for NonCasDemand
			(SecurityAction) 14, 				// Hack for NonCasLinkDemand
			(SecurityAction) 15, 				// Hack for NonCasInheritanceDemand
			SecurityAction.Assert,
			SecurityAction.Deny,
			SecurityAction.PermitOnly,
#if NET_2_0
			SecurityAction.LinkDemandChoice,
			SecurityAction.InheritanceDemandChoice,
			SecurityAction.DemandChoice,
#endif
		};

		static MethodInfo method_getdeclsec;

		static PermissionSet GetDeclarativeSecurity (MethodInfo mi, SecurityAction action)
		{
			if (method_getdeclsec == null) {
				Type t = typeof (Int32).Assembly.GetType ("System.Reflection.MonoMethod");
				method_getdeclsec = t.GetMethod ("GetDeclarativeSecurity", BindingFlags.Instance | BindingFlags.Public);
			}
			return (PermissionSet) method_getdeclsec.Invoke (mi, new object [1] { action });
		}

		static void ProcessMethod (TextWriter tw, MethodInfo mi) 
		{
			if ((mi.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity) {
				foreach (SecurityAction action in actions) {
					PermissionSet ps = GetDeclarativeSecurity (mi, action);
					if (ps != null) {
						tw.WriteLine ("Method {0} {1} Permission Set", mi, action);
						ShowPermissionSet (tw, null, ps);
					}
				}
			}
		}

		static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
			BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.SetProperty;

		static void ProcessType (TextWriter tw, Type t) 
		{
			if ((t.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.HasSecurity) {
				tw.WriteLine ("Class {0} 'SecurityAction' Permission Set", t);
				// SecurityAction
				ShowPermissionSet (tw, null, null);
			}
			// Methods
			foreach (MethodInfo mi in t.GetMethods (flags)) {
				ProcessMethod (tw, mi);
			}
		}*/

		static bool ProcessAssemblyComplete (TextWriter tw, Assembly a) 
		{
/*			Type t = typeof (Assembly);
			FieldInfo fi = t.GetField ("_minimum", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi == null)
				return false;
			PermissionSet ps = (PermissionSet) fi.GetValue (a);
			if (ps != null)
				ShowPermissionSet (tw, "Assembly RequestMinimum Permission Set:", ps);
			
			fi = t.GetField ("_optional", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi == null)
				return false;
			ps = (PermissionSet) fi.GetValue (a);
			if (ps != null)
				ShowPermissionSet (tw, "Assembly RequestOptional Permission Set:", ps);

			fi = t.GetField ("_refuse", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi == null)
				return false;
			ps = (PermissionSet) fi.GetValue (a);
			if (ps != null)
				ShowPermissionSet (tw, "Assembly RequestRefuse Permission Set:", ps);

			Type[] types = a.GetTypes ();
			foreach (Type type in types) {
				ProcessType (tw, type);
			}
			return true; */

			tw.WriteLine ("Currently unsupported");
			return false;
		}

		static TextWriter ProcessOptions (string[] args)
		{
			TextWriter tw = Console.Out;
			for (int i=0; i < args.Length - 1; i++) {
				switch (args [i].ToUpper ()) {
				case "/DECL":
				case "-DECL":
				case "--DECL":
					declarative = true;
					break;
				case "/OUTPUT":
				case "-OUTPUT":
				case "--OUTPUT":
					tw = (TextWriter) new StreamWriter (args [++i]);
					break;
				case "/HELP":
				case "/H":
				case "-HELP":
				case "-H":
				case "--HELP":
				case "--H":
				case "-?":
				case "--?":
					Help ();
					return null;
				}
			}
			return tw;
		}

		[STAThread]
		static int Main (string[] args) 
		{
			try {
				Console.WriteLine (new AssemblyInfo ().ToString ());
				if (args.Length == 0) {
					Help ();
					return 0;
				}

				TextWriter tw = ProcessOptions (args);
				if (tw == null)
					return 0;

				string assemblyName = args [args.Length - 1];
				Assembly a = Assembly.LoadFile (assemblyName);
				if (a != null) {
					bool complete = (declarative ?
						ProcessAssemblyComplete (tw, a) :
						ProcessAssemblyOnly (tw, a));
					if (!complete) {
						Console.Error.WriteLine ("Couldn't reflect informations. Wrong runtime ?");
						return 1;
					}
				} else {
					Console.Error.WriteLine ("Couldn't load assembly '{0}'.", assemblyName);
					return 2;
				}
				tw.Close ();
			}
			catch (Exception e) {
				Console.Error.WriteLine ("Error: " + e.ToString ());
				Help ();
			}
			return 0;
		}
	}
}
