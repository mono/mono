// Compiler options: -r:test-417-lib.dll

using System;
using blah;

namespace blah2
{

public class MyClass
{
    public event MyFunnyDelegate DoSomething;

    public void DoSomethingFunny()
    {
        if (DoSomething != null) DoSomething(this, "hello there", "my friend");
    }

	public static void Main(string[] args)
	{
		MyClass mc = new MyClass();
		mc.DoSomethingFunny();

	}
}

}
