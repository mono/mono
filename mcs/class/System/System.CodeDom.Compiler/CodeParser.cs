//
// System.CodeDom.Compiler.CodeParser.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System.IO;

namespace System.CodeDom.Compiler 
{

	public abstract class CodeParser : ICodeParser
	{

		protected CodeParser ()
		{
		}

		public abstract CodeCompileUnit Parse (TextReader codeStream);
	}
}
