// Compiler options: -unsafe

public unsafe class X
{
	int field;
	int* ufield;

	public static void Main ()
	{
		int i = 5;
		ref int j = ref i;

		var x = new X ();
		ref var v = ref x.TestMethod ();
	}

	ref int TestMethod ()
	{
		return ref field;
	}

	ref int TestProperty {
		get {
			return ref field;
		}
	}

	ref int this [long arg] {
		get {
			return ref field;
		}
	}

	unsafe ref int* Foo ()
	{
		return ref ufield;
	}
}