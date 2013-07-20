namespace A
{
	public partial class B<T>
	{
		public partial class C
		{
			public class A { }
		}
	}
}

namespace A
{
	public abstract partial class B<T> where T : B<T>.C
	{
	}
}

namespace A
{
	public partial class B<T>
	{
		public partial class C : I
		{
		}
	}
}

namespace A
{
	public interface Ibase
	{
	}

	public partial class B<T>
	{
		public interface I : Ibase
		{
		}
	}
}

namespace A
{
	class Bar : B<Bar>.C
	{
	}

	public class Test
	{
		public static void Main ()
		{
			Ibase b = new Bar ();
			System.Console.WriteLine (b != null);
		}
	}
}
