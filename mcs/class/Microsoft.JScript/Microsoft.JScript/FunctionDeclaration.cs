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
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	public class FunctionDeclaration : AST {

		internal FunctionObject Function;

		internal FunctionDeclaration (string name, 
					      FormalParameterList p,
					      Block body)
		{
			Function = new FunctionObject (name, p,body);
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
	}
}
