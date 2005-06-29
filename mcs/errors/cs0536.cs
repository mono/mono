// cs0536.cs: `MyTest' does not implement interface member `System.ICloneable.Clone()'. `MyTest.Clone()' is either static, not public, or has the wrong return type
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

