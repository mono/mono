using System;
using System.Threading;
using System.Threading.Tasks;

public class AmbiguousGeneric
{
	public async void NestedVoidTestSuccess ()
	{
		await Run2 (async () => await ReturnOne ());
	}

	static Task<int> ReturnOne ()
	{
		return Task.Run (() => 1);
	}

	Task Run2 (Func<Task> arg)
	{
		return null;
	}

	Task Run2<T> (Func<T> arg)
	{
		return null;
	}

	public static void Main ()
	{
	}
}