// cs0658.cs : Invalid attribute location "blah"
// Line : 8
// Compiler options: -warnaserror -warn:1

public class MyClass {

	delegate int MyDelegate (int i, int j);
	
	[blah:Help("blah")]
	public static MyClass operator/ (MyClass i, MyClass j)
	{
	
	}

	public static implicit operator MyClass (Object o)
	{

	}
}
