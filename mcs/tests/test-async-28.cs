using System.Linq;
using System.Threading.Tasks;

class C
{
	public static async Task Test ()
	{
		Task<int[]> d = Task.FromResult (new[] { 1, 4, 5 });
		var r = from x in await d select x;
		var res = r.ToList ();
	}

	public static void Main ()
	{
		Test ().Wait ();
	}
}
