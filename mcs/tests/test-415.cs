// Compiler options: -r:test-415-lib.dll

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
