//
// Enum.cs: AST representation of a enum_statement.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;
	using System.Text;
	using System.Collections;

	public class Enum : Statement
	{
		private ArrayList modifiers;
		private string name;
		private string type;		
		private ArrayList pairs;

		internal Enum ()
		{
			Modifiers = new ArrayList ();
			Pairs = new ArrayList ();
		}


		internal ArrayList Modifiers {
			get { return modifiers; }
			set { modifiers = value; }
		}

		internal string Name {
			get { return name; }
			set { name = value; }
		}


		internal string Type {
			get { return type; }
			set { type = value; }
		}


		internal ArrayList Pairs {
			get { return pairs; }
			set { pairs = value; }
		}


		internal override object Visit (Visitor v, object args)
		{
			throw new NotImplementedException ();
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			foreach (string modifier in Modifiers)
				sb.Append (modifier + "\n");

			sb.Append (name + "\n");
			sb.Append (type + "\n");
			
			foreach (BinaryOp bop in Pairs)
				sb.Append (bop.ToString () + "\n");

			return sb.ToString ();
		}
					
	}
}
