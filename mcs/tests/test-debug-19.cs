using System;
using System.Threading.Tasks;

class C
{
	public static void Main ()
	{
	}
	
	static async void Test_1 ()
	{
		await RunAsync ();
	}

	static Task RunAsync ()
	{
		return Task.Factory.StartNew (
		() =>
		{
		});
	}
	
	static async Task<int> Test_2 ()
	{
		return await RunAsync_2 ();
	}

	static Task<int> RunAsync_2 ()
	{
		return Task.Factory.StartNew (() => 2);
	}
	
	async Task<bool> Test_3 ()
	{
		dynamic d = new C ();
		d.Value = 3;
		d.Value += await Task.Factory.StartNew (() => 2);
		return d.Value == 5;
	}
}
