// CS0266: Cannot implicitly convert type `I' to `C'. An explicit conversion exists (are you missing a cast?)
// Line: 16

interface I
{
}

struct C : I
{
}

class X
{
	static void Main (string[] args)
	{
		C c = default (I);
	}
}
