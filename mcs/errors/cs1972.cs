// CS1972: The indexer base access cannot be dynamically dispatched. Consider casting the dynamic arguments or eliminating the base access
// Line: 18

class A
{
	public int this [int i] {
		get {
			return i;
		}
	}
}

class B : A
{
	public void Test ()
	{
		dynamic d = null;
		var r = base [d];
	}
}
