//
// System.Web.Compilation.AssemblyBuilder
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Compilation {

	public class AssemblyBuilder {
		static bool KeepFiles = (Environment.GetEnvironmentVariable ("MONO_ASPNET_NODELETE") != null);

		CodeDomProvider provider;
		List <CodeCompileUnit> units;
		List <string> source_files;
		Dictionary <string, string> resource_files;
		TempFileCollection temp_files;
		string virtual_path;
		//TODO: there should be a Compile () method here which is where all the compilation exceptions are thrown from.
		
		internal AssemblyBuilder (string virtualPath, CodeDomProvider provider)
		{
			this.provider = provider;
			this.virtual_path = virtualPath;
			units = new List <CodeCompileUnit> ();
			units.Add (new CodeCompileUnit ());
			temp_files = new TempFileCollection ();
			CompilationSection section;
			section = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation", virtualPath);
			string tempdir = section.TempDirectory;
			if (tempdir == null || tempdir == "")
				tempdir = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
				
			temp_files = new TempFileCollection (tempdir, KeepFiles);
		}

		internal TempFileCollection TempFiles {
			get { return temp_files; }
		}

		internal CodeCompileUnit [] GetUnitsAsArray ()
		{
			CodeCompileUnit [] result = new CodeCompileUnit [units.Count];
			units.CopyTo (result, 0);
			return result;
		}

		List <string> SourceFiles {
			get {
				if (source_files == null)
					source_files = new List <string> ();
				return source_files;
			}
		}

		Dictionary <string, string> ResourceFiles {
			get {
				if (resource_files == null)
					resource_files = new Dictionary <string, string> ();
				return resource_files;
			}
		}

		public void AddAssemblyReference (Assembly a)
		{
			if (a == null)
				throw new ArgumentNullException ("a");

			StringCollection coll = units [units.Count - 1].ReferencedAssemblies;
			string location = a.Location;
			if (coll.IndexOf (location) == -1)
				coll.Add (location);
		}

		[MonoTODO ("Do something with the buildProvider argument")]
		public void AddCodeCompileUnit (BuildProvider buildProvider, CodeCompileUnit compileUnit)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			if (compileUnit == null)
				throw new ArgumentNullException ("compileUnit");

			units.Add (compileUnit);
		}

		[MonoTODO ("Anything to do with the buildProvider argument?")]
		public TextWriter CreateCodeFile (BuildProvider buildProvider)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			string filename = temp_files.AddExtension ("temp", true);
			SourceFiles.Add (filename);
			return new StreamWriter (File.OpenWrite (filename), WebEncoding.FileEncoding);
		}

		[MonoTODO ("Anything to do with the buildProvider argument?")]
		public Stream CreateEmbeddedResource (BuildProvider buildProvider, string name)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			if (name == null || name == "")
				throw new ArgumentNullException ("name");

			string filename = temp_files.AddExtension ("resource", true);
			Stream stream = File.OpenWrite (filename);
			ResourceFiles [name] = filename;
			return stream;
		}

		[MonoTODO]
		public void GenerateTypeFactory (string typeName)
		{
			// Do nothing by now.
		}

		public string GetTempFilePhysicalPath (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			return temp_files.AddExtension (extension, true);
		}

		public CodeDomProvider CodeDomProvider {
			get { return provider; }
		}

		internal CompilerResults BuildAssembly (string virtualPath, CompilerParameters options)
		{
			CompilerResults results;
			CodeCompileUnit [] units = GetUnitsAsArray ();
			results = provider.CompileAssemblyFromDom (options, units);
			// FIXME: generate the code and display it
			if (results.NativeCompilerReturnValue != 0)
				throw new CompilationException (virtualPath, results.Errors, "");

			Assembly assembly = results.CompiledAssembly;
			if (assembly == null) {
				if (!File.Exists (options.OutputAssembly)) {
					results.TempFiles.Delete ();
					throw new CompilationException (virtualPath, results.Errors,
						"No assembly returned after compilation!?");
				}

				results.CompiledAssembly = Assembly.LoadFrom (options.OutputAssembly);
			}

			results.TempFiles.Delete ();
			return results;
		}
	}
}
#endif

