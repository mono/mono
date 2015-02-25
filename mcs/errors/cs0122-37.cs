// CS0122: `Test.Method' is inaccessible due to its protection level
// Line: 17

public class Test
{
	protected void Method ()
	{
	}

	private void Method (int i)
	{
	}
}

public class C
{
	string str = nameof (Test.Method);
}