//
// System.CodeDom.Compiler ICodeParser Interface
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom.Compiler
{
	using System.CodeDom;
	using System.IO;

	public interface ICodeParser
	{
		CodeCompileUnit Parse( TextReader codeStream );
	}
}
