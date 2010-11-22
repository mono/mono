using System;

public class broken_cast
{
	public static void report (string str)
	{
		throw new Exception (str);
	}

	public static void conv_ovf_i (long val, bool shouldThrow)
	{
		try {
			System.IntPtr x = (System.IntPtr) val;
			if (shouldThrow)
				report (String.Format ("conv_ovf_i did not throw for {0} ", val));
		} catch (OverflowException exception) {
			if (!shouldThrow)
				report (String.Format ("conv_ovf_i did throw for {0}", val));
		}
	}

	public static void conv_ovf_i_un (long val, bool shouldThrow)
	{
		try {
			System.IntPtr x = (System.IntPtr) val;
			if (shouldThrow)
				report (String.Format ("conv_ovf_i_un did not throw for {0} ", val));
		} catch (OverflowException exception) {
			if (!shouldThrow)
				report (String.Format ("conv_ovf_i_un did throw for {0}", val));
		}
	}

	public static void conv_ovf_u (long val, bool shouldThrow)
	{
		try {
			System.IntPtr x = (System.IntPtr) val;
			if (shouldThrow)
				report (String.Format ("conv_ovf_u did not throw for {0} ", val));
		} catch (OverflowException exception) {
			if (!shouldThrow)
				report (String.Format ("conv_ovf_u did throw for {0}", val));
		}
	}

	public static void conv_ovf_u_un (long val, bool shouldThrow)
	{
		try {
			System.IntPtr x = (System.IntPtr) val;
			if (shouldThrow)
				report (String.Format ("conv_ovf_u_un did not throw for {0} ", val));
		} catch (OverflowException exception) {
			if (!shouldThrow)
				report (String.Format ("conv_ovf_u_un did throw for {0}", val));
		}
	}

	public static int Main ()
	{
		long ok_number = 9;
		long negative = -1;
		long biggerThanI4 = int.MaxValue;
		++biggerThanI4;
		long smallerThanI4 = int.MinValue;
		--smallerThanI4;
		long biggerThanU4 = uint.MaxValue;
		++biggerThanU4;

		bool is32bits = IntPtr.Size == 4;
		int i = 1;

		try {
			conv_ovf_i (ok_number, false);
			++i;
			conv_ovf_i (negative, false);
//			++i;
//			conv_ovf_i (biggerThanI4, true && is32bits);
//			++i;
//			conv_ovf_i (smallerThanI4, true && is32bits);
//			++i;
//			conv_ovf_i (biggerThanU4, true && is32bits);

			++i;
			conv_ovf_i_un (ok_number, false);
			++i;
			conv_ovf_i_un (negative, false);
			++i;
//			conv_ovf_i_un (biggerThanI4, true && is32bits);
//			++i;
//			conv_ovf_i_un (smallerThanI4, true && is32bits);
//			++i;
//			conv_ovf_i_un (biggerThanU4, true && is32bits);

			++i;
			conv_ovf_u (ok_number, false);
			++i;
			conv_ovf_u (negative, false);
//			++i;
//			conv_ovf_u (biggerThanI4, true && is32bits);
//			++i;
//			conv_ovf_u (smallerThanI4, true && is32bits);
//			++i;
//			conv_ovf_u (biggerThanU4, true && is32bits);

			++i;
			conv_ovf_u_un (ok_number, false);
			++i;
			conv_ovf_u_un (negative, false);
//			++i;
//			conv_ovf_u_un (biggerThanI4, true && is32bits);
//			++i;
//			conv_ovf_u_un (smallerThanI4, true && is32bits);
//			++i;
//			conv_ovf_u_un (biggerThanU4, true && is32bits);

			return 0;
		} catch (Exception e) {
			Console.WriteLine (e);
			return i;
		}
	}

}
