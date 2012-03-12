// CS1995: The `await' operator may only be used in a query expression within the first collection expression of the initial `from' clause or within the collection expression of a `join' clause
// Line: 12

using System.Linq;
using System.Threading.Tasks;

class C
{
	public static async void Test ()
	{
		Task<int>[] d = null;
		var r = from x in d select await x;
	}
}
