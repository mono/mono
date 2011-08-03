// CS0157: Control cannot leave the body of a finally clause
// Line: 11

class T {
	static void Main ()
	{
		while (true) { 
			try {
				System.Console.WriteLine ("trying");
			} finally {
				break;
			}
		}
	}
}