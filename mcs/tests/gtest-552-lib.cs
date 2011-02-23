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
