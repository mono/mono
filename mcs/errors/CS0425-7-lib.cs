// CS0425: 
// Line: 

public interface IA
{
	void Foo<U> ();
}

public class CA
{
	public void Foo<T> () where T : class
	{
	}
}
