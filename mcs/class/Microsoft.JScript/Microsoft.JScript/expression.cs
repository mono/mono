//
// Expression.cs: Everything related to expressions
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Text;
using System.Collections;

namespace Microsoft.JScript {

	public class Unary : UnaryOp {

		internal Unary (AST operand, JSToken oper)
		{			
			this.operand = operand;
			this.oper = oper;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			if (oper != JSToken.None)
				sb.Append (oper + " ");

			sb.Append (operand.ToString ());
			
			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			operand.Resolve (context);

			return true;
		}			
	}

	public class Binary : BinaryOp {

		internal Binary (AST left, AST right, JSToken op)
		{
			Console.WriteLine ("DEBUG::expression.cs::Binary constructor called");
			this.left = left;
			this.right = right;
			this.current_op = op;	
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (left.ToString () + " ");

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

	public class Conditional : AST {

		AST cond_expr, trueExpr, falseExpr;

		internal Conditional (AST expr, AST  trueExpr, AST falseExpr)
		{
			this.cond_expr = expr;
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (cond_expr != null)
				sb.Append (cond_expr.ToString () + " ");
			if (trueExpr != null)
				sb.Append (trueExpr.ToString () + " ");
			if (falseExpr != null)
				sb.Append (falseExpr.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}
	}

	public class Call : AST {

		internal AST left;
		internal AST args;

		public Call (AST left, AST args)
		{
			this.left = left;
			this.args = args;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (left != null)
				sb.Append (left.ToString () + " ");
			if (args != null)
				sb.Append (args.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (left != null)
				left.Resolve (context);

			if (args != null)
				args.Resolve (context);

			return true;
		}
	}

	internal class Identifier : AST {

		internal string name;

		internal Identifier (string id)
		{
			this.name = id;
		}

		public override string ToString ()
		{
			return name;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (name == "print")
				return SemanticAnalyser.print;
			else if (context.Contains (name))
				return true;
			else throw new Exception ("variable not found: " +  name);
		}
	}

	public class Args : AST {

		internal ArrayList elems;

		internal Args ()
		{
			elems = new ArrayList ();
		}

		internal void Add (AST e)
		{
			elems.Add (e);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			int i, size = elems.Count;
			AST tmp;

			for (i = 0; i < size; i++) {
				tmp = (AST) elems [i];
				tmp.Resolve (context);
			}
			return true;
		}
	}

	public class Expression : AST {

		internal ArrayList exprs;

		internal Expression ()
		{
			exprs = new ArrayList ();
		}

		internal void Add (AST a)
		{
			exprs.Add (a);
		}

		public override string ToString ()
		{
			int size = exprs.Count;		

			if (size > 0) {
				int i;
				StringBuilder sb = new StringBuilder ();

				for (i = 0; i < size; i++)
					sb.Append (exprs [i].ToString ());
					sb.Append ("\n");
				return sb.ToString ();

			} else return String.Empty;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			int i, size = exprs.Count;

			for (i = 0; i < size; i++)
				((AST) exprs [i]).Resolve (context);

			return true;
		}
	}
}
