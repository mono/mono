//
// permview.cs: Managed Permission Viewer for .NET assemblies
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using SSP = System.Security.Permissions;
using System.Text;

using Mono.Cecil;

[assembly: AssemblyTitle ("Mono PermView")]
[assembly: AssemblyDescription ("Managed Permission Viewer for .NET assemblies")]

namespace Mono.Tools {

	static class SecurityDeclarationRocks {

		public static PermissionSet ToPermissionSet (this SecurityDeclaration self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			PermissionSet set;
			if (TryProcessPermissionSetAttribute (self, out set))
				return set;

			return CreatePermissionSet (self);
		}

		static bool TryProcessPermissionSetAttribute (SecurityDeclaration declaration, out PermissionSet set)
		{
			set = null;

			if (!declaration.HasSecurityAttributes && declaration.SecurityAttributes.Count != 1)
				return false;

			var security_attribute = declaration.SecurityAttributes [0];
			var attribute_type = security_attribute.AttributeType;

			if (attribute_type.Name != "PermissionSetAttribute" || attribute_type.Namespace != "System.Security.Permissions")
				return false;

			var named_argument = security_attribute.Properties [0];
			if (named_argument.Name != "XML")
				throw new NotSupportedException ();

			var attribute = new SSP.PermissionSetAttribute ((SSP.SecurityAction) declaration.Action);
			attribute.XML = (string) named_argument.Argument.Value;

			set = attribute.CreatePermissionSet ();
			return true;
		}

		static PermissionSet CreatePermissionSet (SecurityDeclaration declaration)
		{
			var set = new PermissionSet (SSP.PermissionState.None);

			foreach (var attribute in declaration.SecurityAttributes) {
				var permission = CreatePermission (declaration, attribute);
				set.AddPermission (permission);
			}

			return set;
		}

		static IPermission CreatePermission (SecurityDeclaration declaration, SecurityAttribute attribute)
		{
			var attribute_type = Type.GetType (attribute.AttributeType.FullName);
			if (attribute_type == null)
				throw new ArgumentException ();

			var security_attribute = CreateSecurityAttribute (attribute_type, declaration);
			if (security_attribute == null)
				throw new InvalidOperationException ();

			CompleteSecurityAttribute (security_attribute, attribute);

			return security_attribute.CreatePermission ();
		}

		static void CompleteSecurityAttribute (SSP.SecurityAttribute security_attribute, SecurityAttribute attribute)
		{
			if (attribute.HasFields)
				CompleteSecurityAttributeFields (security_attribute, attribute);

			if (attribute.HasProperties)
				CompleteSecurityAttributeProperties (security_attribute, attribute);
		}

		static void CompleteSecurityAttributeFields (SSP.SecurityAttribute security_attribute, SecurityAttribute attribute)
		{
			var type = security_attribute.GetType ();

			foreach (var named_argument in attribute.Fields)
				type.GetField (named_argument.Name).SetValue (security_attribute, named_argument.Argument.Value);
		}

		static void CompleteSecurityAttributeProperties (SSP.SecurityAttribute security_attribute, SecurityAttribute attribute)
		{
			var type = security_attribute.GetType ();

			foreach (var named_argument in attribute.Properties)
				type.GetProperty (named_argument.Name).SetValue (security_attribute, named_argument.Argument.Value, null);
		}

		static SSP.SecurityAttribute CreateSecurityAttribute (Type attribute_type, SecurityDeclaration declaration)
		{
			SSP.SecurityAttribute security_attribute;
			try {
				security_attribute = (SSP.SecurityAttribute) Activator.CreateInstance (
					attribute_type, new object [] { (SSP.SecurityAction) declaration.Action });
			} catch (MissingMethodException) {
				security_attribute = (SSP.SecurityAttribute) Activator.CreateInstance (attribute_type, new object [0]);
			}

			return security_attribute;
		}
	}

	class SecurityElementComparer : IComparer {

		public int Compare (object x, object y)
		{
			SecurityElement sx = (x as SecurityElement);
			SecurityElement sy = (y as SecurityElement);
			if (sx == null)
				return (sy == null) ? 0 : -1;
			else if (sy == null)
				return 1;

			// compare by name (type name, method name, action name)
			return String.Compare (sx.Attribute ("Name"), sy.Attribute ("Name"));
		}
	}

	class PermView {

		private const string NotSpecified = "\tNot specified.";

		static private void Help () 
		{
			Console.WriteLine ("Usage: permview [options] assembly{0}", Environment.NewLine);
			Console.WriteLine ("where options are:");
			Console.WriteLine (" -output filename  Output information into specified file.");
			Console.WriteLine (" -decl             Show declarative security attributes on classes and methods.");
			Console.WriteLine (" -xml              Output in XML format");
			Console.WriteLine (" -help             Show help informations (this text)");
			Console.WriteLine ();
		}

		static bool declarative = false;
		static bool xmloutput = false;

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
				case "/XML":
				case "-XML":
				case "--XML":
					xmloutput = true;
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

