// CS0507: `Class2.GetString()': cannot change access modifiers when overriding `protected internal' inherited member `Class1.GetString()'
// Line: 7
// Compiler options: -r:CS0507-7-lib.dll

public sealed class Class2 : Class1
{
	protected override string GetString ()
	{
		return "Hello2";
	}
}
