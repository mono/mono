//
// NumericUnary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript.Tmp {

	public sealed class NumericUnary : UnaryOp {

		public NumericUnary (int operatorTok)
		{
			throw new NotImplementedException ();
		}


		public object EvaluateUnary (object v)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}		
	}
}
