//
// System.Web.UI.TemplateParser
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	public abstract class TemplateParser : BaseParser
	{
		string inputFile;
		string text;
		string privateBinPath;
		Hashtable mainAttributes;
		ArrayList dependencies;
		ArrayList assemblies;
		Hashtable anames;
		ArrayList imports;
		ArrayList interfaces;
		ArrayList scripts;
		Type baseType;
		string className;
		RootBuilder rootBuilder;
		bool debug;
		string compilerOptions;
		string language;

		public TemplateParser ()
		{
			imports = new ArrayList ();
			imports.Add ("System");
			imports.Add ("System.Collections");
			imports.Add ("System.Collections.Specialized");
			imports.Add ("System.Configuration");
			imports.Add ("System.Text");
			imports.Add ("System.Text.RegularExpressions");
			imports.Add ("System.Web");
			imports.Add ("System.Web.Caching");
			imports.Add ("System.Web.Security");
			imports.Add ("System.Web.SessionState");
			imports.Add ("System.Web.UI");
			imports.Add ("System.Web.UI.WebControls");
			imports.Add ("System.Web.UI.HtmlControls");

			assemblies = new ArrayList ();
			assemblies.Add ("System.dll");
			assemblies.Add ("System.Drawing.dll");
			assemblies.Add ("System.Data.dll");
			assemblies.Add ("System.Web.dll");
			assemblies.Add ("System.Xml.dll");
			AddAssembliesInBin ();
		}

		protected abstract Type CompileIntoType ();

		protected virtual void HandleOptions (object obj)
		{
		}

		internal static string GetOneKey (Hashtable tbl)
		{
			foreach (object key in tbl.Keys)
				return key.ToString ();

			return null;
		}
		
		internal virtual void AddDirective (string directive, Hashtable atts)
		{
			if (String.Compare (directive, DefaultDirectiveName, true) == 0) {
				if (mainAttributes != null)
					ThrowParseException ("Only 1 " + DefaultDirectiveName + " is allowed");

				mainAttributes = atts;
				ProcessMainAttributes (mainAttributes);
				return;
			}

			int cmp = String.Compare ("Assembly", directive, true);
			if (cmp == 0) {
				string name = GetString (atts, "Name", null);
				string src = GetString (atts, "Src", null);

				if (atts.Count > 0)
					ThrowParseException ("Attribute " + GetOneKey (atts) + " unknown.");

				if (name == null && src == null)
					ThrowParseException ("You gotta specify Src or Name");
					
				if (name != null && src != null)
					ThrowParseException ("Src and Name cannot be used together");

				if (name != null) {
					AddAssemblyByName (name);
				} else {
					GetAssemblyFromSource (src);
				}

				return;
			}

			cmp = String.Compare ("Import", directive, true);
			if (cmp == 0) {
				string namesp = GetString (atts, "Namespace", null);
				if (atts.Count > 0)
					ThrowParseException ("Attribute " + GetOneKey (atts) + " unknown.");
				
				if (namesp != null && namesp != "")
					AddImport (namesp);
				return;
			}

			cmp = String.Compare ("Implements", directive, true);
			if (cmp == 0) {
				string ifacename = GetString (atts, "Interface", "");

				if (atts.Count > 0)
					ThrowParseException ("Attribute " + GetOneKey (atts) + " unknown.");
				
				Type iface = LoadType (ifacename);
				if (iface == null)
					ThrowParseException ("Cannot find type " + ifacename);

				if (!iface.IsInterface)
					ThrowParseException (iface + " is not an interface");

				AddInterface (iface.FullName);
				return;
			}

			ThrowParseException ("Unknown directive: " + directive);
		}
		
		internal Type LoadType (string typeName)
		{
			// First try loaded assemblies, then try assemblies in Bin directory.
			// By now i do this 'by hand' but may be this is a runtime/gac task.
			Type type = null;
			Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly ass in assemblies) {
				type = ass.GetType (typeName);
				if (type != null) {
					AddAssembly (ass, false);
					AddDependency (ass.Location);
					return type;
				}
			}

			return null;
		}

		void AddAssembliesInBin ()
		{
			if (!Directory.Exists (PrivateBinPath))
				return;

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string dll in binDlls) {
				Assembly assembly = Assembly.LoadFrom (dll);
				AddAssembly (assembly, true);
			}
		}

		Assembly LoadAssemblyFromBin (string name)
		{
			Assembly assembly;
			if (!Directory.Exists (PrivateBinPath))
				return null;

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string dll in binDlls) {
				string fn = Path.GetFileName (dll);
				fn = Path.ChangeExtension (fn, null);
				if (fn != name)
					continue;

				assembly = Assembly.LoadFrom (dll);
				return assembly;
			}

			return null;
		}

		internal virtual void AddInterface (string iface)
		{
			if (interfaces == null)
				interfaces = new ArrayList ();

			if (!interfaces.Contains (iface))
				interfaces.Add (iface);
		}
		
		internal virtual void AddImport (string namesp)
		{
			if (imports == null)
				imports = new ArrayList ();

			if (!imports.Contains (namesp))
				imports.Add (namesp);
		}
		
		internal virtual void AddDependency (string filename)
		{
			if (dependencies == null)
				dependencies = new ArrayList ();

			if (!dependencies.Contains (filename))
				dependencies.Add (filename);
		}
		
		internal virtual void AddAssembly (Assembly assembly, bool fullPath)
		{
			if (anames == null)
				anames = new Hashtable ();

			string name = assembly.GetName ().Name;
			string loc = assembly.Location;
			if (fullPath) {
				if (!assemblies.Contains (loc)) {
					assemblies.Add (loc);
				}

				anames [name] = loc;
				anames [loc] = assembly;
			} else {
				if (!assemblies.Contains (name)) {
					assemblies.Add (name);
				}

				anames [name] = assembly;
			}
		}

		internal virtual Assembly AddAssemblyByName (string name)
		{
			if (anames == null)
				anames = new Hashtable ();

			if (anames.Contains (name)) {
				object o = anames [name];
				if (o is string)
					o = anames [o];

				return (Assembly) o;
			}

			bool fullpath = true;
			Assembly assembly = LoadAssemblyFromBin (name);
			if (assembly != null) {
				AddAssembly (assembly, fullpath);
				return assembly;
			}

			try {
				assembly = Assembly.Load (name);
				string loc = assembly.Location;
				fullpath = (Path.GetDirectoryName (loc) == PrivateBinPath);
			} catch (Exception e) {
				ThrowParseException ("Assembly " + name + " not found", e);
			}

			AddAssembly (assembly, fullpath);
			return assembly;
		}

		internal virtual void ProcessMainAttributes (Hashtable atts)
		{
			atts.Remove ("Description"); // ignored
			atts.Remove ("CodeBehind");  // ignored

			debug = GetBool (atts, "Debug", true);
			compilerOptions = GetString (atts, "CompilerOptions", null);
			language = GetString (atts, "Language", "C#");
			if (String.Compare (language, "c#", true) != 0)
				ThrowParseException ("Only C# supported.");

			string src = GetString (atts, "Src", null);
			Assembly srcAssembly = null;
			if (src != null)
				srcAssembly = GetAssemblyFromSource (src);

			string inherits = GetString (atts, "Inherits", null);
			if (inherits != null) {
				Type parent;
				if (srcAssembly != null)
					parent = srcAssembly.GetType (inherits);
				else
					parent = LoadType (inherits);

				if (parent == null)
					ThrowParseException ("Cannot find type " + inherits);

				if (!DefaultBaseType.IsAssignableFrom (parent))
					ThrowParseException ("The parent type does not derive from " + DefaultBaseType);

				baseType = parent;
			}

			className = GetString (atts, "ClassName", null);
			if (atts.Count > 0)
				ThrowParseException ("Unknown attribute: " + GetOneKey (atts));
		}

		Assembly GetAssemblyFromSource (string vpath)
		{
			vpath = UrlUtils.Combine (BaseVirtualDir, vpath);
			string realPath = MapPath (vpath, false);
			if (!File.Exists (realPath))
				ThrowParseException ("File " + vpath + " not found");

			AddDependency (realPath);

			//TODO: support other languages
			Assembly assembly = CSCompiler.CompileCSFile (realPath, assemblies);
			AddAssembly (assembly, true);
			return assembly;
		}
		
		protected abstract Type DefaultBaseType { get; }

		protected internal abstract string DefaultDirectiveName { get; }

		internal string InputFile
		{
			get { return inputFile; }
			set { inputFile = value; }
		}

		internal string Text
		{
			get { return text; }
			set { text = value; }
		}

		internal Type BaseType
		{
			get {
				if (baseType == null)
					baseType = DefaultBaseType;

				return baseType;
			}
		}
		
		internal string ClassName {
			get {
				if (className != null)
					return className;

				className = Path.GetFileName (inputFile).Replace ('.', '_');
				className = className.Replace ('-', '_'); 
				className = className.Replace (' ', '_');
				return className;
			}
		}

		internal string PrivateBinPath {
			get {
				if (privateBinPath != null)
					return privateBinPath;

				AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
				privateBinPath = setup.PrivateBinPath;
					
				if (!Path.IsPathRooted (privateBinPath)) {
					string appbase = setup.ApplicationBase;
					if (appbase.StartsWith ("file://")) {
						appbase = appbase.Substring (7);
						if (Path.DirectorySeparatorChar != '/')
							appbase = appbase.Replace ('/', Path.DirectorySeparatorChar);
					}
					privateBinPath = Path.Combine (appbase, privateBinPath);
				}

				return privateBinPath;
			}
		}

		internal ArrayList Scripts {
			get {
				if (scripts == null)
					scripts = new ArrayList ();

				return scripts;
			}
		}

		internal ArrayList Imports {
			get { return imports; }
		}

		internal ArrayList Assemblies {
			get { return assemblies; }
		}

		internal ArrayList Interfaces {
			get { return interfaces; }
		}

		internal RootBuilder RootBuilder {
			get { return rootBuilder; }
			set { rootBuilder = value; }
		}

		internal ArrayList Dependencies {
			get { return dependencies; }
		}

		internal string CompilerOptions {
			get { return compilerOptions; }
		}

		internal string Language {
			get { return language; }
		}

		internal bool Debug {
			get { return debug; }
		}
	}
}

