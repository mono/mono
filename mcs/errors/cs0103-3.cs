// CS0103: The name `test' does not exist in the current context
// Line: 11

class ClassMain
{
	public static void Main ()
	{
		if (true) {
			const bool test = false;
		}
		test = false;
	}
	
	static bool Test { 
		set {
		}
	}
}

