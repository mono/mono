//
// FunctionExpression.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Collections;
using System.Reflection.Emit;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	public class FunctionExpression : Function {

		internal LocalBuilder local_script_func;
		internal FieldBuilder field;

		internal FunctionExpression (AST parent, string name)
			: this (parent, name, null, String.Empty, null)
		{
		}

		internal FunctionExpression (AST parent, string name, 
					     FormalParameterList p,
					     string return_type, Block body)
		{
			this.parent = parent;
			set_prefix ();
			func_obj = new FunctionObject (name, p, return_type, body);
		}
						
		public static FunctionObject JScriptFunctionExpression (RuntimeTypeHandle handle, string name,
									string methodName, string [] formalParams,
									JSLocalField [] fields, bool mustSaveStackLocals, 
									bool hasArgumentsObject, string text,
									VsaEngine engine)
		{
			// FIXME: return something useful
			return new FunctionObject (null, null, null, null);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			set_function_type ();
			if (func_obj.name != null && func_obj.name != String.Empty)
				context.Enter (func_obj.name, this);
			context.OpenBlock ();
			FormalParameterList p = func_obj.parameters;

			if (p != null)
				p.Resolve (context);

			Block body = func_obj.body;
			if (body != null)
				body.Resolve (context);

			locals = context.current_locals;
			context.CloseBlock ();		
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			string name = func_obj.name;
			string full_name;
			TypeBuilder type = ec.type_builder;
			ILGenerator ig = ec.ig;			

			if (prefix == null || prefix == String.Empty) {
				if (name == String.Empty)
					full_name = SemanticAnalyser.NextAnonymousMethod;
				else
					full_name = name;
			} else {
				if (name == String.Empty)
					full_name = prefix + "." + SemanticAnalyser.NextAnonymousMethod;
				else 
					full_name = prefix + "." + name;				
			}
			
			MethodBuilder method_builder = type.DefineMethod (full_name, func_obj.attr, 
									  func_obj.return_type,
									  func_obj.params_types ());
			set_custom_attr (method_builder);
			EmitContext new_ec = new EmitContext (ec.type_builder, ec.mod_builder,
							      method_builder.GetILGenerator ());

			if (parent == null || parent.GetType () == typeof (ScriptBlock)) {
				if (name != String.Empty) {
					field = type.DefineField (name, typeof (object), FieldAttributes.Public | FieldAttributes.Static);
					local_func = ig.DeclareLocal (typeof (FunctionObject));
				} else
					local_func = ig.DeclareLocal (typeof (FunctionObject));
			} else if (parent != null && 
				   (parent.GetType () == typeof (FunctionDeclaration)
				    || parent.GetType () == typeof (FunctionExpression))) {
				local_script_func = ig.DeclareLocal (typeof (ScriptFunction));
				local_func = ig.DeclareLocal (typeof (FunctionObject));
			}
			build_closure (ec, full_name);
			func_obj.body.Emit (new_ec);
			new_ec.ig.Emit (OpCodes.Ret);
		}

		internal void build_closure (EmitContext ec, string full_name)
		{
			ILGenerator ig = ec.ig;
			string name = func_obj.name;

			Type t = ec.mod_builder.GetType ("JScript 0");
			ig.Emit (OpCodes.Ldtoken, t);
			
			if (name != String.Empty)
				ig.Emit (OpCodes.Ldstr, name);
			else
				ig.Emit (OpCodes.Ldstr, SemanticAnalyser.CurrentAnonymousMethod);
			
			ig.Emit (OpCodes.Ldstr, full_name);

			func_obj.parameters.Emit (ec);
			build_local_fields (ig);

			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldstr, "STRING_REPRESENTATION_OF_THE_FUNCTION"); // FIXME
			
			if (parent == null || parent.GetType () == typeof (ScriptBlock)) {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			} else if (parent != null && 
				   (parent.GetType () == typeof (FunctionDeclaration)
				    || parent.GetType () == typeof (FunctionExpression)))
				ig.Emit (OpCodes.Ldarg_1);
			
			ig.Emit (OpCodes.Call, typeof (FunctionExpression).GetMethod ("JScriptFunctionExpression"));
			ig.Emit (OpCodes.Stloc, local_func);
			ig.Emit (OpCodes.Ldloc, local_func);
			ig.Emit (OpCodes.Newobj, typeof (Closure).GetConstructor (new Type [] {typeof (FunctionObject)}));
			if (name != String.Empty) {
				ig.Emit (OpCodes.Dup);
				if (parent == null || parent.GetType () == typeof (ScriptBlock))
					ig.Emit (OpCodes.Stsfld, field);
				else if (parent != null && 
					 (parent.GetType () == typeof (FunctionDeclaration)
					  || parent.GetType () == typeof (FunctionExpression)))
					ig.Emit (OpCodes.Stloc, local_script_func);
			} else {				
				if (parent != null &&
				    (parent.GetType () == typeof (FunctionDeclaration)
				     || parent.GetType () == typeof (FunctionExpression))) {
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Stloc, local_script_func);
				}
			}
		}

		internal void build_local_fields (ILGenerator ig)
		{
			DictionaryEntry e;
			object v;		      
			int n;

			if (locals == null)
				n = 0;
			else 
				n = locals.Length;

			Type t = typeof (JSLocalField);
			ConstructorInfo ctr_info =  t.GetConstructor (new Type [] { 
							typeof (string), typeof (RuntimeTypeHandle), typeof (Int32) });
			ig.Emit (OpCodes.Ldc_I4, n);
			ig.Emit (OpCodes.Newarr, t);

			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				e = locals [i];
				ig.Emit (OpCodes.Ldstr, (string) e.Key);
				v = e.Value;

				if (v is VariableDeclaration)
					ig.Emit (OpCodes.Ldtoken, ((VariableDeclaration) v).type);
				else if (v is FormalParam)
					ig.Emit (OpCodes.Ldtoken, ((FormalParam) v).type);
				else if (v is FunctionDeclaration)
					ig.Emit (OpCodes.Ldtoken, typeof (ScriptFunction));
				else if (v is FunctionExpression)
					ig.Emit (OpCodes.Ldtoken, typeof (object));
				
				ig.Emit (OpCodes.Ldc_I4, i);
				ig.Emit (OpCodes.Newobj, ctr_info);
				ig.Emit (OpCodes.Stelem_Ref);
			}
		}	       
	}
}
