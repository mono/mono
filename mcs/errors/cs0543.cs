// cs0543.cs: The enumerator value `Blah.MyEnum.Bar' is too large to fit in its type `byte'
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
