using System.Reflection;
using System;
using A;

namespace A {
	interface B {
		void METHOD ();
	}
}


class D : B {
	void B.METHOD ()
	{
	}

	public static int Main ()
	{
		MethodInfo [] mi = typeof (D).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo m = null;
		
		foreach (MethodInfo j in mi){
			if (j.Name.IndexOf ("METHOD") != -1){
				m = j;
				break;
			}
		}
		if (m == null)
			return 1;

		if (m.Name != "A.B.METHOD"){
			Console.WriteLine ("Incorrect method name, expecting: {0} got {1}",
					   "A.B.METHOD", m.Name);
			return 2;
		}

		return 0;
	}
}
