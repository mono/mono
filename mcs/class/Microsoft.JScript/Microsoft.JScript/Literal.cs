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

		internal BooleanLiteral (AST parent, bool val)
		{
			this.parent = parent;
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
			ILGenerator ig;
			
			if (parent == null)
				ig = ec.gc_ig;
			else
				ig = ec.ig;

			if (val)
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);

			ig.Emit (OpCodes.Box, typeof (System.Boolean));
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
			ILGenerator ig;

			if (parent == null)
				ig = ec.gc_ig;
			else
				ig = ec.ig;

			ig.Emit (OpCodes.Ldc_I4, (int) val);
			ig.Emit (OpCodes.Box, typeof (System.Int32));
		}
	}
}
