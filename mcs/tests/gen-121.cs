public class B<T>
{
	public int Add (T obj)
	{
		return -1;
	}

	public void AddRange (object o)
	{
		T obj = (T) o;
		Add (obj);
	}
}

public interface IA
{
}

public class A : IA
{
}

public class Test
{
	public static void Main ()
	{
		B<IA> aux = new B<IA> ();
		aux.AddRange (new A ());
	}		
}
