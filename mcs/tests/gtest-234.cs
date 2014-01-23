public interface IFoo { }
public interface IFoo<T> : IFoo { }

public class Test
{
        public IFoo GetFoo () { return GetFooGeneric<object> (); }

        public IFoo<T> GetFooGeneric<T> () { return default (IFoo<T>); }

	public static void Main ()
	{
		Test test = new Test ();
		test.GetFoo ();
	}
}
