//
// StringLiteral.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	internal class StringLiteral : Literal {

		internal string str;

		public string Str {
			get { return str; }
			set { str = value; }
		}

		internal StringLiteral (string s)
		{
			str = s;
		}

		public override string ToString ()
		{
			return str;
		}
	}
}
	