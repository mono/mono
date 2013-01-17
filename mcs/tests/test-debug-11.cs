using System;
using System.Collections;
using System.Collections.Generic;

struct S : IDisposable, IEnumerable
{
	public void Dispose ()
	{
	}
	
	public IEnumerator GetEnumerator ()
	{
		return new List<int>().GetEnumerator (); 
	}
}

class C
{
	public static void Main ()
	{
	}

	void Using_1 ()
	{
		using (var s = new S ())
		{
		}
	}
	
	void Using_2 ()
	{
		using (S s = new S (), s2 = new S ())
		{
		}
	}
	
	void Using_3 ()
	{
		using (S? s = new S ())
		{
		}
	}

	void Using_4 ()
	{
		using (var ms = new System.IO.MemoryStream ())
		{
			Console.WriteLine ("a");
		}
	}
	
	void Lock ()
	{
		lock (this)
		{
		}
	}
	
	void Lock_2 ()
	{
		lock (this)
		{
			return;
		}
	}
	
	void Switch_1 (int arg)
	{
		switch (arg)
		{
			case 1:
				break;
			case 2:
			{
				break;
			}
			case 3:
				goto case 2;
			case 4:
			case 5:
				break;
			default:
				break;
		}
	}
	
	void Switch_2 (int? arg)
	{
		switch (arg)
		{
			case 1:
				break;
			case 2:
			{
				break;
			}
			default:
				break;
		}
	}
	
	void Switch_3 (string s)
	{
		switch (s)
		{
			case "a":
				break;
			case "b":
			{
				break;
			}
			case "c":
			case "e":
				goto case "a";
			case "f":
				break;
			case "gggg":
			case "hhh":
			case "iii":
				break;
			default:
				break;
		}
	}

	void Switch_4 (string s)
	{
		switch (s)
		{
			case "a":
				break;
			case "b":
				break;
			default:
				break;
		}
	}
	
	void Checked ()
	{
		checked
		{
			int a = 1;
		}
		
		unchecked
		{
			int a = 2;
		}
	}

	void DoWhile (int arg)
	{
		do
		{
		}
		while (arg != 0);
		
		while (arg > 0)
		{
		}
	}
	
	void DoWhile_2 ()
	{
		do
		{
			int i = 2;
		}
		while (true);
	}

	void While_2 ()
	{
		while (true)
		{
			Console.WriteLine ("aa");
		}
	}

	void If (string s)
	{
		if (s == "a")
		{
		}
		else
		{
		}
	}
	
	void If_2 (string s)
	{
		if (s == "a")
		{
		}
		else if (s == "b")
		{
		}
		else
		{
		}
	}
	
	void If_3 (int i)
	{
		if (i == i)
		{
		}
		else
		{
		}
	}

	void For_1 ()
	{
		for (int i = 0;
		i < 4;
		++i)
		{
		}
		
		for (;
		;
		)
		{
		}
	}
	
	void For_2 ()
	{
		for (int i = 0; ;)
		{
		}
	}
	
	void ForEach (int[] args)
	{
		foreach (
		var a
		in args)
		{
		}
	}
	
	void ForEach_2 (List<object> args)
	{
		foreach
		(var a
		in
		args)
		{
		}
	}

	void ForEach_3 (S args)
	{
		foreach
		(var a
		in
		args)
		{
		}
	}
	
	void ForEach_4 (int[,] args)
	{
		foreach (
		var a
		in args)
		{
		}
	}
}
