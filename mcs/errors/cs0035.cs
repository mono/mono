// cs0035.cs: Ambiguous 'operator' on an operand of type 'foo'
// Line: 23 

using System;

class ErrorCS0035 {
	int i;
	public ErrorCS0035 (int x) {
		i = x;
	}
	
	public static implicit operator long (ErrorCS0035 x) {
		return (long) x.i;
	}

	public static implicit operator ulong (ErrorCS0035 x) {
		return (ulong) x.i;
	}
	
	public static void Main () {
		object o;
		ErrorCS0035 error = new ErrorCS0035 (10);
		Console.WriteLine ("o = -i = {0}", - error); 
		
	}
}

