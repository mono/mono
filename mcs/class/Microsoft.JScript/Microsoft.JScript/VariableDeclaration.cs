//
// VariableDeclaration.cs: The AST representation of a VariableDeclaration.
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {
	
	public class VariableDeclaration : AST {

		internal string id;
		internal Type type;
		internal string type_annot = String.Empty;
		internal AST val;

		internal FieldInfo field_info;
		internal LocalBuilder local_builder;

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
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (id);
			if (type_annot != String.Empty) {				
				sb.Append (":" + type_annot);
				sb.Append (" = ");
			}

			if (val != null)
				sb.Append (val.ToString ());

			return sb.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (parent == null || (parent.GetType () != typeof (FunctionDeclaration)
					       && parent.GetType () != typeof (FunctionExpression))) {
				FieldBuilder field_builder;
				TypeBuilder type  = ec.type_builder;
				
				field_builder = type.DefineField (id, this.type, FieldAttributes.Public | FieldAttributes.Static);
				field_info = field_builder;

				if (val != null) {
					val.Emit (ec);
					ig.Emit (OpCodes.Stsfld, field_builder);
				}
			} else {
				local_builder = ig.DeclareLocal (type);
				
				if (val != null) {
					val.Emit (ec);
					ig.Emit (OpCodes.Stloc, local_builder);
				}
			}
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (val != null)
				r = val.Resolve (context);
			context.Enter (id, this);
			return r;
		}
	}
}
