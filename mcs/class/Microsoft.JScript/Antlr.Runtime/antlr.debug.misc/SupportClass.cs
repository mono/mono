using System;
internal class SupportClass
{
	public static int URShift(int number, int bits)
	{
		if ( number >= 0)
			return number >> bits;
		else
			return (number >> bits) + (2 << ~bits);
	}

	public static int URShift(int number, long bits)
	{
		return URShift(number, (int)bits);
	}

	public static long URShift(long number, int bits)
	{
		if ( number >= 0)
			return number >> bits;
		else
			return (number >> bits) + (2L << ~bits);
	}

	public static long URShift(long number, long bits)
	{
		return URShift(number, (int)bits);
	}
}
