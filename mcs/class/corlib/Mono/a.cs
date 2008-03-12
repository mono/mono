using System;
using System.Text;
using Mono;

class X {
	static bool fail = false;
	
	static void Dump (byte [] d, string s)
	{
		StringBuilder sb = new StringBuilder ();
		
		for (int i = 0; i < d.Length; i++){
			if ((i % 24) == 0){
				Console.Write ("\n{0:x6}: ", i);
			}
			Console.Write ("{0:x2} ", d [i]);
			sb.Append (String.Format ("{0:x2} ", d [i]));
		}
		if (s != null){
			string result = sb.ToString ().Trim ();
			if (s.Trim () != result){
				Console.WriteLine ();
				Console.WriteLine ("FAILURE:");
				Console.WriteLine ("  Got:      [{0}]", result);
				Console.WriteLine ("  Expected: [{0}]", s);
				fail = true;
			}
		}
		Console.WriteLine ();
	}

	static void Main ()
	{
		Dump (DataConverter.Pack ("z8", "hello"), "68 65 6c 6c 6f 00 00 00 00");
		Dump (DataConverter.Pack ("z6", "hello"), "68 00 65 00 6c 00 6c 00 6f 00 00 00 00 00");
		Dump (DataConverter.Pack ("CCCC", 65, 66, 67, 68), "41 42 43 44");

		Dump (DataConverter.Pack ("4C", 65, 66, 67, 68, 69, 70),  "41 42 43 44");
		Dump (DataConverter.Pack ("^iii", 0x1234abcd, 0x7fadb007), " 12 34 ab cd 7f ad b0 07 00 00 00 00");
		Dump (DataConverter.Pack ("_s!i", 0x7b, 0x12345678), "7b 00 00 00 78 56 34 12");

		byte [] b = DataConverter.Pack ("4C", 1, 2, 3, 4);
		foreach (object c in DataConverter.Unpack ("4C", b, 0)){
			Console.WriteLine ("->{0} {1}", c, c.GetType ());
		}
		Console.WriteLine ("Tests {0}", fail ? "failed" : "passed");

		byte [] source = new byte [] { 1, 2, 3, 4 };
		byte [] dest = new byte [4];

		int l = DataConverter.Int32FromBE (source, 0);
		if (l != 0x01020304){
			Console.WriteLine ("Failure");
		}
	}
}
