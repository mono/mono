class Continuation<R,A>
{
	public static Continuation<R,A> CallCC<B> (object f)
	{
		return null;
	}
}

class Driver
{
	static Continuation<B,A> myTry<A,B> (B f, A x)
	{
		return Continuation<B,A>.CallCC <object> (null);
	}

	public static void Main()
	{
		myTry <int,int> (3, 7);
	}
}


