// Compiler options: -warnaserror -warn:4

using System;
	
public class Foo
{
	[Obsolete]
	public void Something ()
	{
	}
}
	
public class Bar : Foo {
	public new void Something ()
	{
	}
	
	public static void Main ()
	{
	}
}
