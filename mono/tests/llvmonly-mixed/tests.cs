using System;

public class BitcodeMixedTests
{
	public static int Main (String[] args) {
		return TestDriver.RunTests (typeof (BitcodeMixedTests), args);
	}

	public static int test_2_entry_simple () {
		return InterpOnly.entry_1 (1);
	}
}
