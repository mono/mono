public class Parent
{
	public Parent () { }
	private Collide Collide;
}

public class Child : Parent
{
	public class Nested
	{
		public readonly Collide Test;

		public Nested ()
		{
			Test = Collide.Die;
		}
	}
}

public class Collide
{
	public Collide (int a)
	{
		this.A = a;
	}

	public readonly int A;
	public static readonly Collide Die = new Collide (5);

	public static int Main ()
	{
		Child.Nested nested = new Child.Nested ();
		if (nested.Test.A != 5)
			return 1;
		return 0;
	}
}
