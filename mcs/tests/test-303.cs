using System;

class A
{
	class C : IDisposable 
	{
		void IDisposable.Dispose () { throw new Exception ("'using' and 'new' didn't resolve C as A+B+C"); }
	}

	public class B
	{
		class C : IDisposable 
		{
			void IDisposable.Dispose () { }
		}

		public B () {
			using (C c = new C ()) {
			}
		}
	}

	public static void Main()
	{
		object o = new A.B();
	}
}
