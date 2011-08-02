// CS0037: Cannot convert null to `byte' because it is a value type
// Line : 7

public class Blah {

	public enum MyEnum : byte {
		Foo = null,
		Bar
	}

	public static void Main ()
	{
	}
}
