public interface IExtensible<T>
{
        void AddAll<U> (U u)
		where U : T;
}

public class ArrayList<T> : IExtensible<T>
{
        void IExtensible<T>.AddAll<U> (U u)
        {
		InsertAll (u);
        }

	void InsertAll (T t)
	{ }
}

class X
{
	static void Main ()
	{ }
}
