//
// LenientRegexpPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public sealed class LenientRegExpPrototype : RegExpPrototype
	{
		public new object constructor;
		public new object compile;
		public new object exec;
		public new object test;
		public new object toString;
	}
}