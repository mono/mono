public class Test
{
	private static int DoTest (string type, string expected, string actual, int failcode)
	{
		if (!actual.Equals (expected)) {
			System.Console.WriteLine ("Bad {0}: Expected {1}, Was {2}",
							   type, expected, actual);
			return failcode;
		}
		return 0;
	}

	public static int Main ()
	{
		int failure = 0;
		Concrete val = new Concrete ();

		failure |= DoTest ("A", "A", ((A) val).Spec, 0x01);
		failure |= DoTest ("B", "B", ((B) val).Spec, 0x02);
		failure |= DoTest ("C", "B", ((C) val).Spec, 0x04);
		failure |= DoTest ("Concrete", "Concrete", val.Spec, 0x08);

		return failure;
	}
}

interface A
{
	string Spec { get; }
}

interface B : A
{
	new string Spec { get; }
}

interface C : B
{
}

class Concrete : C
{
	string A.Spec { get { return "A"; } }
	string B.Spec { get { return "B"; } }
	public string Spec { get { return "Concrete"; } }
}
