// CS0309: The type `T' must be convertible to `System.IDisposable' in order to use it as parameter `T' in the generic type or method `IB<K,T>'
// Line: 20

public interface IA<K> 
	where K : System.IComparable, System.IComparable<K>
{
}

public class A<K> : IA<K> 
	where K : System.IComparable, System.IComparable<K>
{
}

public interface IB<K,T> 
	where T : System.IDisposable
{
} 

public class B<K,T> : IB<K,T> 
	where T : B<K,T>.Element, new() 
	where K : System.IComparable, System.IComparable<K>
{
	public abstract class Element : A<K>
	{
	}
}




