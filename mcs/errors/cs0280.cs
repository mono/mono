// CS0280: `C.Fixable.GetPinnableReference(int)' has the wrong signature to be used in extensible fixed statement
// Line: 11
// Compiler options: -unsafe -langversion:latest -warnaserror

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
		public ref int GetPinnableReference (int i = 1)
		{
			throw new NotImplementedException ();
		}
	}
}