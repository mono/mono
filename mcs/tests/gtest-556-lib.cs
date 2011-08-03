// Compiler options: -t:library

public class A
{
	public class N<T>
	{
		public static N<T> Method ()
		{
			return default (N<T>);
		}
	}
}
