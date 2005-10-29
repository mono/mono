//
// Microsoft.CSharp CSharpCodeProvider Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2002 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;

namespace Microsoft.CSharp {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class CSharpCodeProvider : CodeDomProvider {

		//
		// Constructors
		//
		public CSharpCodeProvider()
		{
		}

		//
		// Properties
		//
		public override string FileExtension {
			get {
				return "cs";
			}
		}

		//
		// Methods
		//
#if NET_2_0
		[Obsolete ("Use CodeDomProvider class")]
#endif
		public override ICodeCompiler CreateCompiler()
		{
			return new Mono.CSharp.CSharpCodeCompiler();
		}

#if NET_2_0
		[Obsolete ("Use CodeDomProvider class")]
#endif
		public override ICodeGenerator CreateGenerator()
		{
			return new Mono.CSharp.CSharpCodeGenerator();
		}
		
		[MonoTODO]
		public override TypeConverter GetConverter( Type Type )
		{
			throw new NotImplementedException();
		}

#if NET_2_0
		[MonoTODO]
		public override void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			throw new NotImplementedException();
		}
#endif
	}
}
