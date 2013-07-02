public interface IFoo<S>
{ }

public class ArrayList<T>
{
        public virtual int InsertAll (IFoo<T> foo)
        {
                return 0;
        }

        public virtual int InsertAll<U> (IFoo<U> foo)
                where U : T
        {
                return 1;
        }

        public virtual int AddAll (IFoo<T> foo)
        {
                return InsertAll (foo);
        }
}

class X
{
	public static void Main ()
	{ }
}
