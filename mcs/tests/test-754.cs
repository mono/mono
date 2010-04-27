namespace Bug
{
	public delegate void D ();

	public abstract class A
	{
		public abstract event D E;
	}

	public sealed class B : A
	{
		public override event D E
		{
			add { }
			remove { }
		}
	}

	class M
	{
		public static void Main ()
		{
		}
	}
}
