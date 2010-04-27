// CS0122: `Test<float>' is inaccessible due to its protection level
// Line: 8
// Compiler options: -r:GCS0122-2-lib.dll

class X
{
	static void Main ()
	{
		Test<float> test = new Test<float> ();
	}
}

