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
		List <string> referenced_assemblies;
		Dictionary <string, string> resource_files;
		TempFileCollection temp_files;
		//TODO: there should be a Compile () method here which is where all the compilation exceptions are thrown from.

		internal AssemblyBuilder (CodeDomProvider provider)
		: this (null, provider)
		{}
		
		internal AssemblyBuilder (string virtualPath, CodeDomProvider provider)
		{
			this.provider = provider;
			units = new List <CodeCompileUnit> ();
			temp_files = new TempFileCollection ();
			referenced_assemblies = new List <string> ();
			CompilationSection section;
			if (virtualPath != null)
				section = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation", virtualPath);
			else
				section = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
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

			referenced_assemblies.Add (a.Location);
		}

		internal void AddCodeCompileUnit (CodeCompileUnit compileUnit)
		{
			if (compileUnit == null)
				throw new ArgumentNullException ("compileUnit");
			units.Add (compileUnit);
		}
		
		public void AddCodeCompileUnit (BuildProvider buildProvider, CodeCompileUnit compileUnit)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			if (compileUnit == null)
				throw new ArgumentNullException ("compileUnit");

			units.Add (compileUnit);
		}

		public TextWriter CreateCodeFile (BuildProvider buildProvider)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			// Generate a file name with the correct source language extension
			string filename = GetTempFilePhysicalPath (provider.FileExtension);
			SourceFiles.Add (filename);
			return new StreamWriter (File.OpenWrite (filename), WebEncoding.FileEncoding);
		}

		internal void AddCodeFile (string path)
		{
			if (path == null || path.Length == 0)
				return;
			string extension = Path.GetExtension (path);
			if (extension == null || extension.Length == 0)
				return; // maybe better to throw an exception here?
			extension = extension.Substring (1);
			string filename = GetTempFilePhysicalPath (extension);
			File.Copy (path, filename, true);
			SourceFiles.Add (filename);
		}
		
		public Stream CreateEmbeddedResource (BuildProvider buildProvider, string name)
		{
			if (buildProvider == null)
				throw new ArgumentNullException ("buildProvider");

			if (name == null || name == "")
				throw new ArgumentNullException ("name");

			string filename = GetTempFilePhysicalPath ("resource");
			Stream stream = File.OpenWrite (filename);
			ResourceFiles [name] = filename;
			return stream;
		}

		[MonoTODO ("Not implemented, does nothing")]
		public void GenerateTypeFactory (string typeName)
		{
			// Do nothing by now.
		}

		public string GetTempFilePhysicalPath (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			
			return temp_files.AddExtension (String.Format ("_{0}.{1}", temp_files.Count, extension), true);
		}

		public CodeDomProvider CodeDomProvider {
			get { return provider; }
		}

		internal CompilerResults BuildAssembly (CompilerParameters options)
		{
			return BuildAssembly (null, options);
		}
		
		internal CompilerResults BuildAssembly (string virtualPath, CompilerParameters options)
		{
			if (options == null)
				throw new ArgumentNullException ("options");
			
			CompilerResults results;
			CodeCompileUnit [] units = GetUnitsAsArray ();

			// Since we may have some source files and some code
			// units, we generate code from all of them and then
			// compile the assembly from the set of temporary source
			// files. This also facilates possible debugging for the
			// end user, since they get the code beforehand.
			List <string> files = SourceFiles;
			string filename;
			StreamWriter sw = null;
			foreach (CodeCompileUnit unit in units) {
				filename = GetTempFilePhysicalPath (provider.FileExtension);
				try {
					sw = new StreamWriter (File.OpenWrite (filename), WebEncoding.FileEncoding);
					provider.GenerateCodeFromCompileUnit (unit, sw, null);
					files.Add (filename);
				} catch {
					throw;
				} finally {
					if (sw != null) {
						sw.Flush ();
						sw.Close ();
					}
				}
			}
			Dictionary <string, string> resources = ResourceFiles;
			foreach (KeyValuePair <string, string> de in resources)
				options.EmbeddedResources.Add (de.Value);
			foreach (string refasm in referenced_assemblies)
				options.ReferencedAssemblies.Add (refasm);
			
			results = provider.CompileAssemblyFromFile (options, files.ToArray ());			

			if (results.NativeCompilerReturnValue != 0) {
				string fileText = null;
				try {
					using (StreamReader sr = File.OpenText (results.Errors [0].FileName)) {
						fileText = sr.ReadToEnd ();
					}
				} catch (Exception) {}
				
				throw new CompilationException (virtualPath, results.Errors, fileText);
			}
			
			Assembly assembly = results.CompiledAssembly;
			if (assembly == null) {
				if (!File.Exists (options.OutputAssembly)) {
					results.TempFiles.Delete ();
					throw new CompilationException (virtualPath, results.Errors,
						"No assembly returned after compilation!?");
				}

				try {
					results.CompiledAssembly = Assembly.LoadFrom (options.OutputAssembly);
				} catch (Exception ex) {
					results.TempFiles.Delete ();
					throw new HttpException ("Unable to load compiled assembly", ex);
				}
			}

			if (!KeepFiles)
				results.TempFiles.Delete ();
			return results;
		}
	}
}
#endif

