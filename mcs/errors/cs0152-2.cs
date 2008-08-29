// CS0152: The label `case 2:' already occurs in this switch statement
// Line: 19

using System;

enum E
{
	Foo = 2
}

class X
{
	void Foo (E e)
	{
		switch (e)
		{
			case E.Foo:
				break;
			case E.Foo:
				break;
		}
	}
}
