//
// DocumentContext.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	public class DocumentContext
	{
		internal string Name;

		internal DocumentContext (string filename)
		{
			Name = filename;
		}
	}
}