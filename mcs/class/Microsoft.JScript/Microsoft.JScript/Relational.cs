//
// Relational.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Text;

namespace Microsoft.JScript {

	public class Relational : BinaryOp {

		internal Relational (AST left, AST right, JSToken op)
		{
			this.left = left;
			this.right = right;
			this.current_op = op;
		}
		
		public Relational (int operatorTok)
		{
			this.current_op = (JSToken) operatorTok;
		}

		public double EvaluateRelational (object v1, object v2)
		{
			throw new NotImplementedException ();
		}


		public static double JScriptCompare (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (left.ToString ());

			if (current_op != JSToken.None)
				sb.Append (current_op + " ");

			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (left != null)
				left.Resolve (context);

			if (right != null)
				right.Resolve (context);

			return true;			
		}
	}
}
