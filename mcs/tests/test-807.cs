using System;

class AException : Exception
{
}

class Program
{
	public static int Main ()
	{
		try {
			throw new AException ();
		} catch (AException e1) {
			Console.WriteLine ("a");
			try {
			} catch (Exception) {
			}
			
			return 0;
		} catch (Exception e) {
			Console.WriteLine ("e");
		}
		
		return 1;
	}
}