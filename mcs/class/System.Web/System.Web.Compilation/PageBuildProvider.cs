//
// System.Web.Compilation.PageBuildProvider
//
// Authors:
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web.UI;

namespace System.Web.Compilation {

	sealed class PageBuildProvider : BuildProvider {
		PageCompiler pcompiler;
		CompilerType compiler_type;

		public PageBuildProvider()
		{
		}

		public override void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			HttpContext context = HttpContext.Current;
			PageParser parser = new PageParser (VirtualPath, OpenReader (), context);
			pcompiler = new PageCompiler (parser);
			pcompiler.CreateMethods ();
			compiler_type = GetDefaultCompilerTypeForLanguage (parser.Language);
			using (TextWriter writer = assemblyBuilder.CreateCodeFile (this)) {
				CodeDomProvider provider = pcompiler.Provider;
				CodeCompileUnit unit = pcompiler.CompileUnit;
				provider.CreateGenerator().GenerateCodeFromCompileUnit (unit, writer, null);
			}
		}

		public override Type GetGeneratedType (CompilerResults results)
		{
			// This is not called if compilation failed.
			// Returning null makes the caller throw an InvalidCastException
			Assembly assembly = results.CompiledAssembly;
			return assembly.GetType (pcompiler.Parser.ClassName);
		}

		public override CompilerType CodeCompilerType {
			get { return compiler_type; }
		}

		// FIXME: figure this out.
		public override ICollection VirtualPathDependencies {
			get {
				return pcompiler.Parser.Dependencies;
			}
		}
	}
}
#endif

