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

	// Notes:
	// * Oct CTP started to return declarative security attributes with 
	//   GetCustomAttributes, so this wont work with beta1 or previous 2.0 CTP
	// * Nov CTP (and probably Oct CTP too) is bugged and always report 
	//   LinkDemand as the SecurityAction

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

#if NET_2_0
		static PermissionSet GetPermissionSet (SecurityAttribute sa)
		{
			PermissionSet ps = null;
			if (sa is PermissionSetAttribute) {
				ps = (sa as PermissionSetAttribute).CreatePermissionSet ();
			} else {
				ps = new PermissionSet (PermissionState.None);
				IPermission p = sa.CreatePermission ();
				ps.AddPermission (p);
			}
			return ps;
		}
#else
		static PermissionSet GetPermissionSet (Assembly a, string name)
		{
			FieldInfo fi = typeof (Assembly).GetField (name, BindingFlags.Instance | BindingFlags.NonPublic);
			if (fi == null)
				throw new NotSupportedException ("Wrong runtime ?");
			return (PermissionSet) fi.GetValue (a);
		}
#endif

		static bool ProcessAssemblyOnly (TextWriter tw, Assembly a) 
		{
#if NET_2_0
			// This should work for all 2.0 runtime - unless we hit a bug :-(
			object[] attrs = a.GetCustomAttributes (false);
			foreach (object attr in attrs) {
				if (attr is SecurityAttribute) {
					SecurityAttribute sa = (attr as SecurityAttribute);
					switch (sa.Action) {
					case SecurityAction.RequestMinimum:
						ShowPermissionSet (tw, "Minimum Permission Set:", GetPermissionSet (sa));
						break;
					case SecurityAction.RequestOptional:
						ShowPermissionSet (tw, "Optional Permission Set:", GetPermissionSet (sa));
						break;
					case SecurityAction.RequestRefuse:
						ShowPermissionSet (tw, "Refused Permission Set:", GetPermissionSet (sa));
						break;
					default:
						// Bug in VS.NET 2005 Nov CTP - Evrything action is a LinkDemand
						string msg = String.Format ("ERROR {0} Permission Set:", sa.Action);
						ShowPermissionSet (tw, msg, GetPermissionSet (sa));
						break;
					}
				}
			}
#else
			// Note: This will only work using the Mono runtime as we P/Invoke
			// into Mono's corlib to get the required informations.

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
#endif
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
*/
		static void ProcessMethod (TextWriter tw, MethodInfo mi) 
		{
			// no need to process methods without security informations
			if ((mi.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity) {
#if NET_2_0
				object[] attrs = mi.GetCustomAttributes (false);
				foreach (object attr in attrs) {
					if (attr is SecurityAttribute) {
						SecurityAttribute sa = (attr as SecurityAttribute);
						tw.WriteLine ("Method {0} {1} Permission Set", mi, sa.Action);
						ShowPermissionSet (tw, null, GetPermissionSet (sa));
					}
				}
#else
/*				foreach (SecurityAction action in actions) {
					PermissionSet ps = GetDeclarativeSecurity (mi, action);
					if (ps != null) {
						tw.WriteLine ("Method {0} {1} Permission Set", mi, action);
						ShowPermissionSet (tw, null, ps);
					}
				}*/
#endif
			}
		}

		static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
			BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.SetProperty;

		static void ProcessType (TextWriter tw, Type t) 
		{
			// no need to process types without security informations
			if ((t.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.HasSecurity) {
#if NET_2_0
				object[] attrs = t.GetCustomAttributes (false);
				foreach (object attr in attrs) {
					if (attr is SecurityAttribute) {
						SecurityAttribute sa = (attr as SecurityAttribute);
						tw.WriteLine ("Class {0} {1} Permission Set", t, sa.Action);
						ShowPermissionSet (tw, null, GetPermissionSet (sa));
					}
				}
#else
				tw.WriteLine ("Class {0} 'SecurityAction' Permission Set", t);
				// SecurityAction
				ShowPermissionSet (tw, null, null);
#endif
			}
		}

		static bool ProcessAssemblyComplete (TextWriter tw, Assembly a) 
		{
#if NET_2_0
			string header = "Assembly {0} Permission Set";
			object [] attrs = a.GetCustomAttributes (false);
			foreach (object attr in attrs) {
				if (attr is SecurityAttribute) {
					SecurityAttribute sa = (attr as SecurityAttribute);
					// Bug in VS.NET 2005 Nov CTP - Evrything action is a LinkDemand
					ShowPermissionSet (tw, String.Format (header, sa.Action.ToString ()), GetPermissionSet (sa));
				}
			}
#else
			tw.WriteLine ("Currently unsupported");
			return false;
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
				ShowPermissionSet (tw, "Assembly RequestRefuse Permission Set:", ps);*/
#endif
			Type [] types = a.GetTypes ();
			foreach (Type type in types) {
				ProcessType (tw, type);
				foreach (MethodInfo mi in type.GetMethods (flags)) {
					ProcessMethod (tw, mi);
				}
			}

			return true;
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
				Assembly a = Assembly.LoadFile (Path.GetFullPath (assemblyName));
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
				return 3;
			}
			return 0;
		}
	}
}
