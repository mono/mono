//
// LenientErrorPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	public sealed class LenientErrorPrototype : ErrorPrototype
	{
		public new object constructor;
		public new object name;
		public new object toString;
	}
}