//
// Binding.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public abstract class Binding : AST
	{
		public static bool IsMissing (Object value)
		{
			throw new NotImplementedException ();
		}
	}
}		