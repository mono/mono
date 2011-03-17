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
using System.Collections.Generic;

namespace Microsoft.CSharp {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class CSharpCodeProvider : CodeDomProvider {
		IDictionary <string, string> providerOptions;
		
		//
		// Constructors
		//
		public CSharpCodeProvider()
		{
		}

		public CSharpCodeProvider (IDictionary <string, string> providerOptions)
		{
			this.providerOptions = providerOptions;
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
		[Obsolete ("Use CodeDomProvider class")]
		public override ICodeCompiler CreateCompiler()
		{
			if (providerOptions != null && providerOptions.Count > 0)
				return new Mono.CSharp.CSharpCodeCompiler (providerOptions);
			return new Mono.CSharp.CSharpCodeCompiler();
		}

		[Obsolete ("Use CodeDomProvider class")]
		public override ICodeGenerator CreateGenerator()
		{
			if (providerOptions != null && providerOptions.Count > 0)
				return new Mono.CSharp.CSharpCodeGenerator (providerOptions);
			return new Mono.CSharp.CSharpCodeGenerator();
		}
		
		[MonoTODO]
		public override TypeConverter GetConverter( Type Type )
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
