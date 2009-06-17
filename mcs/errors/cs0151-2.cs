// CS0151: A switch expression of type `Y' cannot be converted to an integral type, bool, char, string, enum or nullable type
// Line: 28

class Y {
	byte b;
	
	public static implicit operator int (Y i)
	{
		return i.b;
	}

	public static implicit operator byte (Y i)
	{
		return i.b;
	}

	public Y (byte b)
	{
		this.b = b;
	}			
}

class X {
	static void Main ()
	{
		Y y = new Y (1);

		switch (y){
		case 0:
			break;
		case 1:
			break;
		}

		int a = y;
	}
}
