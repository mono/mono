// CS0619-47: `A.Field' is obsolete: `!!!'
// Line: 11

class A: System.Attribute
{
	[System.Obsolete("!!!", true)]
	public int Field;
}

class Obsolete {
	[A(Field=2)]
	public int Foo;
}
