//
// FunctionDeclaration.cs:
//
// Author:
//	 Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) 2005 Novell Inc.
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
	
	public class FunctionDeclaration : Function, ICanModifyContext {

		private int lexical_depth;

		internal FunctionDeclaration ()
			: base (null, null)
		{
		}

		internal FunctionDeclaration (AST parent, string name, Location location)
			: this (parent, name, null, String.Empty, null, location)
		{
		}
		
		internal FunctionDeclaration (AST parent, string name, 
					      FormalParameterList p,
					      string return_type,
					      Block body, Location location)
			: base (parent, location)
		{
			set_prefix ();
			func_obj = new FunctionObject (name, p, return_type, body, location);
		}

		public static Closure JScriptFunctionDeclaration (RuntimeTypeHandle handle, string name, 
								  string methodName, string [] formalParameters,
								  JSLocalField [] fields, bool mustSaveStackLocals,
								  bool hasArgumentsObjects, string text, 
								  Object declaringObject, VsaEngine engine)
		{
			FunctionObject f = new FunctionObject (name, null, null, null, null);
			f.source = text;
			MethodInfo method = engine.ScriptObjectStackTop ().GetType ().GetMethod (methodName);
			f.method = method;
			f.vsa_engine = engine;
			return new Closure (f);
		}

		internal void create_closure (EmitContext ec)
		{
			string name = func_obj.name;
			string full_name;
			TypeBuilder type = ec.type_builder;
			ILGenerator ig = ec.ig;			
		
			if (prefix == String.Empty) 
				full_name = name;
			else 
				full_name = prefix + "." + name;

			MethodBuilder method_builder = type.DefineMethod (full_name, func_obj.attr, HandleReturnType,
								  func_obj.params_types ());
			MethodBuilder tmp = (MethodBuilder) TypeManager.Get (name);


			if (tmp == null)
				TypeManager.Add (name, method_builder);
			else 
				TypeManager.Set (name, method_builder);

			set_custom_attr (method_builder);
			this.ig = method_builder.GetILGenerator ();

			if (parent == null || parent.GetType () == typeof (ScriptBlock))
				type.DefineField (name, typeof (Microsoft.JScript.ScriptFunction),
						  FieldAttributes.Public | FieldAttributes.Static);
			else {
				local_func = ig.DeclareLocal (typeof (Microsoft.JScript.ScriptFunction));
				TypeManager.AddLocalScriptFunction (name, local_func);
			}
			build_closure (ec, full_name, func_obj.source);
		}

		internal override void Emit (EmitContext ec)
		{
			TypeManager.BeginScope ();
			ILGenerator old_ig = ec.ig;
			ec.ig = this.ig;

			((ICanModifyContext) func_obj.body).EmitDecls (ec);
			func_obj.body.Emit (ec);

			string func_name = func_obj.name;
			
			if (SemanticAnalyser.MethodContainsEval (func_name))
				CodeGenerator.load_local_vars (ec.ig, true);
			else {
				VariableDeclaration decl = SemanticAnalyser.OutterScopeVar (func_name);
				if (decl == null) {
					decl = SemanticAnalyser.VarUsedNested (func_name);
					if (decl != null)
						CodeGenerator.load_local_vars (ec.ig, InFunction);
				} else
					CodeGenerator.locals_to_stack_frame (ec.ig, decl.lexical_depth, lexical_depth - decl.lexical_depth, true);
			}
			this.ig.Emit (OpCodes.Ret);
			ec.ig = old_ig;
			TypeManager.EndScope ();
		}
		

		internal void build_closure (EmitContext ec, string full_name, string encodedSource)
		{
			ILGenerator ig = ec.ig;
			string name = func_obj.name;
			Type t = ec.mod_builder.GetType (CodeGenerator.GetTypeName (Location.SourceName));
			ig.Emit (OpCodes.Ldtoken, t);
			ig.Emit (OpCodes.Ldstr, name);
			ig.Emit (OpCodes.Ldstr, full_name);

			func_obj.parameters.Emit (ec);
			build_local_fields (ig);

			//
			// If we have en eval method call, we have to 
			// save the loca vars in the stack
			//
			if (SemanticAnalyser.MethodContainsEval (name) ||
			    SemanticAnalyser.MethodVarsUsedNested (name))
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);

			ig.Emit (OpCodes.Ldc_I4_0); // FIXME: this hard coded for now.
			ig.Emit (OpCodes.Ldstr, Decompiler.Decompile (encodedSource, 0, 0).Trim ());
			ig.Emit (OpCodes.Ldnull); // FIXME: this hard coded for now.

			CodeGenerator.load_engine (InFunction, ig);

			ig.Emit (OpCodes.Call, typeof (FunctionDeclaration).GetMethod ("JScriptFunctionDeclaration"));

			if (parent == null || parent.GetType () == typeof (ScriptBlock))
				ig.Emit (OpCodes.Stsfld, t.GetField (name));
			else					
				ig.Emit (OpCodes.Stloc, local_func);	
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

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			//
			// In the case of function
			// declarations we add
			// function's name to the
			// table but we resolve its
			// body until later, as free
			// variables can be referenced
			// in function's body.
			//
			string name = func_obj.name;
			AST binding = (AST) env.Get (String.Empty, Symbol.CreateSymbol (name));

			if (binding == null)
				env.Enter (String.Empty, Symbol.CreateSymbol (name), this);
			else if (binding is FunctionDeclaration || binding is VariableDeclaration) {
				Console.WriteLine ("{0}({1},0) : warning JS1111: '{2}' is already defined",
					   Location.SourceName, Location.LineNumber, name);
			}

			if (binding is VariableDeclaration) {
				VariableDeclaration var_decl = (VariableDeclaration) binding;
				string error_msg = Location.SourceName + "(" + var_decl.Location.LineNumber + ",0) : "
					+ "error JS5040: '" + var_decl.id + "' is read-only";
				throw new Exception (error_msg);
			}
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			((ICanModifyContext) func_obj.body).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			set_function_type ();
			env.BeginScope (String.Empty);
			lexical_depth = env.Depth (String.Empty);

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
	}
}
