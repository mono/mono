class C
{
	public static void Main ()
	{
		Create (false);
	}

	static IA Create (bool arg)
	{
		// Verifier issue
		IA runner = arg ? new B2 () : (IA) new B1 ();
		return runner;
	}
}

interface IA
{

}

class B2 : IA
{
}

class B1 : B
{
}

class B : IA
{
}