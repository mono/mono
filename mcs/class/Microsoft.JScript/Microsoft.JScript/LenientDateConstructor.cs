//
// LenientDateConstructor.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public sealed class LenientDateConstructor : DateConstructor
	{
		public new object parse;
		public new object UTC;
	}
}