[System.AttributeUsage (System.AttributeTargets.All)]
class DocAttribute : System.Attribute
{
	public DocAttribute (string name)
	{
	}
}

delegate TR Func<[Doc("r")] T1, out TR>(T1 a);

class Test {
	public static void Main ()
	{
	}
}