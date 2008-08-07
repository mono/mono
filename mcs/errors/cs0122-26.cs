// CS0122: `C.I' is inaccessible due to its protection level
// Line: 11

class C
{
	protected interface I
	{
	}
}

class A : C.I
{
}
