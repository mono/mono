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

		internal FunctionExpression (AST parent, string name, 
					     FormalParameterList p,
					     string return_type, Block body)
		{
			this.parent = parent;
			Function = new FunctionObject (name, p, return_type, body);
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

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
