//
// StringLiteral.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	internal class StringLiteral : Literal
	{
		string str;
	
		public string Str {
			get { return str; }
			set { str = value; }
		}

		internal override object Visit (Visitor v, object args)
		{
			return v.VisitStringLiteral (this, args);
		}


		public override string ToString ()
		{
			return str;
		}
	}
}
	