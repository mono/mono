//
// Equality.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class Equality : BinaryOp
	{
		public Equality (Context context, AST oper1, AST oper2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}


		public bool EvaluateEquality (object v1, object v2)
		{
			throw new NotImplementedException ();
		}


		public static bool JScriptEquals (object v1, object v2)
		{
			throw new NotImplementedException ();
		}


		internal override object Visit (Visitor v, object obj)
		{
			throw new NotImplementedException ();
		}
	}
}