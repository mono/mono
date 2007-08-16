using System;

public class Bla {
	
	public static void BuildNode (ref string label)
	{
		string s = "a";
		label += s + s + s + s;
	}
	
	public static void BuildNode_B (ref object label)
	{
		string s = "b";
		label += s + s;
	}
	
	public static void BuildNode_C (ref string label)
	{
		string[] a = new string [2];
		int i = 0;
		a [0] = "a";
		string s = "b";

		a [i++] += label + s + s + s;
	}
	
	public static int Main ()
	{
		String str = "test";
		BuildNode (ref str);
		Console.WriteLine (str);
		if (str != "testaaaa")
			return 1;
		
		object ostr = "test";
		BuildNode_B (ref ostr);
		Console.WriteLine (ostr);
		if (ostr.ToString () != "testbb")
			return 2;
		
		str = "test";
		BuildNode_C (ref str);
		Console.WriteLine (str);
		if (str != "test")
			return 3;
		
		return 0;
	}
}
