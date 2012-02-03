// CS0128: A local variable named `res' is already defined in this scope
// Line: 13

class C
{
	static void Foo (int arg)
	{
		switch (arg) {
			case 1:
				int res = 1;
				break;
			case 2:
				int res = 2;
				break;
		}
	}
}
