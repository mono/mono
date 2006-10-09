namespace Test
{
	public delegate void Handler<TA> (TA sender)
		where TA: A<TA>;

	public class A<TA>
		where TA: A<TA>
	{ }
}

class X
{
	static void Main ()
	{ }
}
