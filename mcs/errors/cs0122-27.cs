// CS0122: `G.GG' is inaccessible due to its protection level
// Line: 14

class G
{
	private class GG
	{
		public class F { }
	}
}

class X
{
	G.GG.F foo;
}

