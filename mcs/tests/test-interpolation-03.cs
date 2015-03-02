using System;

public static class Test {
	public static void Main() {
		RunTest(() => Console.WriteLine ($"Initializing the map took {1}ms"));
	}

	static void RunTest (Action callback)
	 {
		callback();
	}
}