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
	
	internal class VariableDeclaration : AST, ICanModifyContext {

		internal string id;
		internal Type type;
		internal string type_annot = String.Empty;
		internal AST val;

		internal FieldInfo field_info;
		internal LocalBuilder local_builder;

		internal int lexical_depth;
		internal Function func_decl;

		internal VariableDeclaration (AST parent, string id, string t, AST init, Location location)
			: base (parent, location)
		{
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

			return "var " + sb.ToString ();
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			Symbol _id = Symbol.CreateSymbol (id);

			if (!env.InCurrentScope (ns, _id))
				env.Enter (ns, _id, this);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			object var;
			
			if ((var = CodeGenerator.variable_defined_in_current_scope (id)) != null) {
				Type t = var.GetType ();
				if (t == typeof (FieldBuilder))
					field_info = (FieldBuilder) var;
				else if (t == typeof (LocalBuilder))
					local_builder = (LocalBuilder) var;
				return;
			}
			
			ILGenerator ig = ec.ig;
			if (parent == null || (parent.GetType () != typeof (FunctionDeclaration)
					       && parent.GetType () != typeof (FunctionExpression))) {
				FieldBuilder field_builder;
				TypeBuilder type_builder  = ec.type_builder;
				
				field_builder = type_builder.DefineField (id, this.type, FieldAttributes.Public | FieldAttributes.Static);
				TypeManager.Add (id, field_builder);
				field_info = field_builder;
			} else {
				local_builder = ig.DeclareLocal (type);
				TypeManager.Add (id, local_builder);
			}
		}

		internal override void Emit (EmitContext ec)
		{			
			ILGenerator ig = ec.ig;

			if (parent == null || (parent.GetType () != typeof (FunctionDeclaration)
					       && parent.GetType () != typeof (FunctionExpression))) {
				if (val != null) {
					val.Emit (ec);
					CodeGenerator.EmitBox (ig, val);
					ig.Emit (OpCodes.Stsfld, field_info);
				}
			} else {
				if (val != null) {
					val.Emit (ec);
					CodeGenerator.EmitBox (ig, val);
					ig.Emit (OpCodes.Stloc, local_builder);
				}
			}
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
 			if (val != null)
 				r = val.Resolve (env);
			lexical_depth = env.Depth (String.Empty);
			func_decl = GetContainerFunction;
			return r;
		}
	}
}
