// CS0619-39: `C.ob' is obsolete: `ooo'
// Line: 13

using System;

class C
{
	[Obsolete("ooo", true)]
	const int ob = 4;
	
    public int Prop {
		get {
			return ob;
		}
	}
}
