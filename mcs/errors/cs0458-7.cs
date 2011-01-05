// CS0472: The result of the expression is always `null' of type `MyEnum?'
// Line: 17
// Compiler options: -warnaserror -warn:2

using System;

enum MyEnum
{
	Value_1
}

class C
{
	public static void Main ()
	{
		var d = MyEnum.Value_1;
		var x = d & null;
	}
}
