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

	public class FunctionExpression : Function, ICanModifyContext {

		internal LocalBuilder local_script_func;
		internal FieldBuilder field;

		internal FunctionExpression (AST parent, string name, Location location)
			: this (parent, name, null, String.Empty, null, location)
		{
			this.location = location;
		}

		internal FunctionExpression (AST parent, string name, 
					     FormalParameterList p,
					     string return_type, Block body, Location location)
			: base (parent, location)
		{
			func_obj = new FunctionObject (name, p, return_type, body, location);
		}
						
		public static FunctionObject JScriptFunctionExpression (RuntimeTypeHandle handle, string name,
									string methodName, string [] formalParams,
									JSLocalField [] fields, bool mustSaveStackLocals, 
									bool hasArgumentsObject, string text,
									VsaEngine engine)
		{
			MethodInfo method = engine.ScriptObjectStackTop ().GetType ().GetMethod (methodName);
			FunctionObject fun = new FunctionObject (method);
			fun.source = text;
			fun.vsa_engine = engine;
			return fun;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			//
			// In the case of function
			// expressions we add
			// function's name to the
			// table but we resolve its
			// body until later, as free
			// variables can be referenced
			// in function's body.
			//
			string name = func_obj.name;
			AST binding = null;

			if (name != null && name != String.Empty)
				binding = (AST) env.Get (ns, Symbol.CreateSymbol (name));

			if (binding == null)
				env.Enter (ns, Symbol.CreateSymbol (name), new FunctionDeclaration ());
			else {
				Console.WriteLine ("repeated functions are not handled yet");
				throw new NotImplementedException ();
			}
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			((ICanModifyContext) func_obj.body).EmitDecls (ec);
		}
		
		internal override bool Resolve (Environment env)
		{
			set_prefix ();
			set_function_type ();

			if (func_obj.name != null && func_obj.name != String.Empty)
				env.Enter (String.Empty, Symbol.CreateSymbol (func_obj.name), this);
			env.BeginScope (String.Empty);

			((ICanModifyContext) func_obj).PopulateContext (env, String.Empty);
			
			FormalParameterList p = func_obj.parameters;

			if (p != null)
				p.Resolve (env);

			Block body = func_obj.body;
			if (body != null)
				body.Resolve (env);

			locals = env.CurrentLocals (String.Empty);
			env.EndScope (String.Empty);
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			TypeManager.BeginScope ();

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
									  HandleReturnType,
									  func_obj.params_types ());
			MethodBuilder tmp = (MethodBuilder) TypeManager.Get (name);
			if (tmp == null)
				TypeManager.Add (name, method_builder);
			else 
				TypeManager.Set (name, method_builder);
			set_custom_attr (method_builder);
			EmitContext new_ec = new EmitContext (ec.type_builder, ec.mod_builder,
							      method_builder.GetILGenerator ());
			if (InFunction) {
				local_script_func = ig.DeclareLocal (typeof (ScriptFunction));
				local_func = ig.DeclareLocal (typeof (FunctionObject));
			} else {
				if (name != String.Empty) {
					field = type.DefineField (name, typeof (object), FieldAttributes.Public | FieldAttributes.Static);
					local_func = ig.DeclareLocal (typeof (FunctionObject));
				} else
					local_func = ig.DeclareLocal (typeof (FunctionObject));
			}
			build_closure (ec, full_name, func_obj.source);
			func_obj.body.Emit (new_ec);
			new_ec.ig.Emit (OpCodes.Ret);

			TypeManager.EndScope ();
		}

		internal void build_closure (EmitContext ec, string full_name, string encodedSource)
		{
			ILGenerator ig = ec.ig;
			string name = func_obj.name;

			Type t = ec.mod_builder.GetType (CodeGenerator.GetTypeName (Location.SourceName));
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
			ig.Emit (OpCodes.Ldstr, Decompiler.Decompile (encodedSource, 0, 0).Trim ());
			CodeGenerator.load_engine (InFunction, ig);
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
			object e;
			int n;

			if (locals == null)
				n = 0;
			else 
				n = locals.Length;

			Type t = typeof (JSLocalField);
			ConstructorInfo ctr_info =  t.GetConstructor (new Type [] { 
							typeof (string), typeof (RuntimeTypeHandle), typeof (Int32) });
			if (not_void_return)
				ig.Emit (OpCodes.Ldc_I4, n + 1);
			else
				ig.Emit (OpCodes.Ldc_I4, n);

			ig.Emit (OpCodes.Newarr, t);

			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				e = locals [i];
				ig.Emit (OpCodes.Ldstr, GetName (e));

				if (e is VariableDeclaration)
					ig.Emit (OpCodes.Ldtoken, ((VariableDeclaration) e).type);
				else if (e is FormalParam)
					ig.Emit (OpCodes.Ldtoken, ((FormalParam) e).type);
				else if (e is FunctionDeclaration)
					ig.Emit (OpCodes.Ldtoken, typeof (ScriptFunction));
				else if (e is FunctionExpression)
					ig.Emit (OpCodes.Ldtoken, typeof (object));
				
				ig.Emit (OpCodes.Ldc_I4, i);
				ig.Emit (OpCodes.Newobj, ctr_info);
				ig.Emit (OpCodes.Stelem_Ref);
			}
			if (not_void_return)
				emit_return_local_field (ig, ctr_info, n);
		}	       
	}
}
