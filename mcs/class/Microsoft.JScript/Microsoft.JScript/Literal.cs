//
// Literal.cs: This class groups the differents types of Literals.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) Cesar Lopez Nataren 
//

using System;

namespace Microsoft.JScript {

	public class Literal : AST {

		public Literal ()
		{}
	}


	internal class This : Literal {
	}

	internal class BooleanLiteral : Literal {

		internal bool val;

		internal BooleanLiteral (bool val)
		{
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}
	}
}