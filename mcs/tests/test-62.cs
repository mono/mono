// 
// This test just makes sure that we can typecast to
// array types, as this was introduced later into the
// grammar.
//

class X {

	X [] GetX ()
	{
		return (X []) null;
	}

	int [] getInt ()
	{
		return (int []) null;
	}

	int [,,] getMoreInt ()
	{
		return (int [,,]) null;
	}

	public static int Main ()
	{
		return 0;
	}
}
