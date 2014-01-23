// CS0737: `MySubClass' does not implement interface member `I.Foo.set' and the best implementing candidate `MyTest.Foo.set' is not public
// Line: 6

using System;

interface I
{
	int Foo { get; set; }
}

public class MySubClass : MyTest, I
{
}

public class MyTest
{
	public int Foo
	{
		get { return 1; }
		protected set { }
	}
}
