using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace preservesig_test
{
	class Class1
	{
		public static int Main(string[] args)
		{
			MethodInfo dofoo = typeof(TestClass).GetMethod("DoFoo");
			if ((dofoo.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) == 0)
				return 1;
			
			dofoo = typeof(TestClass).GetProperty("Foo").GetGetMethod ();
			if ((dofoo.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) == 0)
				return 1;

			dofoo = typeof(TestClass).GetEvent("e").GetAddMethod (true);
			if ((dofoo.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) == 0)
				return 1;
			
			Console.WriteLine("Has PreserveSig");
			return 0;
		}
	}

	public class TestClass
	{
		public delegate void D ();
		
		[method:PreserveSig]
		public event D e;
		
		[PreserveSig()]
		public int DoFoo()
		{
			return 0;
		}
		
		public int Foo {
			[PreserveSig]
			get {
				return 2;
			}
		}
	}
}
