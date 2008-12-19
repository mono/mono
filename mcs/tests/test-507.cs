using System;
using System.Reflection;

namespace NewslotVirtualFinal
{
	internal interface INewslotVirtualFinal
	{
		void SomeMethod();
		void SomeMethod2();
	}

	internal class NewslotVirtualFinal : INewslotVirtualFinal
	{
		private NewslotVirtualFinal()
		{
		}

		public void SomeMethod()
		{
		}

		public virtual void SomeMethod2()
		{
		}
	}
	
	class C
	{
		public static int Main ()
		{
			Type t = typeof (NewslotVirtualFinal);
			MethodInfo mi = t.GetMethod ("SomeMethod");
			if (mi.Attributes != (MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask))
				return 1;
			
			mi = t.GetMethod ("SomeMethod2");
			if (mi.Attributes != (MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask))
				return 2;
			
			Console.WriteLine ("OK");
			return 0;
		}
	}
}