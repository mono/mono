//
// VariableDeclaration.cs: The AST representation of a VariableDeclaration.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System.Text;

	public class VariableDeclaration : Statement
	{
		private string id;
		private AST assignExp;

		internal VariableDeclaration ()
		{}


		public string Id {
			get { return id; }
			set { id = value; }
		}


		public override object Visit (Visitor v, object args)
		{
			return v.VisitVariableDeclaration (this, args);
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			// FIXME: we must add the string 
			// representation of assignExp, too.
			sb.Append (Id);
			
			return sb.ToString ();
		}
	}
}
