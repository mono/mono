// cs0151.cs: A value of an integral type or string expected for switch
// Line: 12
class Y {
	byte b;
}

class X {
	static void Main ()
	{
		Y y = new Y ();

		switch (y){
		case 0:
			break;
		case 1:
			break;
		}
	}
}
