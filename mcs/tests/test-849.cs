using System;

class ConditionalPromotions
{
	public static int Test (bool condition, short value)
	{
		return condition ? -1 : value;
	}

	public static int Main(string[] args)
	{
		var r1 = args.Length > 0 ? 1 : (short)1;
		var r2 = args.Length > 0 ? (short)1 : 1;
		var r3 = args.Length > 0 ? 1 : (uint)1;
		var r4 = args.Length > 0 ? (uint)1 : 1;
		var r5 = args.Length > 0 ? 0 : (uint)1;

		if (r1.GetType () != typeof (int))
			return 1;

		if (r2.GetType () != typeof (int))
			return 2;
		
		if (r3.GetType () != typeof (uint))
			return 3;

		if (r4.GetType () != typeof (uint))
			return 4;

		if (r5.GetType () != typeof (uint))
			return 5;
		
		byte x = 4;
		byte a = (byte)(true ? x : 0);
		
		return 0;
	}
}