using System;

public class TestClass : IDisposable
{
	public static void Main()
	{
		TestClass test = new TestClass();
		test.MyMethod();
	}

	public void Dispose()
	{
			
	}

	public void MyMethod()
	{
		byte[] buffer = new byte[1500];

		using(TestClass test = new TestClass())
		{
			while(true)
			{
			}
		}
	}
}
