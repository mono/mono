//
// ScriptBlock.cs: Represents a file, which maps to a 'JScript N' class in the assembly.
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// Copyright (C) 2005 Novell Inc (http://novell.com)
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
using System.Reflection;
using Microsoft.JScript.Vsa;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Microsoft.JScript {

	public class ScriptBlock : AST, ICanModifyContext {

		private TypeBuilder type_builder;
		private MethodBuilder global_code;
		private ILGenerator global_code_ig;
		private EmitContext base_emit_context;
		internal Block src_elems;
		
		internal MethodBuilder GlobalCode {
			get { return global_code; }
		}

		internal TypeBuilder TypeBuilder {
			get { return type_builder; }
		}

		internal ScriptBlock ()
			: base (null, null)
		{
			src_elems = new Block (null, null);
		}

		internal ScriptBlock (Location location)
			: base (null, location)
		{
			src_elems = new Block (null, location);
		}

		internal void Add (AST e)
		{
			src_elems.Add (e);
		}

		internal void EmitDecls (ModuleBuilder mb)
		{
			base_emit_context = new EmitContext (type_builder, mb, global_code_ig);
			EmitInitGlobalCode ();
			((ICanModifyContext) src_elems).EmitDecls (base_emit_context);
		}

		internal void Emit ()
		{
			Emit (base_emit_context);
			EmitEndGlobalCode ();
		}

		internal override void Emit (EmitContext ec)
		{
			EmitTypeCtr ();
			src_elems.Emit (ec);
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			((ICanModifyContext) src_elems).PopulateContext (env, ns);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
		}

		internal override bool Resolve (Environment env)
		{
			return src_elems.Resolve (env);
		}


		internal void InitTypeBuilder (ModuleBuilder moduleBuilder, string next_type)
		{ 
			type_builder = moduleBuilder.DefineType (next_type, TypeAttributes.Public);
			type_builder.SetParent (typeof (GlobalScope));
			type_builder.SetCustomAttribute (new CustomAttributeBuilder
							 (typeof (CompilerGlobalScopeAttribute).GetConstructor (new Type [] {}), new object [] {}));
		}

		internal void CreateType ()
		{
			type_builder.CreateType ();
		}

		internal void EmitTypeCtr ()
		{
			ConstructorBuilder cons_builder;
			cons_builder = type_builder.DefineConstructor (MethodAttributes.Public,
							       CallingConventions.Standard,
							       new Type [] { typeof (GlobalScope) });
			ILGenerator ig = cons_builder.GetILGenerator ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldarg_1);
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Ldfld,
				 typeof (ScriptObject).GetField ("engine"));
			
			ig.Emit (OpCodes.Call, 
				 typeof (GlobalScope).GetConstructor (new Type [] {typeof (GlobalScope), 
										   typeof (VsaEngine)}));
			ig.Emit (OpCodes.Ret);
		}

		internal void InitGlobalCode ()
		{
			global_code = type_builder.DefineMethod ("Global Code", MethodAttributes.Public,
							 typeof (System.Object), new Type [] {});
			global_code_ig = global_code.GetILGenerator ();
		}

		private void EmitInitGlobalCode ()
		{
			ILGenerator ig = global_code_ig;

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Call,
				typeof (VsaEngine).GetMethod ("PushScriptObject",
							      new Type [] { typeof (ScriptObject)}));
		}

		private void EmitEndGlobalCode ()
		{
			ILGenerator ig = global_code_ig;

			ig.Emit (OpCodes.Ldnull);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("PopScriptObject"));
			ig.Emit (OpCodes.Pop);
			ig.Emit (OpCodes.Ret);
		}
	}
}
