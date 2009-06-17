// CS0151: A switch expression of type `Y' cannot be converted to an integral type, bool, char, string, enum or nullable type
// Line: 13

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
