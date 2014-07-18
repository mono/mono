// CS0591: Invalid value for argument to `System.Runtime.CompilerServices.MethodImplAttribute' attribute
// Line: 8

using System.Runtime.CompilerServices;

class Program
{
	[MethodImpl((MethodImplOptions)255)]
	void Foo()
	{
	}
}