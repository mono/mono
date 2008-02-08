namespace N2
{
	public class X<T>
	{
		private class A<T>
		{
			private class B<T>
			{
				public class C<T>
				{
				}
			
				internal C<T> foo;
			}
		}
	}
	
	class C
	{
		public static void Main ()
		{
		}
	}
}
