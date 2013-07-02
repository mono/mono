using System;

//
// ~ constant folding
//
class X {
	const byte b = 0x0f;
	
	public static int Main ()
	{
		int x = ~b;
		byte bb = 0xf;
		
		if (~bb != x){
			Console.WriteLine ("{0:x}", x);
			return 1;
		}
		return 0;
	}
}
