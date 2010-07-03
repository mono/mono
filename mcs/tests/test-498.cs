using System.Reflection;
using System.Runtime.CompilerServices;

struct A
{
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	public extern A (float value);
	
	public extern int Foo {
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		get;
	}
}

struct C
{
	static int Main ()
	{
		MethodImplAttributes iflags = typeof (A).GetConstructors()[0].GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.InternalCall) == 0)
			return 1;

		iflags = typeof (A).GetProperties ()[0].GetGetMethod ().GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.InternalCall) == 0)
			return 2;
		
		return 0;
	}
}
