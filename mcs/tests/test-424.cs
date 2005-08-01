//
// Sample for bug 75674
//
using System;
using System.Collections;

class XX {
	static void Metodo (Exception e)
	{
		if (e is NotImplementedException){
			Console.WriteLine ("OK");
		} else {
			Console.WriteLine ("Fail");
		}
	}
	
	static IEnumerable X ()
	{
		try {
			throw new NotImplementedException ();
		} catch (Exception e){
			Metodo (e);
		}
		yield return 0;
	}
	
	static void Main ()
	{
		foreach (int a in X ()){
		}
	}
}
