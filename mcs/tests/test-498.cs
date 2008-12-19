using System.Reflection;
using System.Runtime.CompilerServices;

struct C
{
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	public extern C(float value);

	static int Main ()
	{
		MethodImplAttributes iflags = typeof (C).GetConstructors()[0].GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.InternalCall) == 0)
			return 1;

		return 0;
	}
}
