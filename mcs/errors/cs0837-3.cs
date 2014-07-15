// CS0837: The `is' operator cannot be applied to a lambda expression, anonymous method, or method group
// Line: 10

using System;
 
class Test
{
	static void Main ()
	{
		var res = Main is object;
	}
}