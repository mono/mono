//
// LenientObjectPrototype.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	public class LenientObjectPrototype : ObjectPrototype
	{
		public new object constructor;
		public new object hasOwnProperty;
		public new object isPrototypeOf;
		public new object propertyIsEnumerable;
		public new object toLocaleString;
		public new object toString;
		public new object valueOf;
	}
}