// cs0536.cs: Clone method is not public, so it cant implement ICloneable
// Line: 4
using System;
public class MyTest : ICloneable {
	object Clone(){
		return MemberwiseClone();
	}

	static void Main ()
	{
	}
}

