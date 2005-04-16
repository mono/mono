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
