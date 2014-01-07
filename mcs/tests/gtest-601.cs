using System;

public class TestProgram
{
	public static void Main ()
	{
		IMyStruct myStruct = null;
		MyStruct? structValue;

		structValue = (MyStruct?)myStruct;
	}
}

public struct MyStruct : IMyStruct
{
}

public interface IMyStruct
{
}
