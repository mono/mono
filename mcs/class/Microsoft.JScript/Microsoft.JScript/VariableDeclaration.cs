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

	public class VariableDeclaration : AST {

		private string id;
		private Type type;
		private string type_annot;
		private AST val;
		internal AST parent;

		internal VariableDeclaration (AST parent, string id, string t, AST init)
		{
			this.parent = parent;
			this.id = id;

			if (t == null)
				this.type = typeof (System.Object);
			else {
				this.type_annot = t;
				// FIXME: resolve the type annotations
				this.type = typeof (System.Object);
			}

			this.val = init;
		}


		public string Id {
			get { return id; }
			set { id = value; }
		}

		public AST InitValue {
			get { return val; }
		}

		public Type Type {
			get { return type; }
			set { type = value; }
		}


		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (Id);
			sb.Append (":" + type_annot);
			sb.Append (" = ");

			if (val != null)
				sb.Append (val.ToString ());

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			if (parent == null) {
				FieldBuilder field;
				TypeBuilder type  = ec.type_builder;

				field = type.DefineField (id, Type,
						  	  FieldAttributes.Public |
						  	  FieldAttributes.Static);
			} else
				ec.ig.DeclareLocal (Type);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			context.Enter (id, this);
			Console.WriteLine ("VariableDeclaration::Resolve");
			return true;
		}
	}
}
