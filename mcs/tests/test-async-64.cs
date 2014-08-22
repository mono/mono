using System;
using System.Threading.Tasks;

class X
{
	public static void Main ()
	{
		Invoke (async delegate {
			await Task.Yield ();
			return 1;
		});
	}

	static T Invoke<T> (Func<Task<T>> m)
	{
		return default (T);
	}
}