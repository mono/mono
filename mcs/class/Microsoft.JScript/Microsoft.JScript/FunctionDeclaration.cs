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

		internal FunctionDeclaration (string name, 
					      FormalParameterList p,
					      string return_type,
					      Block body)
		{
			Function = new FunctionObject (name, p, 
						       return_type, body);
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

		internal override void Emit (EmitContext ec)
		{
			TypeBuilder type;
			MethodBuilder method;

			string name = Function.name;

			type = ec.type_builder;

			type.DefineField (name, 
					  typeof (Microsoft.JScript.ScriptFunction),
					  FieldAttributes.Public |
					  FieldAttributes.Static);
		}
	}
}
