// CS0136: A local variable named `res' cannot be declared in this scope because it would give a different meaning to `res', which is already used in a `child' scope to denote something else
// Line: 15

class C
{
	public void Foo (int i, int v)
	{
		switch (i) {
			case 1:
				if (v > 0) {
					int res = 1;
				}
				break;
			case 2:
				int res = 2;
				break;
		}
	}
}
