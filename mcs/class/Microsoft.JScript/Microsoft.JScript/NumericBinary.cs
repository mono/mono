//
// NumericBinary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public sealed class NumericBinary : BinaryOp {

		public NumericBinary (int operatorTok)
		{			
			current_op = (JSToken) operatorTok;
		}

		public object EvaluateNumericBinary (object v1, object v2)
		{
			return new object ();
		}


		public static object DoOp (object v1, object v2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
