//
// VariableDeclaration.cs: The AST representation of a VariableDeclaration.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class VariableDeclaration : Statement {

		private string id;
		private Type type;
		private AST val;

		internal VariableDeclaration (string id, string t, AST init)
		{
			this.id = id;

			if (t == null)
				this.type = typeof (System.Object);
			
			this.val = init;
		}


		public string Id {
			get { return id; }
			set { id = value; }
		}

		
		public Type Type {
			get { return type; }
			set { type = value; }
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (Id);
			sb.Append (" = ");

			if (val != null)
				sb.Append (val.ToString ());

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			FieldBuilder field;
			TypeBuilder type  = ec.type_builder;

			field = type.DefineField (id, Type,
						  FieldAttributes.Public |
						  FieldAttributes.Static);
						  
		}
	}
}
