//
// Print.cs: The AST representation of a print_statement.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class Print : Statement
	{
		internal AST exp;

		public AST Exp {
			get { return exp; }
			set { exp = value; }
		}


		internal Print ()
		{}


		internal override object Visit (Visitor v, object args)
		{
			return v.VisitPrint (this, args);
		}


		public override string ToString ()
		{
			return exp.ToString ();
		}
	}
}