// CS0619-48: `A.Prop' is obsolete: `!!!'
// Line: 13

class A: System.Attribute
{
	[System.Obsolete("!!!", true)]
	public string Prop {
		set { }
		get { return ""; }
	}
}

[A(Prop="System.String.Empty")]
class Obsolete {
}
