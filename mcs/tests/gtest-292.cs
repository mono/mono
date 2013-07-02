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
	public static void Main ()
	{ }
}
