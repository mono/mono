//
// LenientBooleanPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	public sealed class LenientBooleanPrototype : BooleanPrototype
	{
		public new object constructor;
		public new object toString;
		public new object valueOf;
	}
}
