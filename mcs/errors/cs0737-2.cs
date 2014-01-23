// CS0737: `MyTest' does not implement interface member `System.ICloneable.Clone()' and the best implementing candidate `MyTest.Clone()' is not public
// Line: 6

using System;

public class MyTest : ICloneable
{
	object Clone()
	{
		return MemberwiseClone();
	}
}

