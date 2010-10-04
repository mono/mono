using System;

public struct MyStruct
{
	public static int operator !=(MyStruct? a, string b)
	{
		return -1;
	}
	
	public static int operator ==(MyStruct? a, string b)
	{
		return 1;
	}
	
	public static int operator !=(string a, MyStruct? b)
	{
		return -2;
	}
	
	public static int operator ==(string a, MyStruct? b)
	{
		return 2;
	}
}

public class Test
{
	public static int Main()
	{
		MyStruct? ms = new MyStruct ();
		int v;
		v = ms == "a";
		if (v != 1)
			return 1;
		
		v = "b" != ms;
		if (v != -2)
			return 2;
		
		return 0;
	}
}