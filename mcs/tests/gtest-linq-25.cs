using System;
using System.Linq;

class A
{
	public string Header { get { return null; } }
}

class B
{
	public string Name { get { return null; } }
}

class C
{
	public static int Main ()
	{
		Test (delegate () {
			var a = new A[0];
			var b = new B[0];

			if (a != null) {
				var r = from c in new A[0]
						from p in new B[0]
						where c.Header == p.Name && p.Name == typeof (string).ToString ()
						select new { C = c, P = p };
			}
		});
		
		return 0;
	}

	static void Test (Action a)
	{
		a ();
	}
}
