using System;

public class Identifier
{
	public Identifier () { }

	public static bool operator == (Identifier id1, Identifier id2)
	{
		return true;
	}
	public static bool operator != (Identifier id1, Identifier id2)
	{
		return true;
	}

	public static implicit operator Identifier (string identifier)
	{
		return null;
	}
	
	public static implicit operator String (Identifier id)
	{
		return null;
	}
	
	public static implicit operator decimal (Identifier id)
	{
		return -1;
	}	

	public static int Main ()
	{
		Identifier a = null;
		string b = "a";

		if (!(a == b))
			return 1;

		decimal d = 5;
		if (a == d)
			return 2;
		
		return 0;
	}
}

