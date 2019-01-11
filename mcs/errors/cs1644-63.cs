// CS1644: Feature `extensible fixed statement' cannot be used because it is not part of the C# 7.2 language specification 
// Line: 11
// Compiler options: -unsafe -langversion:7.2

using System;

unsafe class C
{
	public static void Main ()
	{
		fixed (int* p = new Fixable ()) {
		}
	}

	struct Fixable
	{
		public ref int GetPinnableReference ()
		{
			throw new NotImplementedException ();
		}
	}
}