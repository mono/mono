//
// Globals.cs:
// 
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public sealed class Globals
	{
		public static VsaEngine contextEngine;

	
		public static ArrayObject ConstructArray (params object [] args)
		{
			throw new NotImplementedException ();
		}


		public static ArrayObject ConstructArrayLiteral (object [] args)
		{
			throw new NotImplementedException ();
		}
	}
}