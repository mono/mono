using System;

class X {
	public static int Main()
	{
		if (! Test1 ()) return 1;
		if (! Test2 ()) return 2;
		if (! Test3 ()) return 3;
		
		return 0;
	}
	
	static bool Test1 ()
	{
		byte num1 = 105;
		byte num2 = 150;

		// should generate OverflowException
		try {
			checked {
				byte sum = (byte) (num1 - num2);
			}
			
			return false;
		} catch (OverflowException) {
			return true;
		}
	}
	
	static bool Test2 ()
	{
		long l = long.MinValue;
		
		// should generate OverflowException
		try {
			checked {
				l = - l;
			}
			
			return false;
		} catch (OverflowException) {
			return true;
		}
	}
	
	static bool Test3 ()
	{
		int i = int.MinValue;
		
		// should generate OverflowException
		try {
			checked {
				i = - i;
			}
			
			return false;
		} catch (OverflowException) {
			return true;
		}
	}
}
