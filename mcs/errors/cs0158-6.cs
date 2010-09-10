// CS0158: The label `a' shadows another label by the same name in a contained scope
// Line: 11

class Foo
{
	static void Main ()
	{
		int i = 1;
		goto a;
		if (i == 9) {
			a:
			return;
		}
a:
		return;
	}
}
