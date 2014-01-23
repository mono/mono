using System;

namespace test2
{
	public class Test<T, U, V>
		where T : U, new ()
		where U : V, new ()
		where V : IDisposable, new ()
	{

		public void Method ()
		{
			IDisposable t = new T ();
			IDisposable u = new U ();
			IDisposable v = new V ();
		}
	}

	class MainClass : IDisposable
	{
		public void Dispose ()
		{
		}

		public static void Main ()
		{
			new Test<MainClass, MainClass, MainClass> ().Method ();
		}
	}
}
