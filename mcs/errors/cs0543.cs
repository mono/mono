// CS0543: The enumerator value `Blah.MyEnum.Bar' is outside the range of enumerator underlying type `byte'
// Line : 8

public class Blah
{
	public enum MyEnum : byte {
		Foo = 255,
		Bar
	}
}
