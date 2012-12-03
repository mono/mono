public interface IExtensible<T>
{
	void AddAll<U> (U item)
		where U : T;
}

public class ArrayList<T> : IExtensible<T>
{
        void IExtensible<T>.AddAll<U> (U item)
        { }
}

class X
{
	public static void Main ()
	{ }
}
