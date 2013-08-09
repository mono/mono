// CS0647: Error during emitting `System.Runtime.CompilerServices.MethodImplAttribute' attribute. The reason is `Incorrect argument value'
// Line: 8

using System.Runtime.CompilerServices;

class Test
{
	[MethodImplAttribute(445)]
	public void test ()
	{
	}
}