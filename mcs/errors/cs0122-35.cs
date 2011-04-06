// CS0122: `Test<A>' is inaccessible due to its protection level
// Line: 8
// Compiler options: -r:CS0122-35-lib.dll

class X
{
	static void Main ()
	{
		Test<float> test = new Test<float> ();
	}
}

