// CS0115: `MyTestExtended.GetName()' is marked as an override but no suitable method found to override
// Line: 12
// Compiler options: -r:CS0534-4-lib.dll

using System;
public class MyTestExtended : MyTestAbstract
{
	public MyTestExtended() : base()
	{
	}

	protected override string GetName() { return "foo"; }
	public static void Main(string[] args)
	{
		Console.WriteLine("Calling PrintName");
		MyTestExtended test = new MyTestExtended();
		test.PrintName();
		Console.WriteLine("Out of PrintName");
	}
	
}
