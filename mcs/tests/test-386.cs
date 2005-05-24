using System;

class SuperDecimal{
	private Decimal val;

	public SuperDecimal (Decimal val)
	{
		this.val = val;
	}

	public static implicit operator SuperDecimal (Decimal val)
	{
		return new SuperDecimal (val);
	}

	public static void Main ()
	{
		int i = 2;
		SuperDecimal sd = i;
	}
}
