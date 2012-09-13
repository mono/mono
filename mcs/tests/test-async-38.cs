using System;
using System.Threading.Tasks;

class C
{
	void Test ()
	{
		Func<Task<int>> f = async () => {
			await GetResultsAsync (null);
			return 2;
		};

		f ();
	}

	Task<int> GetResultsAsync (object arg)
	{
		return null;
	}

	public static void Main ()
	{
		new C ().Test ();
	}
}
