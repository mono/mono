// CS0157: Control cannot leave the body of a finally clause
// Line: 12

class T {
	static void Main ()
	{
		while (true) { 
			try {
				System.Console.WriteLine ("trying");
			} finally {
				try {
				    break;
				}
				catch {}
			}
		}
	}
}