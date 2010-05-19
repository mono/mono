//
// System.Web.UI.SimpleWebHandlerParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.UI
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class SimpleWebHandlerParser
	{
		HttpContext context;
		string vPath;
		string physPath;
		string className;
		bool debug;
		string language;
		string program;
		bool gotDefault;
		ArrayList assemblies;
		ArrayList dependencies;
		Hashtable anames;
		string baseDir;
		string baseVDir;
		TextReader reader;
		int appAssemblyIndex = -1;
		Type cachedType;

		protected SimpleWebHandlerParser (HttpContext context, string virtualPath, string physicalPath)
		: this (context, virtualPath, physicalPath, null)
		{}
		
		internal SimpleWebHandlerParser (HttpContext context, string virtualPath, string physicalPath, TextReader reader)
		{
			this.reader = reader;
			cachedType = CachingCompiler.GetTypeFromCache (physicalPath);
			if (cachedType != null)
				return; // We don't need anything else.

			// context is obsolete in 2.0+ - MSDN recommends passing null, so we need to
			// take that into account
			if (context != null)
				this.context = context;
			else
				this.context = HttpContext.Current;
			
			this.vPath = virtualPath;
			AddDependency (virtualPath);
			
			// physicalPath is obsolete in 2.0+ - same note what for context applies here
			if (physicalPath != null && physicalPath.Length > 0)
				this.physPath = physicalPath;
			else {
				HttpRequest req = this.context != null ? context.Request : null;
				if (req != null)
					this.physPath = req.MapPath (virtualPath);
			}

			assemblies = new ArrayList ();
			string location = Context.ApplicationInstance.AssemblyLocation;
			if (location != typeof (TemplateParser).Assembly.Location)
				appAssemblyIndex = assemblies.Add (location);

			bool addAssembliesInBin = false;
			foreach (AssemblyInfo info in CompilationConfig.Assemblies) {
				if (info.Assembly == "*")
					addAssembliesInBin = true;
				else
					AddAssemblyByName (info.Assembly, null);
			}
			if (addAssembliesInBin)
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
			string line;
			bool directiveFound = false;
			bool inDirective = false;
			StringBuilder directive = null;
			StringBuilder content = new StringBuilder ();
			int idxStart, idxEnd, length;
			StreamReader sr;

			if (reader != null)
				sr = reader as StreamReader;
			else
				sr = new StreamReader (File.OpenRead (physPath), WebEncoding.FileEncoding);
			
			using (sr) {
				while ((line = sr.ReadLine ()) != null && cachedType == null) {
					length = line.Length;
					if (length == 0) {
						content.Append ("\n");
						continue;
					}
					
					idxStart = line.IndexOf ("<%");
					if (idxStart > -1) {
						idxEnd = line.IndexOf ("%>");						
						if (idxStart > 0)
							content.Append (line.Substring (0, idxStart));

						if (directive == null)
							directive = new StringBuilder ();
						else
							directive.Length = 0;
						
						if (idxEnd > -1) {
							directiveFound = true;
							inDirective = false;
							directive.Append (line.Substring (idxStart, idxEnd - idxStart + 2));
							if (idxEnd < length - 2)
								content.Append (line.Substring (idxEnd + 2, length - idxEnd - 2));
						} else {
							inDirective = true;
							directiveFound = false;
							directive.Append (line.Substring (idxStart));
							continue;
						}
					}

					if (inDirective) {
						int idx = line.IndexOf ("%>");
						if (idx > -1) {
							directive.Append (line.Substring (0, idx + 2));
							if (idx < length)
								content.Append (line.Substring (idx + 2) + "\n");
							inDirective = false;
							directiveFound = true;
						} else {
							directive.Append (line);
							continue;
						}
					}
					
					if (directiveFound) {
						ParseDirective (directive.ToString ());
						directiveFound = false;
						if (gotDefault) {
							cachedType = CachingCompiler.GetTypeFromCache (physPath);
							if (cachedType != null)
								break;
						}

						continue;
					}

					content.Append (line + "\n");
				}
				directive = null;
			}

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

			if (tagid == null || tagid.Length == 0 || String.Compare (tagid, DefaultDirectiveName, true, Helpers.InvariantCulture) == 0) {
				AddDefaultDirective (location, attributes);
			} else if (String.Compare (tagid, "Assembly", true, Helpers.InvariantCulture) == 0) {
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

		static string GetAndRemove (IDictionary table, string key)
		{
			string o = table [key] as string;
			table.Remove (key);
			return o;
		}
		
		void ParseDirective (string line)
		{
			AspParser parser;

			using (StringReader input = new StringReader (line)) {
				parser = new AspParser (physPath, input);
			}
			
			parser.Error += new ParseErrorHandler (ParseError);
			parser.TagParsed += new TagParsedHandler (TagParsed);
			parser.TextParsed += new TextParsedHandler (TextParsed);

			parser.Parse ();
		}

		internal virtual void AddDefaultDirective (ILocation location, TagAttributes attrs)
		{
			CompilationSection compConfig;
			compConfig = CompilationConfig;
			
			if (gotDefault)
				throw new ParseException (location, "duplicate " + DefaultDirectiveName + " directive");

			gotDefault = true;
			IDictionary attributes = attrs.GetDictionary (null);
			className = GetAndRemove (attributes, "class");
			if (className == null)
				throw new ParseException (null, "No Class attribute found.");
			
			string d = GetAndRemove (attributes, "debug");
			if (d != null) {
				debug = (String.Compare (d, "true", true, Helpers.InvariantCulture) == 0);
				if (debug == false && String.Compare (d, "false", true, Helpers.InvariantCulture) != 0)
					throw new ParseException (null, "Invalid value for Debug attribute");
			} else
				debug = compConfig.Debug;

			language = GetAndRemove (attributes, "language");
			if (language == null)
				language = compConfig.DefaultLanguage;

			GetAndRemove (attributes, "codebehind");
			if (attributes.Count > 0)
				throw new ParseException (location, "Unrecognized attribute in " +
							  DefaultDirectiveName + " directive");
		}

		internal virtual void AddAssemblyDirective (ILocation location, TagAttributes attrs)
		{
			IDictionary tbl = attrs.GetDictionary (null);
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
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			
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

			Assembly assembly = LoadAssemblyFromBin (name);
			if (assembly != null) {
				AddAssembly (assembly, true);
				return assembly;
			}

			Exception ex = null;
			try {
				assembly = Assembly.LoadWithPartialName (name);
			} catch (Exception e) {
				ex = e;
				assembly = null;
			}

			if (assembly == null)
				throw new ParseException (location, String.Format ("Assembly '{0}' not found", name), ex);
			
			AddAssembly (assembly, true);
			return assembly;
		}

		void AddAssembliesInBin ()
		{
			Exception ex;
			foreach (string s in HttpApplication.BinDirectoryAssemblies) {
				ex = null;
				
				try {
					Assembly assembly = Assembly.LoadFrom (s);
					AddAssembly (assembly, true);
				} catch (FileLoadException e) {
					ex = e;
					// ignore
				} catch (BadImageFormatException e) {
					ex = e;
					// ignore
				} catch (Exception e) {
					throw new Exception ("Error while loading " + s, e);
				}
				
				if (ex != null && RuntimeHelpers.DebuggingEnabled) {
					Console.WriteLine ("**** DEBUG MODE *****");
					Console.WriteLine ("Bad assembly found in bin/. Exception (ignored):");
					Console.WriteLine (ex);
				}
			}
		}

		Assembly LoadAssemblyFromBin (string name)
		{
			Assembly assembly = null;
			foreach (string dll in HttpApplication.BinDirectoryAssemblies) {
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

			AddDependency (vpath);

			CompilerResults result = CachingCompiler.Compile (language, realPath, realPath, assemblies);
			if (result.NativeCompilerReturnValue != 0) {
				using (StreamReader sr = new StreamReader (realPath)) {
					throw new CompilationException (realPath, result.Errors, sr.ReadToEnd ());
				}
			}

			AddAssembly (result.CompiledAssembly, true);
			return result.CompiledAssembly;
		}
		
		internal Type GetTypeFromBin (string tname)
		{
			if (tname == null || tname.Length == 0)
				throw new ArgumentNullException ("tname");
			
			Type result = null;
			string typeName;
			string assemblyName;
			int comma = tname.IndexOf (',');
			
			if (comma != -1) {
				typeName = tname.Substring (0, comma).Trim ();
				assemblyName = tname.Substring (comma + 1).Trim ();
			} else {
				typeName = tname;
				assemblyName = null;
			}

			Type type = null;
			Assembly assembly = null;
			if (assemblyName != null) {
				assembly = Assembly.Load (assemblyName);
				if (assembly != null)
					type = assembly.GetType (typeName, false);
				if (type != null)
					return type;
			}
			
			IList toplevelAssemblies = BuildManager.TopLevelAssemblies;
			if (toplevelAssemblies != null && toplevelAssemblies.Count > 0) {
				foreach (Assembly asm in toplevelAssemblies) {
					type = asm.GetType (typeName, false);
					if (type != null) {
						if (result != null)
							throw new HttpException (String.Format ("Type {0} is not unique.", typeName));
						result = type;
					}
				}
			}

			foreach (string dll in HttpApplication.BinDirectoryAssemblies) {
				try {
					assembly = Assembly.LoadFrom (dll);
				} catch (FileLoadException) {
					// ignore
					continue;
				} catch (BadImageFormatException) {
					// ignore
					continue;
				}
				
				type = assembly.GetType (typeName, false);
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

		internal bool Debug {
			get { return debug; }
		}

		internal string Language {
			get { return language; }
		}

		internal string Program {
			get {
				if (program != null)
					return program;

				return String.Empty;
			}
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

		CompilationSection CompilationConfig {
			get {
				string vp = VirtualPath;
				if (String.IsNullOrEmpty (vp))
					return WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
				else
					return WebConfigurationManager.GetSection ("system.web/compilation", vp) as CompilationSection;
			}
		}

		internal TextReader Reader {
			get { return reader; }
			set { reader = value; }
		}
	}
}

