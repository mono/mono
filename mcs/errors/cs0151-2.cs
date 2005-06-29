// cs0151-2.cs: A value of an integral type or string expected for switch
// Line: 12

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
