//
// Relational.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript.Tmp {

	public class Relational : BinaryOp {

		public Relational (int operatorTok)
		{}

		public double EvaluateRelational (object v1, object v2)
		{
			throw new NotImplementedException ();
		}


		public static double JScriptCompare (object v1, object v2)
		{
			throw new NotImplementedException ();
		}
	}
}