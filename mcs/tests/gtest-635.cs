public interface I<T>
{
	T Clone();
	T1 Clone<T1>() where T1 : T;
}

public interface I2 : I<I2>
{
}

public class TestClass : I2
{
	public I2 Clone ()
	{
		return null;
	}

	public T1 Clone<T1> () where T1 : I2
	{
		return (T1) Clone();
	}

	public static void Main () 
	{
		new TestClass ();
	}
}