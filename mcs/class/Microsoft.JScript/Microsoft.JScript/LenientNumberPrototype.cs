//
// LenientFunctionPrototype.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public sealed class LenientNumberPrototype : NumberPrototype
	{
		public new object constructor;
		public new object toExponential;
		public new object toFixed;
		public object toLocaleString;
		public new object toPrecision;
		public new object toString;
		public new object valueOf;
	}
}