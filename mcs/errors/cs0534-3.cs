// CS0534: `MyTestExtended' does not implement inherited abstract member `MyTestAbstract.GetName()'
// Line: 6
// Compiler options: -r:CS0534-3-lib.dll

using System;
public class MyTestExtended : MyTestAbstract
{
	public MyTestExtended() : base()
	{
	}

	public static void Main(string[] args)
	{
		Console.WriteLine("Calling PrintName");
		MyTestExtended test = new MyTestExtended();
		test.PrintName();
		Console.WriteLine("Out of PrintName");
	}
	
}
