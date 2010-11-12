class C<T>
{
	public interface IA
	{
		void MA (T arg);
	}
	
	public interface IB : IA
	{
		void MB (T arg);
	}
}

class D : C<int>
{
	public class Impl : IB
	{
		public void MA (int arg)
		{
		}
		
		public void MB (int arg)
		{
		}
	}
}

class Test
{
	public static void Main ()
	{
		C<int>.IB arg = new D.Impl ();
		arg.MA (1);
		arg.MB (1);	
	}
}