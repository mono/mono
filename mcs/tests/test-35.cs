//
// This test checks the !x optimization for if/while/for/do
//
class X {

	static bool t = true;
	static bool f = false;
	static int j = 0;
	
	static void a ()
	{
		if (!t)
			j = 1;
	}
	
	static int Main ()
	{
		int ok = 0, error = 0;
		
		if (!f)
			ok = 1;
		else
			error++;

		if (f)
			error++;
		else
			ok |= 2;

		if (t)
			ok |= 4;
		else
			error++;

		if (!t)
			error++;
		else
			ok |= 8;

		if (!(t && f == false))
			error++;
		else
			ok |= 16;

		int i = 0;
		do {
			i++;
		} while (!(i > 5));
		if (i != 6)
			error ++;
		else
			ok |= 32;
		
		do {
			i++;
		} while (!t);
		
		System.Console.WriteLine ("Ok=" + ok + " Errors=" + error);
		return ((ok == 63) && (error == 0)) ? 0 : 1;
	}
}
