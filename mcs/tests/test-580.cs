using System;

public class Bla {
	
	public static void BuildNode (ref string label)
	{
		string s = "a";
		label += s + s + s + s;
	}
	
	public static void BuildNode (ref string[] label)
	{
		string s = "a";
		int idx = 0;
		label[idx++] += s + s + s + s;
	}
	
	public static void BuildNode_B (ref object label)
	{
		string s = "b";
		label += s + s;
	}
	
	public static string BuildNode_C (ref string label)
	{
		string[] a = new string [2];
		int i = 0;
		a [0] = "a";
		string s = "b";

		a [i++] += label + s + s + s;
		return a [i - 1];
	}
	
	public static string BuildNode_D ()
	{
		System.Collections.ArrayList values = new System.Collections.ArrayList ();
		for (int i = 0; i < 6; i++)
			values.Add (i);
		string[] strs = new string [values.Count];
		int idx = 0;
		foreach (int val in values) {
			strs [idx] = "Value:";
			strs [idx++] += val.ToString ();
		}
		
		return strs [5];
	}
	
	public static void BuildNode_E (ref string[,] label)
	{
		string s = "a";
		int idx = 0;
		label = new string [1, 1];
		label[idx++, idx - 1] += s + s + s + s;
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
		string res = BuildNode_C (ref str);
		Console.WriteLine (str);
		if (str != "test")
			return 3;
		
		Console.WriteLine (res);
		if (res != "atestbbb")
			return 4;
		
		string[] sa = new string [1];
		BuildNode (ref sa);
		Console.WriteLine (sa [0]);
		if (sa [0] != "aaaa")
			return 5;
		
		str = BuildNode_D ();
		Console.WriteLine (str);
		if (str != "Value:5")
			return 6;
		
		string[,] sa2 = null;
		BuildNode_E (ref sa2);
		Console.WriteLine (sa2 [0, 0]);
		if (sa2 [0,0] != "aaaa")
			return 7;
		
		return 0;
	}
}
