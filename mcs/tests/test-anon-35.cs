using System;

public class ExceptionWithAnonMethod
{
	public delegate void EmptyCallback();
    	static string res;
	
	public static int Main()
	{
		try {
			throw new Exception("e is afraid to enter anonymous land");
		} catch(Exception e) {
			AnonHandler(delegate {
				Console.WriteLine(e.Message); 
				res = e.Message;
			});
		}
		if (res == "e is afraid to enter anonymous land"){
		    Console.WriteLine ("Test passed");
		    return 0;
		}
		Console.WriteLine ("Test failed");
		return 1;
	}

	public static void AnonHandler(EmptyCallback handler)
	{
		if(handler != null) {
			handler();
		}
	}
}
