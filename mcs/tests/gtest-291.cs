using System;

class Global {
	public static void Main() {
		Console.Write(Test2((int?)2));
	}
	static string Test2(decimal? value) {
		return "ok";
	}
}
