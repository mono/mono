using System.Linq;

//
// This is a lambda test for situation when parent is infering return types and child not
//

public class Product
{
	public int CategoryID;
	public decimal UnitPrice;
}

class MainClass
{
	public static void Main ()
	{
		Product[] products = new[] {
			new Product { CategoryID = 1, UnitPrice = 1m }
		};

		var categories = from p in products
						 group p by p.CategoryID into g
						 select new {
							 g,
							 ExpensiveProducts = from p2 in g
												 where (p2.UnitPrice > g.Average (p3 => p3.UnitPrice))
												 select p2
						 };
	}
}
