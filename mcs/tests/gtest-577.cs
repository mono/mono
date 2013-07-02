using System;

static class Program
{
	public interface I1
	{
		string Id { get; }
	}

	public class BaseClass
	{
		public int Id {
			get {
				return 4;
			}
		}
	}

	public class Derived : BaseClass, I1
	{
		public new string Id {
			get {
				return "aa";
			}
		}
	}

	static void Generic<T> (T item) where T : BaseClass, I1
	{
		if (item.Id != 4)
			throw new Exception ("Doom!");
	}

	static void Main ()
	{
		Generic (new Derived ());
	}
}
