using System;

class Cons<T,U>
{
	public T car;
	public U cdr;

	public Cons (T x, U y)
	{
		car = x; cdr = y;
	}

	public override String ToString ()
	{
		return "(" + car + '.' + cdr + ')';
	}
}

class List<A> : Cons<A, List<A>>
{
	public List (A value)
		: base(value, null)
	{ }

	public List (A value, List<A> next)
		: base(value, next)
	{ }

	public void zip<B> (List<B> other)
	{
		cdr.zip (other.cdr);
	}
}

abstract class Test
{
	public static void Main (String[] args)
	{
		List<int> list = new List<Int32> (3);
		Console.WriteLine (list);
	}
}
