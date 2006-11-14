using System;
using System.Collections;

public class Tester
{
	string[] ABC = { "A", "B", "C" };
	// D
	string [,] EFGH = { { "E", "F" }, { "G", "H"}};
	// I
	ArrayList al = new ArrayList ();
	
	public Tester ()
	{
		al.Add ("J");
		al.Add ("K");
	}
	
	public System.Collections.IEnumerator GetEnumerator()
	{
		foreach (string s in ABC){
			if (s == null)
				throw new Exception ();
			else
				yield return s;
		}
		
		yield return "D";
		foreach (string s in EFGH){
			if(s == null)
				throw new Exception ();
			else
				yield return s;
		}
		
		yield return "I";
		foreach (string s in al){
			if (s == null)
				throw new Exception ();
			else
				yield return s;
		}
		
		yield return "L";
	}
}


class Test
{
	public static int Main()
	{
		Tester tester = new Tester();
		string [] list = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
		int top = 0;
			
		foreach (string s in tester){
			if (s != list [top]){
				Console.WriteLine ("Failure, got {0} expected {1}", s, list [top]);
				return 1;
			}
			top++;
		}
		if (top != list.Length){
			Console.WriteLine ("Failure, expected {0} got {1}", list.Length, top);
		}
		Console.WriteLine ("Success");
		return 0;
	}
}
