//
// PostOrPrefixOperator.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public class PostOrPrefixOperator : UnaryOp {

		public PostOrPrefixOperator (int operatorTok)
		{
			throw new NotImplementedException ();
		}

		internal PostOrPrefixOperator (AST parent, AST operand, JSToken oper)
		{
			this.parent = parent;
			this.operand = operand;
			this.oper = oper;
		}

		public object EvaluatePostOrPrefix (ref object v)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (oper == JSToken.None)
				return operand.Resolve (context);
			else
				throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			if (oper == JSToken.None)
				if (operand is Exp)
					return ((Exp) operand).Resolve (context, no_effect);
				else
					return operand.Resolve (context);
			else
				throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			if (oper == JSToken.None)
				operand.Emit (ec);
			else
				throw new NotImplementedException ();
		}
	}
}
