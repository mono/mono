using System;
using System.Threading.Tasks;

class Program
{
	public class C
	{
		public void M ()
		{
			Console.WriteLine ("called");
		}
	}

	public static void F (Action<C> a)
	{
		a (new C ());
	}

	public static void Main ()
	{
		F (async (c) => {
			await Task.Run (() => { });
			c.M ();
		});
	}
}

