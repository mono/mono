//
// Literal.cs: This class groups the differents types of Literals.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) Cesar Lopez Nataren 
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class Literal : AST
	{
		public Literal ()
		{}

		internal override object Visit (Visitor v, object args)
		{
			return null;
		}
	}
}