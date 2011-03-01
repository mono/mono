// Compiler options: -t:library

public class G<T> where T : G<T>.GPD
{
	public interface IGD
	{
	}

	public class GPD : IGD
	{
		public T GT;
		
		public void Foo ()
		{
		}
	}
}

public class H<T>
{
	public class N<U, V> where U : H<T> where V : H<T>.M<V>
	{
	}
	
	public class M<X>
	{
	}
}
