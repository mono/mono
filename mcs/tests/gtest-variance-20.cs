[System.AttributeUsage (System.AttributeTargets.All)]
class DocAttribute : System.Attribute
{
	public DocAttribute (string name)
	{
	}
}

delegate TR Func<[Doc("b")] in T1, [Doc("a")] out TR>(T1 a);

class Test {
	public static void Main ()
	{
	}
}