using System;

class A
{
	class C { }

	public class B
	{
		class C { }

		public B() {
			string error = "";

			object o1 = new C ();
			if (o1.GetType() != typeof (A.B.C))
				error += " 'new' keyword,";

			if (typeof (C) != typeof (A.B.C))
				error += " 'typeof' keyword,";

			object o2 = new A.B.C ();
			if (!(o2 is C))
				error += " 'is' keyword,";

			object o3 = o2 as C;
			if (o3 == null)
				error += " 'as' keyword,";

			try {
				object o4 = (C) o2;
			}
			catch (Exception e) {
				error += " type cast";
			}

			try {
				object o4 = (C) (o2);
			}
			catch (Exception e) {
				error += " invocation-or-cast";
			}

			if (error.Length != 0)
				throw new Exception ("The following couldn't resolve C as A+B+C:" + 
						     error);


		}
	}

	public static void Main()
	{
		object o = new A.B();
	}
}
