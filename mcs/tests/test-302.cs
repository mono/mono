using System;

class A
{
	class C : Exception { }

	public class B
	{
		class C : Exception { }

		public B () {
			try {
				throw new A.B.C ();
			}
			catch (C e) {
			}
		}
	}

	public static void Main()
	{
		object o = new A.B();
	}
}
