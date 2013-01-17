//
// User casts basically would produce more than one match on 
// integral types, since the implicit conversion to int is
// also an implicit conversion to long.   This tests that
// we do not bail too early on the switch statement with its
// implicit conversion.  

class Y {
	byte b;
	
	public static implicit operator int (Y i)
	{
		return i.b;
	}

	public Y (byte b)
	{
		this.b = b;
	}			
}

class X {
	public static void Main ()
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
