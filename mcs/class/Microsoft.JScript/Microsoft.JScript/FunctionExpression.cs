//
// FunctionExpression.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;

namespace Microsoft.JScript {

	public class FunctionExpression : AST {

		internal FunctionObject Function;

		internal FunctionExpression (string name, 
					     FormalParameterList p,
					     Block body)
		{
			Function = new FunctionObject (name, p, body);
		}
						

		internal FunctionExpression ()
		{
			Function = new FunctionObject ();
		}

		public static FunctionObject JScriptFunctionExpression (RuntimeTypeHandle handle, string name,
									string methodName, string [] formalParams,
									JSLocalField [] fields)
		{
			throw new NotImplementedException ();
		}
	}
}
