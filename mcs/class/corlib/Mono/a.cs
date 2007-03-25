using System;
using Mono;

class X {
	static void Dump (byte [] d)
	{
		for (int i = 0; i < d.Length; i++){
			if ((i % 24) == 0){
				Console.Write ("\n{0:x6}: ", i);
			}
			Console.Write ("{0:x2} ", d [i]);
		}
		Console.WriteLine ();
	}

	static void Main ()
	{
		//Dump (DataConverter.Pack ("z8", "hello"));
		//Dump (DataConverter.Pack ("z6", "hello"));
		//Dump (DataConverter.Pack ("CCCC", 65, 66, 67, 68));
		
		//Dump (DataConverter.Pack ("4C", 65, 66, 67, 68, 69, 70));
		//Dump (DataConverter.Pack ("^iii", 0x1234abcd, 0x7fadb007));
		Dump (DataConverter.Pack ("_s!i", 0x7b, 0x12345678));
	}
}
