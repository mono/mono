// cs0647.cs: Error emitting 'MethodImplAttribute' attribute because 'Incorrect argument value'
// Line: 8

using System.Runtime.CompilerServices;

class Test
{
	[MethodImplAttribute(444)]
	public void test ()
	{
	}
}