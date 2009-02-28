//
// System.Web.Compilation.AppWebResourcesCompiler
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://novell.com/)
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

#if NET_2_0 && WEBSERVICES_DEP
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace System.Web.Compilation
{
	internal class AppWebReferencesCompiler
	{
		const string ResourcesDirName = "App_WebReferences";
		
		public void Compile ()
		{
			string refsPath = Path.Combine (HttpRuntime.AppDomainAppPath, ResourcesDirName);
			if (!Directory.Exists (refsPath))
				return;

			string[] files = Directory.GetFiles (refsPath, "*.wsdl", SearchOption.AllDirectories);
			if (files == null || files.Length == 0)
				return;

			CompilationSection cs = WebConfigurationManager.GetWebApplicationSection ("system.web/compilation") as CompilationSection;
			if (cs == null)
				throw new HttpException ("Unable to determine default compilation language.");

			CompilerType ct = BuildManager.GetDefaultCompilerTypeForLanguage (cs.DefaultLanguage, cs);
			CodeDomProvider codeDomProvider = null;
			Exception codeDomException = null;
			
			try {
				codeDomProvider = Activator.CreateInstance (ct.CodeDomProviderType) as CodeDomProvider;
			} catch (Exception e) {
				codeDomException = e;
			}

			if (codeDomProvider == null)
				throw new HttpException ("Unable to instantiate default compilation language provider.", codeDomException);

			AssemblyBuilder ab = new AssemblyBuilder (codeDomProvider, "App_WebReferences_");
			ab.CompilerOptions = ct.CompilerParameters;
			
			VirtualPath vp;
			WsdlBuildProvider wbp;
			foreach (string file in files) {
				vp = VirtualPath.PhysicalToVirtual (file);
				if (vp == null)
					continue;
				
				wbp = new WsdlBuildProvider ();
				wbp.SetVirtualPath (vp);
				wbp.GenerateCode (ab);
			}

			CompilerResults results;
			try {
				results = ab.BuildAssembly ();
			} catch (CompilationException ex) {
				throw new HttpException ("Failed to compile web references.", ex);
			}

			if (results == null)
				return;
			
			Assembly asm = results.CompiledAssembly;
			BuildManager.TopLevelAssemblies.Add (asm);
		}
	}
}
#endif

