//
// Try.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004, Cesar Octavio Lopez Nataren
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
using Microsoft.JScript.Vsa;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Microsoft.JScript {

	public sealed class Try : AST {

 		internal FieldBuilder field_info;
 		internal LocalBuilder local_builder;
		
		internal AST guarded_block;
		internal ArrayList catch_blocks;
		internal AST finally_block;


		internal Try (AST guarded_block, ArrayList catch_block, AST finally_block, AST parent, int line_number)
		{
			this.parent = parent;
			this.guarded_block = guarded_block;
			this.catch_blocks = catch_block;
			this.finally_block = finally_block;
		}		

		public static Object JScriptExceptionValue (object e, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		public static void PushHandlerScope (VsaEngine engine, string id, int scopeId)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (guarded_block != null)
				r &= guarded_block.Resolve (context);
			
			if (catch_blocks != null && catch_blocks.Count > 0) {
				foreach (Catch c in catch_blocks) {
					context.OpenBlock ();
					context.Enter (c.id, c);
					r &= c.Resolve (context);
					context.CloseBlock ();
				}
			}
			if (finally_block != null)
				r &= finally_block.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type t = typeof (object);
			bool not_inside_func = parent == null;
			
			ig.BeginExceptionBlock ();

			if (guarded_block != null)
				guarded_block.Emit (ec);

			if (catch_blocks != null && catch_blocks.Count > 0) {
				foreach (Catch c in catch_blocks)
					c.Emit (ec);
			}	       	
			if (finally_block != null) {
				ig.BeginFinallyBlock ();
				finally_block.Emit (ec);
			}
			ig.EndExceptionBlock ();
		}
	}
}
