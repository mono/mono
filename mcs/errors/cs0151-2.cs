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
