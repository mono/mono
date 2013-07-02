class Fun<A,B> {}

class List<T> {
  public List<T2> Map<T2> (Fun<T,T2> x)
  {
    return new List<T2>();
  }

  public void foo<T2> ()
  {
    (new List<T2> ()).Map<T> (new Fun<T2,T> ());
  }
}


class X
{
	public static void Main ()
	{ }
}
