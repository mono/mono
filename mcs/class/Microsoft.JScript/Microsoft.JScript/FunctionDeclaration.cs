//
// FunctionDeclaration.cs:
//
// Author:
//	 Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.JScript.Vsa;
using System.Collections;

namespace Microsoft.JScript {

	public class FunctionDeclaration : AST {

		internal FunctionObject Function;
		internal DictionaryEntry [] locals;
		internal LocalBuilder local_func;

		internal FunctionDeclaration (AST parent, string name, 
					      FormalParameterList p,
					      string return_type,
					      Block body)
		{
			this.parent = parent;
			Function = new FunctionObject (name, p, return_type, body);
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
		
		internal FunctionDeclaration ()
		{
			Function = new FunctionObject ();
		}


		public override string ToString ()
		{
			return Function.ToString ();
		}

		internal string get_composite_name ()
		{
			string parent_name, full_name;
			FunctionDeclaration p = parent as FunctionDeclaration;

			if (p.parent != null)
				parent_name = p.get_composite_name ();
			else parent_name = p.Function.name;

			full_name = parent_name + "." + Function.name;

			return full_name;
		}

		internal override void Emit (EmitContext ec)
		{
			TypeBuilder type = ec.type_builder;
			MethodBuilder method;
			string name;
			
			if (parent == null) {
				name = Function.name;
				type.DefineField (name, 
						  typeof (Microsoft.JScript.ScriptFunction),
						  FieldAttributes.Public | 
						  FieldAttributes.Static);
				build_closure (ec);
				method = type.DefineMethod (name, Function.attr, 
							    Function.return_type,
							    Function.params_types ());
				ec.ig = method.GetILGenerator ();
				Function.body.Emit (ec);
				ec.ig.Emit (OpCodes.Ret);
				return;
			} else {
				name = get_composite_name ();
				local_func = ec.ig.DeclareLocal (typeof (Microsoft.JScript.ScriptFunction));
				
				method = type.DefineMethod (name, Function.attr, 
							    Function.return_type,
							    Function.params_types ());
				EmitContext new_ec = new EmitContext (ec.type_builder, ec.mod_builder, ec.ig, method.GetILGenerator ());
				build_closure_nested (new_ec);				
				Function.body.Emit (new_ec);
				new_ec.ig.Emit (OpCodes.Ret);
			}
		}
		
		internal void build_closure (EmitContext ec)
		{
			ILGenerator ig = ec.gc_ig;
			string name = Function.name;
			Type t = ec.mod_builder.GetType ("JScript 0");
			ig.Emit (OpCodes.Ldtoken, t);
			ig.Emit (OpCodes.Ldstr, name);
			ig.Emit (OpCodes.Ldstr, name);
			Function.parameters.Emit (ec);
			build_local_fields (ig);

			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldstr, "STRING_REPRESENTATION_OF_THE_FUNCTION"); // FIXME
			ig.Emit (OpCodes.Ldnull); // FIXME: this hard coded for now.

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (FunctionDeclaration).GetMethod ("JScriptFunctionDeclaration"));
			ig.Emit (OpCodes.Stsfld, ec.mod_builder.GetType ("JScript 0").GetField (name));
		}

		internal void build_closure_nested (EmitContext ec)
		{
			ILGenerator ig = ec.gc_ig;
			string name = get_composite_name ();
			Type t = ec.mod_builder.GetType ("JScript 0");
			ig.Emit (OpCodes.Ldtoken, t);
			ig.Emit (OpCodes.Ldstr, Function.name);
			ig.Emit (OpCodes.Ldstr, name);
			Function.parameters.Emit (ec);
			build_local_fields (ig);

			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldstr, "STRING_REPRESENTATION_OF_THE_FUNCTION"); // FIXME
			ig.Emit (OpCodes.Ldnull); // FIXME: this hard coded for now.

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (FunctionDeclaration).GetMethod ("JScriptFunctionDeclaration"));
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

				ig.Emit (OpCodes.Ldc_I4, i);
				ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] { 
										typeof (string), 
										typeof (RuntimeTypeHandle),
										typeof (Int32) }));
				ig.Emit (OpCodes.Stelem_Ref);
			}
		}

		internal override bool Resolve (IdentificationTable context)
		{
			context.Enter (Function.name, this);
			context.OpenBlock ();
			FormalParameterList p = Function.parameters;

			if (p != null)
				p.Resolve (context);

			Block body = Function.body;
			if (body != null)
				body.Resolve (context);

			locals = context.current_locals;
			context.CloseBlock ();		
			return true;
		}
	}
}
