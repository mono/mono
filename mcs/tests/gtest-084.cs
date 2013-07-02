namespace HasherBuilder
{
	public class ByPrototype<S>
	{
		public static IHasher<S> Examine()
		{
			return null;
		}
	}
}

public interface IHasher<T>
{
}

public class ArrayList<U>
{
	public IHasher<U> GetHasher ()
	{
		return HasherBuilder.ByPrototype<U>.Examine();
	}
}

class X
{
	public static void Main ()
	{ }
}

