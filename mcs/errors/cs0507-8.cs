// CS0507: `Class2.GetString()': cannot change access modifiers when overriding `protected' inherited member `Class1.GetString()'
// Line: 7
// Compiler options: -r:CS0507-8-lib.dll

public sealed class Class2 : Class1
{
	protected internal override string GetString ()
	{
		return "Hello2";
	}
}
