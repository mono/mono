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

class D : IInferior
{
	public int TargetIntegerSize {
		get { return 5; }
	}

	int Hello (IInferior inferior)
	{
		return inferior.TargetIntegerSize;
	}

	static int Main ()
	{
		D d = new D ();

		if (d.Hello (d) != 5)
			return 1;

		return 0;
	}
}
