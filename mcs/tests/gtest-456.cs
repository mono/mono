using System;

class G<T>
{
	public struct S
	{
		public string Test ()
		{
			return GetType ().ToString ();
		}
	}
}

class C
{	
	public static int Main ()
	{
		string s = new G<int>.S ().Test ();
		if (s != "G`1+S[System.Int32]")
			return 1;

		return 0;
	}
}
