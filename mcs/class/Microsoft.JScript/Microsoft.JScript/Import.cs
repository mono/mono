//
// Import.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	public class Import : AST
	{
		string name;

		public static void JScriptImport (string name, VsaEngine engine)
		{}


		public override object Visit (Visitor v, object args)
		{
			return v.VisitImport (this, args);
		}
	}
}