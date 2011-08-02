// CS0019: Operator `&' cannot be applied to operands of type `System.Reflection.MethodImplAttributes' and `System.Runtime.CompilerServices.MethodImplOptions'
// Line : 13

//
// From bug #59864 
//
using System.Reflection;
using System.Runtime.CompilerServices;

public class Foo {

	public static void Main ()
	{
		MethodImplAttributes methodImplAttributes = 0;
            
                if ((methodImplAttributes & MethodImplOptions.Synchronized) == 0) {
                }
	}
}
