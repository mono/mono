//
// BitwiseBinary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript.Tmp {

	public sealed class BitwiseBinary : BinaryOp {

		public BitwiseBinary (Context context, AST operand1, AST operand2,
				      JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}


		public object EvaluateBitwiseBinary (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}
}
