using System;

public class FooAttribute : Attribute {
	public char [] Separator;
}

[Foo (Separator = new char[] {'A'})]
public class Tests {
	public static void Main () {
		FooAttribute foo = (FooAttribute) (typeof (Tests).GetCustomAttributes (false) [0]);
		Console.WriteLine (foo.Separator);
	}
}