		static bool ProcessAssemblyOnly (TextWriter tw, AssemblyDefinition ad) 
		{
			bool result = true;
			string minimal = NotSpecified + Environment.NewLine;
			string optional = NotSpecified + Environment.NewLine;
			string refused = NotSpecified + Environment.NewLine;

			foreach (SecurityDeclaration decl in ad.SecurityDeclarations) {
				switch (decl.Action) {
				case Mono.Cecil.SecurityAction.RequestMinimum:
					minimal = decl.ToPermissionSet ().ToString ();
					break;
				case Mono.Cecil.SecurityAction.RequestOptional:
					optional = decl.ToPermissionSet ().ToString ();
					break;
				case Mono.Cecil.SecurityAction.RequestRefuse:
					refused = decl.ToPermissionSet ().ToString ();
					break;
				default:
					tw.WriteLine ("Invalid assembly level declaration {0}{1}{2}",
						decl.Action, Environment.NewLine, decl.ToPermissionSet ());
					result = false;
					break;
				}
			}

			tw.WriteLine ("Minimal Permission Set:");
			tw.WriteLine (minimal);
			tw.WriteLine ("Optional Permission Set:");
			tw.WriteLine (optional);
			tw.WriteLine ("Refused Permission Set:");
			tw.WriteLine (refused);
			return result;
		}

		static void ShowSecurity (TextWriter tw, string header, IEnumerable<SecurityDeclaration> declarations)
		{
			foreach (SecurityDeclaration declsec in declarations) {
				tw.WriteLine ("{0} {1} Permission Set:{2}{3}", header,
					declsec.Action, Environment.NewLine, declsec.ToPermissionSet ());
			}
		}

		static bool ProcessAssemblyComplete (TextWriter tw, AssemblyDefinition ad)
		{
			if (ad.SecurityDeclarations.Count > 0) {
				ShowSecurity (tw, "Assembly", ad.SecurityDeclarations);
			}

			foreach (ModuleDefinition module in ad.Modules) {

				foreach (TypeDefinition type in module.Types) {

					if (type.SecurityDeclarations.Count > 0) {
						ShowSecurity (tw, "Class " + type.ToString (), ad.SecurityDeclarations);
					}

					foreach (MethodDefinition method in type.Methods) {
						if (method.SecurityDeclarations.Count > 0) {
							ShowSecurity (tw, "Method " + method.ToString (), method.SecurityDeclarations);
						}
					}
				}
			}
			return true;
		}

		static void AddAttribute (SecurityElement se, string attr, string value)
		{
			value = value.Replace ("&", "&amp;");
			se.AddAttribute (attr, value);
		}

		static SecurityElement AddSecurityXml (IEnumerable<SecurityDeclaration> declarations)
		{
			ArrayList list = new ArrayList ();
			foreach (SecurityDeclaration declsec in declarations) {
				SecurityElement child = new SecurityElement ("Action");
				AddAttribute (child, "Name", declsec.Action.ToString ());
				child.AddChild (declsec.ToPermissionSet ().ToXml ());
				list.Add (child);
			}
			// sort actions
			list.Sort (Comparer);

			SecurityElement se = new SecurityElement ("Actions");
			foreach (SecurityElement child in list) {
				se.AddChild (child);
			}
			return se;
		}

		static SecurityElementComparer comparer;
		static IComparer Comparer {
			get {
				if (comparer == null)
					comparer = new SecurityElementComparer ();
				return comparer;
			}
		}

		static bool ProcessAssemblyXml (TextWriter tw, AssemblyDefinition ad)
		{
			SecurityElement se = new SecurityElement ("Assembly");
			se.AddAttribute ("Name", ad.Name.FullName);

			if (ad.SecurityDeclarations.Count > 0) {
				se.AddChild (AddSecurityXml (ad.SecurityDeclarations));
			}

			ArrayList tlist = new ArrayList ();
			ArrayList mlist = new ArrayList ();

			foreach (ModuleDefinition module in ad.Modules) {

				foreach (TypeDefinition type in module.Types) {

					SecurityElement klass = new SecurityElement ("Class");
					SecurityElement methods = new SecurityElement ("Methods");

					SecurityElement typelem = null;
					if (type.SecurityDeclarations.Count > 0) {
						typelem = AddSecurityXml (type.SecurityDeclarations);
					}

					if (mlist.Count > 0)
						mlist.Clear ();

					foreach (MethodDefinition method in type.Methods) {
						if (method.SecurityDeclarations.Count > 0) {
							SecurityElement meth = new SecurityElement ("Method");
							AddAttribute (meth, "Name", method.ToString ());
							meth.AddChild (AddSecurityXml (method.SecurityDeclarations));
							mlist.Add (meth);
						}
					}

					// sort methods
					mlist.Sort (Comparer);
					foreach (SecurityElement method in mlist) {
						methods.AddChild (method);
					}

					if ((typelem != null) || ((methods.Children != null) && (methods.Children.Count > 0))) {
						AddAttribute (klass, "Name", type.ToString ());
						if (typelem != null)
							klass.AddChild (typelem);
						if ((methods.Children != null) && (methods.Children.Count > 0))
							klass.AddChild (methods);
						tlist.Add (klass);
					}
				}

				// sort types
				tlist.Sort (Comparer);
				foreach (SecurityElement type in tlist) {
					se.AddChild (type);
				}
			}

			tw.WriteLine (se.ToString ());
			return true;
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
				using (AssemblyDefinition ad = AssemblyDefinition.ReadAssembly (assemblyName)) {
					if (ad != null) {
						bool complete = false;

						if (declarative) {
							// full output (assembly+classes+methods)
							complete = ProcessAssemblyComplete (tw, ad);
						} else if (xmloutput) {
							// full output in XML (for easier diffs after c14n)
							complete = ProcessAssemblyXml (tw, ad);
						} else {
							// default (assembly only)
							complete = ProcessAssemblyOnly (tw, ad);
						}

						if (!complete) {
							Console.Error.WriteLine ("Couldn't reflect informations.");
							return 1;
						}
					} else {
						Console.Error.WriteLine ("Couldn't load assembly '{0}'.", assemblyName);
						return 2;
					}
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
