using System;
using System.Threading.Tasks;

class X
{
	public static void Main ()
	{
		new X ().Test ();
	}

	void Test ()
	{
		object v1 = null;

		Action a = () =>
		{
			if (v1 == null)
			{
				object v2 = null;

				Action a2 = () =>
				{
					Console.WriteLine (v2);
				};
				
				Action a3 = async () =>
				{
					// This scope needs to access to Scope which can do ldftn on instance method
					{
					Func<Task> a4 = async () =>
					{
						await Foo ();
					};
					}

					await Task.Yield ();
				};

				a3 ();
			}
		};

		a ();
	}

	async Task Foo ()
	{
		await Task.FromResult (1);
	}

}