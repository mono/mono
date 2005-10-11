//
// ArrayLiteral.cs:
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
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class ArrayLiteral : AST, ICanLookupPrototype {

		internal ASTList elems;
		int skip_count;

		public ArrayLiteral (Context context, ASTList elems)
			: base (null, null)
		{
			this.elems = elems;
		}
		
		internal ArrayLiteral (Context context, ASTList elems, int skip_count, Location location)
			: this (context, elems)
		{
			this.skip_count = skip_count;
			this.location = location;
		}

		internal override bool Resolve (Environment env)
		{
			 bool r = true;
			 foreach (AST ast in elems.elems)
				 if (ast != null)
					 r &= ast.Resolve (env);
			return r;
		}

		bool ICanLookupPrototype.ResolveFieldAccess (AST ast)
		{
			if (ast is Identifier) {
				Identifier name = (Identifier) ast;
				Type prototype = typeof (StringPrototype);
				MemberInfo [] members = prototype.GetMember (name.name.Value);
				return members.Length > 0;
			} else
				return false;
		}

		internal override void Emit (EmitContext ec)
		{
			int i = 0;
			ILGenerator ig = ec.ig;
			ArrayList exps = elems.elems;
			FieldInfo missing = null;
			if (skip_count != 0)
				missing = typeof (Missing).GetField ("Value");
			ig.Emit (OpCodes.Ldc_I4, elems.Size);
			ig.Emit (OpCodes.Newarr, typeof (object));
			foreach (AST ast in exps) {
				ig.Emit (OpCodes.Dup);
 				ig.Emit (OpCodes.Ldc_I4, i);
				if (ast != null) {
					ast.Emit (ec);
					CodeGenerator.EmitBox (ig, ast);
				} else 
					ig.Emit (OpCodes.Ldsfld, missing);
 				ig.Emit (OpCodes.Stelem_Ref);
				i++;
			}
 			ig.Emit (OpCodes.Call, typeof (Globals).GetMethod ("ConstructArrayLiteral"));
		}
	}
}
