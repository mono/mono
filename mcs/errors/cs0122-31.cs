// CS0122: `Data.Count' is inaccessible due to its protection level
// Line: 13

class Data
{
	int Count;
}

public class Test
{
	static void Main ()
	{
		var c = new Data { Count = 10 };
	}
}
