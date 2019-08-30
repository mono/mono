using System;
using System.Runtime.InteropServices;
using System.Linq;

public class TestQueryableOrderBy
{
	public static void Main ()
	{
		Test ();
	}

	class Pet
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}

	static void Test ()
	{
		Pet[] pets = { new Pet { Name="Barley", Age=8 },
					   new Pet { Name="Boots", Age=4 },
					   new Pet { Name="Whiskers", Age=1 }
		};

		var query = pets.AsQueryable ().OrderByDescending (pet => pet.Age);

		foreach (Pet pet in query)
			Console.WriteLine("{0} - {1}", pet.Name, pet.Age);
	}
}
