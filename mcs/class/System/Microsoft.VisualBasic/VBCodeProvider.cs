//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
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

namespace Microsoft.VisualBasic {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class VBCodeProvider : CodeDomProvider {

		public VBCodeProvider()
		{
		}

		public VBCodeProvider(System.Collections.Generic.IDictionary<string, string> providerOptions)
		{
			// TODO: Do something meaningful here...
		}

		public override string FileExtension {
			get {
				return "vb";
			}
		}

		public override LanguageOptions LanguageOptions {
			get {
				return LanguageOptions.CaseInsensitive;
			}
		}

		[Obsolete ("Use CodeDomProvider class")]
		public override ICodeCompiler CreateCompiler()
		{
			return new Microsoft.VisualBasic.VBCodeCompiler ();
		}

		[Obsolete ("Use CodeDomProvider class")]
		public override ICodeGenerator CreateGenerator()
		{
			return new Microsoft.VisualBasic.VBCodeGenerator();
		}
		
		public override TypeConverter GetConverter (Type type)
		{
			return TypeDescriptor.GetConverter (type);
		}

		[MonoTODO]
		public override void GenerateCodeFromMember (CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
