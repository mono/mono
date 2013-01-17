public abstract class NonGenericBase
{
	public abstract int this[int i] { get; }
}

public abstract class GenericBase<T> : NonGenericBase
	where T : GenericBase<T>
{
	T Instance { get { return default (T); } }

	public void Foo ()
	{
		int i = Instance[10];
	}
}

public class EntryPoint
{
	public static void Main () { }
}
