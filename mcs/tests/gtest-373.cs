using System;

namespace MonoBug
{
	class MainClass
	{
		public static void Main ()
		{
			GenericType<bool> g = new GenericType<bool> (true);
			if (g)
				Console.WriteLine ("true");
		}
	}

	public class GenericType<T>
	{
		private T value;

		public GenericType (T value)
		{
			this.value = value;
		}

		public static implicit operator T (GenericType<T> o)
		{
			return o.value;
		}
	}
}