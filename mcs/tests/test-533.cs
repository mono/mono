using System;
class Test {
	const bool bt = (false == false) & (false != true) & (true != false) & (true == true);
	const bool bf = (false != false) | (false == true) | (true == false) | (true != true);

	static void True  (bool b) { False (!b); }
	static void False (bool b) { if (b) throw new System.Exception (); }
	public static void Main ()
	{
		True  (false == false);
		False (false == true);
		False (true  == false);
		True  (true  == true);

		False (false != false);
		True  (false != true);
		True  (true  != false);
		False (true  != true);

		True  (bt);
		False (bf);
	}
}
