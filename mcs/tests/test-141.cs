using System;

class X {
	public static int Main()
	{
		byte num1 = 105;
		byte num2 = 150;
		byte sum;

		bool ok = false;
		
		// should generate OverflowException
		try {
			checked {
				sum = (byte) (num1 - num2);
			}
		} catch (OverflowException){
			ok = true;
		}

		if (ok)
			return 0;
		return 1;
	}
}
