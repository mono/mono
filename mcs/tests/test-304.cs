using System;
using System.Collections;

using C = A.D;


class A
{
	internal class D { }
	public class B
	{
		class C { }

		public B() {
			string error = "";

			if (typeof (C) != typeof (A.B.C))
				error += " 'typeof' keyword,";

			object o0 = new C ();
			if (o0.GetType() != typeof (A.B.C))
				error += " 'new' keyword,";

			C o1 = new C ();
			if (o1.GetType () != typeof (A.B.C))
				error += " type declaration,";

			object o2 = new A.B.C ();
			if (!(o2 is C))
				error += " 'is' keyword,";

			object o3 = o2 as C;
			if (o3 == null)
				error += " 'as' keyword,";

			try {
				object o4 = (C) o2;
			}
			catch {
				error += " type cast,";
			}

			try {
				object o5 = (C) (o2);
			}
			catch {
				error += " invocation-or-cast,";
			}

			object o6 = new C [1];

			if (o6.GetType ().GetElementType () != typeof (A.B.C))
				error += " array creation,";

			if (typeof (C []).GetElementType () != typeof (A.B.C))
				error += " composed cast (array),";

			ArrayList a = new ArrayList ();
			a.Add (new A.B.C ());

			try {
				foreach (C c in a)
				{ 
				}
			}
			catch {
				error += " 'foreach' statement,";
			}

			if (error.Length != 0)
				throw new Exception ("The following couldn't resolve C as A+B+C:" + error);
		}
	}

	public static void Main()
	{
		object o = new A.B();
	}
}
