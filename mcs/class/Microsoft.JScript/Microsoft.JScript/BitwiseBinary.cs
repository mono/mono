//
// BitwiseBinary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public sealed class BitwiseBinary : BinaryOp {

		public BitwiseBinary (int operatorTok)
		{
			current_op = (JSToken) operatorTok;
		} 

		public BitwiseBinary (Context context, AST operand1, AST operand2,
				      JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}


		public object EvaluateBitwiseBinary (object v1, object v2)
		{
			return new object ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
