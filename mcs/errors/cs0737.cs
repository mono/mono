// CS0737: `MySubClass' does not implement interface member `System.ICloneable.Clone()' and the best implementing candidate `MyTest.Clone()' is not public
// Line: 6

using System;

public class MySubClass : MyTest, ICloneable
{
}

public class MyTest
{
	internal object Clone ()
	{
		return MemberwiseClone ();
	}
}
