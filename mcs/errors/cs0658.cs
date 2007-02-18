// CS0658: `blah' is invalid attribute target. All attributes in this attribute section will be ignored
// Line : 9
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
