//
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System.Text;

namespace Microsoft.JScript {

	public class Statement : AST {

		public Statement ()
		{}
	}

	internal class If : Statement {

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
	}

	internal class Continue : Statement {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
		}
	}

	internal class Break : Statement {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
		}
	}
}