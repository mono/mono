// CS1502: The best overloaded method match for `TestCase.TestS(ref object)' has some invalid arguments
// Line: 21

using System;

public struct Struct {
	public int x, y, z;
}

public class TestCase {
	
	public static void Main() {
		
		Struct s = new Struct();
		
		s.x = 1;
		s.y = 2;
		
		System.Console.WriteLine("{0} {1} {2}", s.x, s.y, s.z);
		
		TestS(ref s);
	}	
	
	public static void TestS(ref object ino) {
		System.Console.WriteLine("{0}", ((Struct)(ino)).x);
	}
	
}
