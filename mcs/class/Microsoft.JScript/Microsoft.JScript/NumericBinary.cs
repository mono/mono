//
// NumericBinary.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript.Tmp {

	public sealed class NumericBinary : BinaryOp {

		public NumericBinary (int operatorTok)
		{
			throw new NotImplementedException ();
		}


		public object EvaluateNumericBinary (object v1, object v2)
		{
			throw new NotImplementedException ();
		}


		public static object DoOp (object v1, object v2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}
}
