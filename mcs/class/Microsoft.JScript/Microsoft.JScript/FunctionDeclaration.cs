//
// FunctionDeclaration.cs:
//
// Author:
//	 Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using Microsoft.JScript.Vsa;
using System.Collections;

namespace Microsoft.JScript {
	
	public class FunctionDeclaration : Function {

		internal FunctionDeclaration (AST parent, string name)
			: this (parent, name, null, String.Empty, null)
		{
		}
		
		internal FunctionDeclaration (AST parent, string name, 
					      FormalParameterList p,
					      string return_type,
					      Block body)
		{
			this.parent = parent;
			set_prefix ();
			func_obj = new FunctionObject (name, p, return_type, body);
		}

		public static Closure JScriptFunctionDeclaration (RuntimeTypeHandle handle, string name, 
								  string methodName, string [] formalParameters,
								  JSLocalField [] fields, bool mustSaveStackLocals,
								  bool hasArgumentsObjects, string text, 
								  Object declaringObject, VsaEngine engine)
		{
			FunctionObject f = null;
			return new Closure (f);
		}

		public override string ToString ()
		{
			return func_obj.ToString ();
		}

		internal override void Emit (EmitContext ec)
		{			
			string name = func_obj.name;
			string full_name;
			TypeBuilder type = ec.type_builder;
			ILGenerator ig = ec.ig;			
			
			if (prefix == String.Empty) 
				full_name = name;
			else 
				full_name = prefix + "." + name;

			MethodBuilder method_builder = type.DefineMethod (full_name, func_obj.attr, 
									  func_obj.return_type,
									  func_obj.params_types ());
			TypeManager.AddMethod (name, method_builder);
			
			set_custom_attr (method_builder);
			EmitContext new_ec = new EmitContext (ec.type_builder, ec.mod_builder,
							      method_builder.GetILGenerator ());
			if (parent == null || parent.GetType () == typeof (ScriptBlock))
				type.DefineField (name, typeof (Microsoft.JScript.ScriptFunction),
						  FieldAttributes.Public | FieldAttributes.Static);
			else
				local_func = ig.DeclareLocal (typeof (Microsoft.JScript.ScriptFunction));
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
			ig.Emit (OpCodes.Ldstr, name);
			ig.Emit (OpCodes.Ldstr, full_name);

			func_obj.parameters.Emit (ec);
			build_local_fields (ig);

			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldstr, "STRING_REPRESENTATION_OF_THE_FUNCTION"); // FIXME
			ig.Emit (OpCodes.Ldnull); // FIXME: this hard coded for now.

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (FunctionDeclaration).GetMethod ("JScriptFunctionDeclaration"));

			if (parent == null || parent.GetType () == typeof (ScriptBlock))
				ig.Emit (OpCodes.Stsfld, t.GetField (name));
			else					
				ig.Emit (OpCodes.Stloc, local_func);	
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

		internal override bool Resolve (IdentificationTable context)
		{
			set_function_type ();
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
	}
}
