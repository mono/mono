//
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Text;
using System;

namespace Microsoft.JScript {

	internal class If : AST {

		internal AST cond, true_stm, false_stm;

		internal If (AST condition, AST true_stm, AST false_stm)
		{
			this.cond = condition;
			this.true_stm = true_stm;
			this.false_stm = false_stm;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (cond != null)
				sb.Append (cond.ToString ());
			if (true_stm != null)
				sb.Append (true_stm.ToString ());
			if (false_stm != null)
				sb.Append (false_stm.ToString ());
			
			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			cond.Resolve (context);
			true_stm.Resolve (context);
			false_stm.Resolve (context);

			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class Continue : AST {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
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

	internal class Break : AST {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
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

	internal class Return : AST {

		internal AST expression;

		internal Return (AST exp)
		{
			expression = exp;
		}

		public override string ToString ()
		{
			return expression.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			expression.Resolve (context);
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
