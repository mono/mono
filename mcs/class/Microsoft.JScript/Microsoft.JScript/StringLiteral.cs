//
// StringLiteral.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript.Tmp {

	internal class StringLiteral : Literal {

		string str;
	
		public string Str {
			get { return str; }
			set { str = value; }
		}


		public override string ToString ()
		{
			return str;
		}
	}
}
	