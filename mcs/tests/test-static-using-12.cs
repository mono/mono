namespace A.B
{
	public static class G<T>
	{
		public class DD
		{
		}

		public static object Dock () => null;
	}
}

namespace N2
{
	using static A.B.G<int>;

	class M : DD
	{
		public static void Main ()
		{
			Dock ();
		}
	}
}
