//
// System.Web.UI.TemplateParser
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
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
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
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
		bool output_cache;
		int oc_duration;
		string oc_header, oc_custom, oc_param, oc_controls;
		bool oc_shared;
		OutputCacheLocation oc_location;
		Assembly srcAssembly;
		int appAssemblyIndex = -1;
                
		internal TemplateParser ()
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
			assemblies.AddRange (CompilationConfig.Assemblies);
			if (CompilationConfig.AssembliesInBin)
				AddAssembliesInBin ();

			language = CompilationConfig.DefaultLanguage;
		}

		internal void AddApplicationAssembly ()
		{
			string location = Context.ApplicationInstance.AssemblyLocation;
			if (location != typeof (TemplateParser).Assembly.Location) {
				appAssemblyIndex = assemblies.Add (location);
			}
		}

		protected abstract Type CompileIntoType ();

		internal virtual void HandleOptions (object obj)
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

			cmp = String.Compare ("OutputCache", directive, true);
			if (cmp == 0) {
				output_cache = true;
				
				if (atts ["Duration"] == null)
					ThrowParseException ("The directive is missing a 'duration' attribute.");
				if (atts ["VaryByParam"] == null)
					ThrowParseException ("This directive is missing a 'VaryByParam' " +
							"attribute, which should be set to \"none\", \"*\", " +
							"or a list of name/value pairs.");

				foreach (DictionaryEntry entry in atts) {
					string key = (string) entry.Key;
					switch (key.ToLower ()) {
					case "duration":
						oc_duration = Int32.Parse ((string) entry.Value);
						if (oc_duration < 1)
							ThrowParseException ("The 'duration' attribute must be set " +
									"to a positive integer value");
						break;
					case "varybyparam":
						oc_param = (string) entry.Value;
						if (String.Compare (oc_param, "none") == 0)
							oc_param = null;
						break;
					case "varybyheader":
						oc_header = (string) entry.Value;
						break;
					case "varybycustom":
						oc_custom = (string) entry.Value;
						break;
					case "location":
						if (!(this is PageParser))
							goto default;

						try {
							oc_location = (OutputCacheLocation) Enum.Parse (
								typeof (OutputCacheLocation), (string) entry.Value, true);
						} catch {
							ThrowParseException ("The 'location' attribute is case sensitive and " +
									"must be one of the following values: Any, Client, " +
									"Downstream, Server, None, ServerAndClient.");
						}
						break;
					case "varybycontrol":
						if (this is PageParser)
							goto default;

                                                oc_controls = (string) entry.Value;
						break;
					case "shared":
						if (this is PageParser)
							goto default;

						try {
							oc_shared = Boolean.Parse ((string) entry.Value);
						} catch {
							ThrowParseException ("The 'shared' attribute is case sensitive" +
									" and must be set to 'true' or 'false'.");
						}
						break;
					default:
						ThrowParseException ("The '" + key + "' attribute is not " +
								"supported by the 'Outputcache' directive.");
						break;
					}
					
				}
				
				return;
			}

			ThrowParseException ("Unknown directive: " + directive);
		}

		internal Type LoadType (string typeName)
		{
			// First try loaded assemblies, then try assemblies in Bin directory.
			Type type = null;
			bool seenBin = false;
			Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly ass in assemblies) {
				type = ass.GetType (typeName);
				if (type == null)
					continue;

				if (Path.GetDirectoryName (ass.Location) != PrivateBinPath) {
					AddAssembly (ass, false);
				} else {
					seenBin = true;
				}

				AddDependency (ass.Location);
				return type;
			}

			if (seenBin)
				return null;

			// Load from bin
			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string s in binDlls) {
				Assembly binA = Assembly.LoadFrom (s);
				type = binA.GetType (typeName);
				if (type == null)
					continue;

				AddDependency (binA.Location);
				return type;
			}

			return null;
		}

		void AddAssembliesInBin ()
		{
			if (!Directory.Exists (PrivateBinPath))
				return;

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string s in binDlls) {
				assemblies.Add (s);
			}
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

			bool fullpath = false;
			Assembly assembly = null;
			try {
				assembly = Assembly.LoadWithPartialName (name);
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
			atts.Remove ("AspCompat"); // ignored

			debug = GetBool (atts, "Debug", true);
			compilerOptions = GetString (atts, "CompilerOptions", "");
			language = GetString (atts, "Language", CompilationConfig.DefaultLanguage);
			string src = GetString (atts, "Src", null);
			if (src != null)
				srcAssembly = GetAssemblyFromSource (src);

			string inherits = GetString (atts, "Inherits", null);
			if (inherits != null)
				SetBaseType (inherits);

			className = GetString (atts, "ClassName", null);
			if (className != null && !CodeGenerator.IsValidLanguageIndependentIdentifier (className))
				ThrowParseException (String.Format ("'{0}' is not valid for 'className'", className));

			if (atts.Count > 0)
				ThrowParseException ("Unknown attribute: " + GetOneKey (atts));
		}

		internal void SetBaseType (string type)
		{
			if (type == DefaultBaseTypeName)
				return;

			Type parent = null;
			if (srcAssembly != null)
				parent = srcAssembly.GetType (type);

			if (parent == null)
				parent = LoadType (type);

			if (parent == null)
				ThrowParseException ("Cannot find type " + type);

			if (!DefaultBaseType.IsAssignableFrom (parent))
				ThrowParseException ("The parent type does not derive from " + DefaultBaseType);

			baseType = parent;
		}

		Assembly GetAssemblyFromSource (string vpath)
		{
			vpath = UrlUtils.Combine (BaseVirtualDir, vpath);
			string realPath = MapPath (vpath, false);
			if (!File.Exists (realPath))
				ThrowParseException ("File " + vpath + " not found");

			AddDependency (realPath);

			CompilerResults result = CachingCompiler.Compile (language, realPath, realPath, assemblies);
			if (result.NativeCompilerReturnValue != 0) {
				StreamReader reader = new StreamReader (realPath);
				throw new CompilationException (realPath, result.Errors, reader.ReadToEnd ());
			}

			AddAssembly (result.CompiledAssembly, true);
			return result.CompiledAssembly;
		}
		
		internal abstract Type DefaultBaseType { get; }
		internal abstract string DefaultBaseTypeName { get; }
		internal abstract string DefaultDirectiveName { get; }

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

				if (Char.IsDigit(className[0])) {
					className = "_" + className;
				}

				return className;
			}
		}

		internal string PrivateBinPath {
			get {
				if (privateBinPath != null)
					return privateBinPath;

				AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
				privateBinPath = Path.Combine (setup.ApplicationBase, setup.PrivateBinPath);

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
			get {
				if (appAssemblyIndex != -1) {
					object o = assemblies [appAssemblyIndex];
					assemblies.RemoveAt (appAssemblyIndex);
					assemblies.Add (o);
					appAssemblyIndex = -1;
				}

				return assemblies;
			}
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

		internal bool OutputCache {
			get { return output_cache; }
		}

		internal int OutputCacheDuration {
			get { return oc_duration; }
		}

		internal string OutputCacheVaryByHeader {
			get { return oc_header; }
		}

		internal string OutputCacheVaryByCustom {
			get { return oc_custom; }
		}

		internal string OutputCacheVaryByControls {
			get { return oc_controls; }
		}
		
		internal bool OutputCacheShared {
			get { return oc_shared; }
		}
		
		internal OutputCacheLocation OutputCacheLocation {
			get { return oc_location; }
		}

		internal string OutputCacheVaryByParam {
			get { return oc_param; }
		}

		internal PagesConfiguration PagesConfig {
			get { return PagesConfiguration.GetInstance (Context); }
		}
	}
}

