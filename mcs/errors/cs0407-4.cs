// CS0407: A method or delegate `TestDelegateA MainClass.Method(bool)' return type does not match delegate `int TestDelegateA(bool)' return type
// Line: 12

delegate int TestDelegateA (bool b);

public class MainClass
{
	static TestDelegateA Method (bool b)
	{
		return Method;
	}
}

