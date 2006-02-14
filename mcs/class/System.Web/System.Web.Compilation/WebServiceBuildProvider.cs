//
// System.Web.Compilation.WebServiceBuildProvider
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

	sealed class WebServiceBuildProvider : BuildProvider {
		WebServiceCompiler wscompiler;
		CompilerType compiler_type;

		public WebServiceBuildProvider ()
		{
		}

		public override void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			HttpContext context = HttpContext.Current;
			WebServiceParser parser = new WebServiceParser (context, VirtualPath, OpenReader ());
			wscompiler = new WebServiceCompiler (parser);
			compiler_type = GetDefaultCompilerTypeForLanguage (parser.Language);
			if (parser.Program.Trim () == "")
				return;

			using (TextWriter writer = assemblyBuilder.CreateCodeFile (this)) {
				writer.WriteLine (parser.Program);
			}
		}

		public override Type GetGeneratedType (CompilerResults results)
		{
			SimpleWebHandlerParser parser = wscompiler.Parser;
			Type type = null;
			if (parser.Program.Trim () == "") {
				type = parser.GetTypeFromBin (parser.ClassName);
				return type;
			}

			Assembly assembly = results.CompiledAssembly;
			return assembly.GetType (parser.ClassName);
		}

		public override CompilerType CodeCompilerType {
			get { return compiler_type; }
		}

		// FIXME: figure this out.
		public override ICollection VirtualPathDependencies {
			get {
				return wscompiler.Parser.Dependencies;
			}
		}
	}
}
#endif

