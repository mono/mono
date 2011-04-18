// CS0619-49: `A.A(string[])' is obsolete: `!!!'
// Line: 12

class A: System.Attribute
{
	[System.Obsolete("!!!", true)]
	public A (string[] s)
	{
	}
}

[A(new string[0])]
class Obsolete {
}
