//
// LenientFunctionPrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript.Tmp
{
	public sealed class LenientFunctionPrototype : FunctionPrototype
	{
		public new object constructor;
		public new object apply;
		public new object call;
		public new object toString;
	}
}