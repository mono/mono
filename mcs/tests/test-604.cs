using System;
using System.Reflection;

class Program {
	interface Iface1 {
		void IfaceMethod1 ();
	}

	interface Iface2 {
		void IfaceMethod2 ();
	}

	public class ImplementingExplicitInterfacesMembers : Iface1, Iface2 {
		void Iface1.IfaceMethod1 ()
		{
		}

		void Iface2.IfaceMethod2 ()
		{
		}
	}

	public static int Main ()
	{
		object[] o = typeof (ImplementingExplicitInterfacesMembers).GetMethods (BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (MethodInfo mi in o) {
			if (mi.Name.IndexOf ('+') != -1)
				return 1;
			Console.WriteLine (mi.Name);
		}
		
		return 0;
	}
}
