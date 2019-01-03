using System;
using System.Collections;

public class BitcodeMixedTests
{
	public static int Main (String[] args) {
		return TestDriver.RunTests (typeof (BitcodeMixedTests), args);
	}

	public static int test_2_entry_simple () {
		return InterpOnly.entry_1 (1);
	}

	public static int test_0_corlib_call () {
		var alist2 = new ArrayList (10);
		var alist = InterpOnly.corlib_call ();
		return alist.Capacity == 5 ? 0 : 1;
	}

	public static int test_2_entry_delegate () {
		Func<int, int> func = InterpOnly.entry_1;

		return func (1);
	}

	public static int test_1_entry_delegate_unbox () {
		var s = new InterpOnlyStruct () { Field = 1 };
		if (s.get_Field () != 1)
			return 2;
		Func<int> func = s.get_Field;
		return func ();
	}
}
