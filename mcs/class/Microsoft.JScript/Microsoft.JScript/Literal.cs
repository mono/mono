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

	internal class This : AST {

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}

	internal class BooleanLiteral : AST {

		internal bool val;

		internal BooleanLiteral (bool val)
		{
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}

	public class NumericLiteral : AST {

		double val;

		internal NumericLiteral (double val)
		{
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}
	}
}
