//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;

namespace Microsoft.VisualBasic
{
	[ToolboxItem (""), DesignerCategory ("Component")]
	public class VBCodeProvider : CodeDomProvider
	{

		public VBCodeProvider()
		{
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

		public override ICodeCompiler CreateCompiler()
		{
			return new Microsoft.VisualBasic.VBCodeGenerator();
		}

		public override ICodeGenerator CreateGenerator()
		{
			return new Microsoft.VisualBasic.VBCodeGenerator();
		}
		
		public override TypeConverter GetConverter (Type type)
		{
			return TypeDescriptor.GetConverter (type);
		}
	}
}
