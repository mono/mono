using System;

interface ITargetInfo
{
	int TargetIntegerSize {
		get;
	}
}

interface ITargetMemoryAccess : ITargetInfo
{
}

interface IInferior : ITargetMemoryAccess
{
}

interface ITest
{
	int this [int index] {
		get;
	}
}

class Test : ITest
{
	public int this [int index] {
		get { return 5; }
	}

	int ITest.this [int index] {
		get { return 8; }
	}
}

class D : IInferior
{
	public int TargetIntegerSize {
		get { return 5; }
	}

	int Hello (IInferior inferior)
	{
		return inferior.TargetIntegerSize;
	}

	public static int Main ()
	{
		D d = new D ();

		if (d.Hello (d) != 5)
			return 1;

		Test test = new Test ();
		ITest itest = test;

		if (test [0] != 5)
			return 2;
		if (itest [0] != 8)
			return 3;

		return 0;
	}
}
