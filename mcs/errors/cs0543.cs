// cs1008.cs : Enumerator value for 'Bar' is too large to fit in its type 
// Line : 9

public class Blah {

	public enum MyEnum : byte {
		Foo = 255,
		Bar
	}

	public static void Main ()
	{
	}
}
