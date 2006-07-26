//
// Microsoft.JScript JScriptCodeProvider Class implementation
//
// Authors:
//      akiramei (mei@work.email.ne.jp)
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


namespace Microsoft.JScript 
{
	using System;
	using System.CodeDom.Compiler;
	// using System.Security.Permissions;
#if NET_2_0
	using System.CodeDom;
	using System.IO;
#endif

	// [PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	// [PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class JScriptCodeProvider : CodeDomProvider
	{
		JScriptCodeCompiler code_compiler;

		public JScriptCodeProvider ()
		{
			code_compiler = new JScriptCodeCompiler ();
		}

		public override string FileExtension {
			get {
				return "js";
			}
		}

#if NET_2_0
		[Obsolete ("Use CodeDomProvider class")]
#endif
		public override ICodeCompiler CreateCompiler ()
		{
			return code_compiler;
		}

#if NET_2_0
		[Obsolete ("Use CodeDomProvider class")]
#endif
		public override ICodeGenerator CreateGenerator ()
		{
			return code_compiler;
		}

#if NET_2_0
		public override void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			throw new NotImplementedException ();
		}
#endif
    }
}
