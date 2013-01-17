using System;

class G<T>
{
}

class C
{
	static G<TResult> M<T, TResult>(G<T>[] arg, Func<G<T>[], TResult> func)
	{
		return null;
	}
	
	public static int Main ()
	{
		G<int>[] tasks = new G<int>[0];
		G<G<int>[]> r = M(tasks, l => l);
		return 0;
	}
}
