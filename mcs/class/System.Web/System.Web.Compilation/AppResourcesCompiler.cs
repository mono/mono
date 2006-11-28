//
// System.Web.Compilation.AppResourcesCompiler: Support for compilation of .resx files into a satellite assembly
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
using System.IO;
using System.Web;
using System.Web.Configuration;

namespace System.Web.Compilation
{
	internal abstract class AppResourcesCompiler: AppResourceFilesCompiler
	{
		protected string resourceDirName = null;
    
		public AppResourcesCompiler ()
			: base ()
		{}
    
		public AppResourcesCompiler (string [] filePaths)
			: base (filePaths)
		{}

		public AppResourcesCompiler (string [] filePaths, FcCodeGenerator fg)
			: base (filePaths, fg)
		{}
    
		public string DynamicDirectory {
			get { return AppDomain.CurrentDomain.DynamicDirectory; }
		}

		public string ResourceDirectory {
			get {
				if (resourceDirName == null)
					return null;
				string dir = Path.Combine (TopPath, resourceDirName);
				if (!Directory.Exists (dir))
					return null;
				return dir;
			}
		}
    
		public bool CompilationPossible {
			get { return (ResourceDirectory != null); }
		}

		public abstract string TopPath {
			get;
		}
    
		virtual public CompilerResults Compile ()
		{
			string dir = ResourceDirectory;
      
			if (dir == null)
				return null;

			DirectoryInfo di = new DirectoryInfo (dir);
			if (di == null)
				return null;

			FileInfo[] fileinfos = di.GetFiles ("*.resx");
			if (fileinfos != null && fileinfos.Length > 0) {
				List<string> al = new List<string> (fileinfos.Length);
				foreach (FileInfo fi in fileinfos)
					al.Add (fi.FullName);
				filePaths = al.ToArray ();
			} else
				return null;
			
			CodeCompileUnit unit = FilesToDom ();
			if (unit == null || resourceFiles == null)
				return null;

			CompilerParameters cp = new CompilerParameters ();
			foreach (string rf in resourceFiles)
				cp.EmbeddedResources.Add (rf);
			cp.IncludeDebugInformation = false;
			cp.GenerateExecutable = false;
			cp.TreatWarningsAsErrors = false;
			cp.OutputAssembly = GenRandomFileName (TempDir, "dll");
			
			CodeDomProvider provider = GetCodeProvider ();
			StringWriter sw = new StringWriter ();
			provider.GenerateCodeFromCompileUnit (unit, sw, new CodeGeneratorOptions ());
			CompilerResults ret = provider.CompileAssemblyFromDom (cp, unit);      
			
			if (ret.Errors.Count != 0) {
				Console.WriteLine ("Failed to compile {0}/*.resx. Errors:", ResourceDirectory);
				foreach (CompilerError ce in ret.Errors)
					Console.WriteLine("{5} {0} ({1} {2}:{3}): {4}", ce.ErrorNumber,
							  ce.FileName, ce.Line, ce.Column, ce.ErrorText,
							  ce.IsWarning ? "warning" : "error");
			} else {
				WebConfigurationManager.ExtraAssemblies.Add(ret.PathToAssembly);
				BuildManager.TopLevelAssemblies.Add (ret.CompiledAssembly);
				BuildManager.HaveResources = true;
			}
			
			return ret;
		}
	}

	internal sealed class AppGlobalResourcesCompiler: AppResourcesCompiler
	{
		public AppGlobalResourcesCompiler ()
			: base ()
		{
			this.resourceDirName = "App_GlobalResources";
		}
    
		public override string TopPath {
			get { return HttpRuntime.AppDomainAppPath; }
		}
	}

	//FIXME: it seems that local resources are NOT strongly typed
	// see http://msdn2.microsoft.com/en-us/library/ms227427.aspx
	//
	// Local resources should probably be only put in a satellite assembly and referenced
	// from there.
	internal sealed class AppLocalResourcesCompiler: AppResourcesCompiler
	{
		public AppLocalResourcesCompiler ()
			: base ()
		{
			this.resourceDirName = "App_LocalResources";
		}

		public override string TopPath {
			get {
				return Path.GetDirectoryName (
					HttpContext.Current.Request.MapPath (
						HttpContext.Current.Request.CurrentExecutionFilePath));
			}
		}
	}
}
#endif
