//
// This test excercises the simple name lookups on
// unfinished enumerations.
//

public enum FL { 
	EMPTY = 0, 
	USHIFT = 11, 
	USER0 = (1<<(USHIFT+0)),
}

class X {

	static int Main ()
	{
		return 0;
	}
}
