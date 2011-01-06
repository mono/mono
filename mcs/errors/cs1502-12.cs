// CS1502: The best overloaded method match for `Global.Test1(int?)' has some invalid arguments
// Line: 8

using System;

class Global {
	static void Main() {
		Console.Write(Test1((decimal?)2));
	}	
	static string Test1(int? value) {
		return "ok";
	}
}
