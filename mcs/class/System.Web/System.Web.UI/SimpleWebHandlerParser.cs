//
// System.Web.UI.SimpleWebHandlerParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.UI
{
	public abstract class SimpleWebHandlerParser
	{
		HttpContext context;
		string vPath;
		string physPath;
		string className;
		string codeBehind;
		bool debug;
		string language;
		string program;
		bool gotDefault;
		ArrayList assemblies;
		ArrayList dependencies;
		Hashtable anames;
		string privateBinPath;
		string baseDir;
		string baseVDir;
		CompilationConfiguration compilationConfig;
		int appAssemblyIndex = -1;
		Type cachedType;

		protected SimpleWebHandlerParser (HttpContext context, string virtualPath, string physicalPath)
		{
			cachedType = CachingCompiler.GetTypeFromCache (physicalPath);
			if (cachedType != null)
				return; // We don't need anything else.

			this.context = context;
			this.vPath = virtualPath;
			this.physPath = physicalPath;
			AddDependency (physicalPath);

			assemblies = new ArrayList ();
			string location = Context.ApplicationInstance.AssemblyLocation;
			if (location != typeof (TemplateParser).Assembly.Location)
				appAssemblyIndex = assemblies.Add (location);

			assemblies.AddRange (CompilationConfig.Assemblies);
			if (CompilationConfig.AssembliesInBin)
				AddAssembliesInBin ();

			language = CompilationConfig.DefaultLanguage;

			GetDirectivesAndContent ();
		}

		protected Type GetCompiledTypeFromCache ()
		{
			return cachedType;
		}

		void GetDirectivesAndContent ()
		{
			StreamReader reader = new StreamReader (File.OpenRead (physPath));
			string line;
			bool directiveFound = false;
			StringBuilder content = new StringBuilder ();

			while ((line = reader.ReadLine ()) != null && cachedType == null) {
				string trimmed = line.Trim ();
				if (!directiveFound && trimmed == String.Empty)
					continue;
				
				if (trimmed.StartsWith ("<")) {
					ParseDirective (trimmed);
					directiveFound = true;
					if (gotDefault) {
						cachedType = CachingCompiler.GetTypeFromCache (physPath);
						if (cachedType != null)
							break;
					}

					continue;
				}

				content.Append (line + "\n");
				content.Append (reader.ReadToEnd ());
			}
			reader.Close ();

			if (!gotDefault)
				throw new ParseException (null, "No @" + DefaultDirectiveName +
							" directive found");

			if (cachedType == null)
				this.program = content.ToString ();
		}

		void TagParsed (ILocation location, System.Web.Compilation.TagType tagtype, string tagid, TagAttributes attributes)
		{
			if (tagtype != System.Web.Compilation.TagType.Directive)
				throw new ParseException (location, "Unexpected tag");

			if (String.Compare (tagid, DefaultDirectiveName, true) == 0) {
				AddDefaultDirective (location, attributes);
			} else if (String.Compare (tagid, "Assembly", true) == 0) {
				AddAssemblyDirective (location, attributes);
			} else {
				throw new ParseException (location, "Unexpected directive: " + tagid);
			}
		}

		void TextParsed (ILocation location, string text)
		{
			if (text.Trim () != "")
				throw new ParseException (location, "Text not allowed here");
		}

		void ParseError (ILocation location, string message)
		{
			throw new ParseException (location, message);
		}

		static string GetAndRemove (Hashtable table, string key)
		{
			string o = table [key] as string;
			table.Remove (key);
			return o;
		}
		
		void ParseDirective (string line)
		{
			AspParser parser = new AspParser (physPath, new StringReader (line));
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);

			parser.Parse ();
		}

		internal virtual void AddDefaultDirective (ILocation location, TagAttributes attrs)
		{
			if (gotDefault)
				throw new ParseException (location, "duplicate " + DefaultDirectiveName + " directive");

			gotDefault = true;
			Hashtable attributes = attrs.GetDictionary (null);
			className = GetAndRemove (attributes, "class");
			if (className == null)
				throw new ParseException (null, "No Class attribute found.");
			
			string d = GetAndRemove (attributes, "debug");
			if (d != null) {
				debug = (String.Compare (d, "true", true) == 0);
				if (debug == false && String.Compare (d, "false", true) != 0)
					throw new ParseException (null, "Invalid value for Debug attribute");
			}

			language = GetAndRemove (attributes, "language");
			if (language == null)
				language = CompilationConfig.DefaultLanguage;

			codeBehind = GetAndRemove (attributes, "codebehind");
			if (attributes.Count > 0)
				throw new ParseException (location, "Unrecognized attribute in " +
							  DefaultDirectiveName + " directive");
		}

		internal virtual void AddAssemblyDirective (ILocation location, TagAttributes attrs)
		{
			Hashtable tbl = attrs.GetDictionary (null);
			string name = GetAndRemove (tbl, "Name");
			string src = GetAndRemove (tbl, "Src");
			if (name == null && src == null)
				throw new ParseException (location, "You gotta specify Src or Name");

			if (name != null && src != null)
				throw new ParseException (location, "Src and Name cannot be used together");

			if (name != null) {
				AddAssemblyByName (name, location);
			} else {
				GetAssemblyFromSource (src, location);
			}

			if (tbl.Count > 0)
				throw new ParseException (location, "Unrecognized attribute in Assembly directive");
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

		internal virtual Assembly AddAssemblyByName (string name, ILocation location)
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
				assembly = Assembly.LoadWithPartialName (name);
				string loc = assembly.Location;
				fullpath = (Path.GetDirectoryName (loc) == PrivateBinPath);
			} catch (Exception e) {
				throw new ParseException (location, "Assembly " + name + " not found", e);
			}

			AddAssembly (assembly, fullpath);
			return assembly;
		}

		void AddAssembliesInBin ()
		{
			if (!Directory.Exists (PrivateBinPath))
				return;

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string dll in binDlls) {
				try {
					Assembly assembly = Assembly.LoadFrom (dll);
					AddAssembly (assembly, true);
				} catch (Exception e) {
					throw new Exception ("Error while loading " + dll, e);
				}
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

		Assembly GetAssemblyFromSource (string vpath, ILocation location)
		{
			vpath = UrlUtils.Combine (BaseVirtualDir, vpath);
			string realPath = context.Request.MapPath (vpath);
			if (!File.Exists (realPath))
				throw new ParseException (location, "File " + vpath + " not found");

			AddDependency (realPath);

			CompilerResults result = CachingCompiler.Compile (language, realPath, realPath, assemblies);
			if (result.NativeCompilerReturnValue != 0) {
				StreamReader reader = new StreamReader (realPath);
				throw new CompilationException (realPath, result.Errors, reader.ReadToEnd ());
			}

			AddAssembly (result.CompiledAssembly, true);
			return result.CompiledAssembly;
		}
		
		internal Type GetTypeFromBin (string typeName)
		{
			if (!Directory.Exists (PrivateBinPath))
				throw new HttpException (String.Format ("Type {0} not found.", typeName));

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			Type result = null;
			foreach (string dll in binDlls) {
				Assembly assembly = Assembly.LoadFrom (dll);
				Type type = assembly.GetType (typeName, false);
				if (type != null) {
					if (result != null) 
						throw new HttpException (String.Format ("Type {0} is not unique.", typeName));

					result = type;
				} 
			}

			if (result == null)
				throw new HttpException (String.Format ("Type {0} not found.", typeName));

			return result;
		}
		
		internal virtual void AddDependency (string filename)
		{
			if (dependencies == null)
				dependencies = new ArrayList ();

			if (!dependencies.Contains (filename))
				dependencies.Add (filename);
		}
		
		// Properties
		protected abstract string DefaultDirectiveName { get; }

		internal HttpContext Context {
			get { return context; }
		}

		internal string VirtualPath {
			get { return vPath; }
		}

		internal string PhysicalPath {
			get { return physPath; }
		}

		internal string ClassName {
			get { return className; }
		}

		internal string CodeBehind {
			get { return codeBehind; }
		}

		internal bool Debug {
			get { return debug; }
		}

		internal string Language {
			get { return language; }
		}

		internal string Program {
			get { return program; }
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

		internal ArrayList Dependencies {
			get { return dependencies; }
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

		internal string BaseDir {
			get {
				if (baseDir == null)
					baseDir = context.Request.MapPath (BaseVirtualDir);

				return baseDir;
			}
		}

		internal virtual string BaseVirtualDir {
			get {
				if (baseVDir == null)
					baseVDir = UrlUtils.GetDirectory (context.Request.FilePath);

				return baseVDir;
			}
		}

		internal CompilationConfiguration CompilationConfig {
			get {
				if (compilationConfig == null)
					compilationConfig = CompilationConfiguration.GetInstance (context);

				return compilationConfig;
			}
		}
	}
}

