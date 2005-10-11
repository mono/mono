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

	public sealed class Try : AST, ICanModifyContext {

 		internal FieldBuilder field_info;
 		internal LocalBuilder local_builder;
		
		internal AST guarded_block;
		internal ArrayList catch_blocks;
		internal AST finally_block;


		internal Try (AST guarded_block, ArrayList catch_block, AST finally_block, AST parent, Location location)
			: base (parent, location)
		{
			this.guarded_block = guarded_block;
			this.catch_blocks = catch_block;
			this.finally_block = finally_block;
		}		

		public static Object JScriptExceptionValue (object e, VsaEngine engine)
		{
			Exception exc = e as Exception;
			string message = null;
			if (exc != null)
				message = exc.Message;
			else
				message = String.Format ("Unknown exception of type {0}", exc.GetType ());
			return new ErrorObject (message);
		}

		public static void PushHandlerScope (VsaEngine engine, string id, int scopeId)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (guarded_block != null)
				r &= guarded_block.Resolve (env);

			if (catch_blocks != null && catch_blocks.Count > 0) {
				foreach (Catch c in catch_blocks) {
					env.BeginScope (String.Empty, true);
					env.Enter (String.Empty, Symbol.CreateSymbol (c.id), c);
					r &= c.Resolve (env);
					env.EndScope (String.Empty);
				}
			}
			if (finally_block != null)
				r &= finally_block.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
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


		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (guarded_block is ICanModifyContext)
				((ICanModifyContext) guarded_block).PopulateContext (env, ns);

			foreach (AST ast in catch_blocks)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);

			if (finally_block is ICanModifyContext)
				((ICanModifyContext) finally_block).PopulateContext (env, ns);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (guarded_block is ICanModifyContext)
				((ICanModifyContext) guarded_block).EmitDecls (ec);

			foreach (AST ast in catch_blocks)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);

			if (finally_block is ICanModifyContext)
				((ICanModifyContext) finally_block).EmitDecls (ec);
		}
	}
}
