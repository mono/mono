//
// Literal.cs: This class groups the differents types of Literals.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) Cesar Lopez Nataren 
//

using System;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	internal class This : AST {

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
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
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			if (ec.is_global_code_method) {
				if (val)
					ec.ig.Emit (OpCodes.Ldc_I4_1);
				else
					ec.ig.Emit (OpCodes.Ldc_I4_0);

				ec.ig.Emit (OpCodes.Box, typeof (System.Boolean));
			}
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

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
