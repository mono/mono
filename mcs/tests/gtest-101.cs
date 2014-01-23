using System;

public class Test
{
	public static void Main ()
	{
		SimpleStruct <string> s = new SimpleStruct <string> ();
	}
}

public struct SimpleStruct <T>
{
	T data;

	public SimpleStruct (T data)
	{
		this.data = data;
	}
}
