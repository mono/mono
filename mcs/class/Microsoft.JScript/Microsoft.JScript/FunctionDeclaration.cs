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

namespace Microsoft.JScript {

	public class FunctionDeclaration : AST {

		internal FunctionObject Function;

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
			throw new NotImplementedException ();
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
			} else {
				name = get_composite_name ();
				ec.ig.DeclareLocal (typeof (Microsoft.JScript.ScriptFunction));
			}
			method = type.DefineMethod (name, Function.attr, 
						    Function.return_type,
						    Function.params_types ());
	
			ec.ig = method.GetILGenerator ();		
			Function.body.Emit (ec);
			ec.ig.Emit (OpCodes.Ret);
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

			context.CloseBlock ();
		
			return true;
		}
	}
}
