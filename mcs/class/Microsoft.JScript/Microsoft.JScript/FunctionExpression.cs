//
// FunctionExpression.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class FunctionExpression : AST
	{
		public static FunctionObject JScriptFunctionExpression (RuntimeTypeHandle handle, string name,
									string methodName, string [] formalParams,
									JSLocalField [] fields)
		{
			throw new NotImplementedException ();
		}


		internal override object Visit (Visitor v, object args)
		{
			return v.VisitFunctionExpression (this, args);
		}
	}
}
