using System;

class X
{
	static int counter;
	static int Index ()
	{
		if (counter++ != 0)
			throw new ApplicationException ();
		
		return 1;
	}
	
	int? indexer;
	int? this [int index] {
		get {
			return indexer;
		}
		set {
			indexer = value;
		}
	}	
	
	static int Test ()
	{
		int? a = 5;
		int? b = a++;

		if (a != 6)
			return 1;
		if (b != 5)
			return 2;

		int? c = ++a;

		if (c != 7)
			return 3;

		b++;
		++b;

		if (b != 7)
			return 4;

		int? d = b++ + ++a;

		if (a != 8)
			return 5;
		if (b != 8)
			return 6;
		if (d != 15)
			return 7;
		
		var s = new short?[] { 3, 2, 1 };
		counter = 0;
		var r = s [Index ()]++;
		if (counter != 1)
			return 8;
		
		if (r != 2)
			return 9;
		
		if (s[1] != 3)
			return 10;

		counter = 0;
		s [Index ()]++;
		if (counter != 1)
			return 11;
		
		if (s[1] != 4)
			return 12;
			
		X x = new X ();
		x.indexer = 6;
		counter = 0;
		var r2 = x[Index ()]--;
		if (counter != 1)
			return 13;
			
		if (r2 != 6)
			return 14;
		
		if (x.indexer != 5)
			return 15;

		return 0;
	}

	static int Main ()
	{
		int result = Test ();
		if (result != 0)
			Console.WriteLine ("ERROR: {0}", result);
		return result;
	}
}
