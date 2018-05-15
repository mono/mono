using System;

class TupleDeconstructionDeclaration
{
	public static int Main ()
	{
		(string s, long l) = GetValues ();
		(var vs, var vl) = GetValues ();
		(object o, var vl2) = GetValues ();
		(string ds, _) = GetValues ();

		return 0;
	}

	static (string, long) GetValues ()
	{
		return ("a", 3);
	}
}