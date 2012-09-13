using System;
using System.Collections.Generic;

class A
{
	public IEnumerable<string> Test (B b)
	{
		string s = "s";

		yield return "a";
		{
			string stringValue = "two";

			Console.WriteLine (b.ToString ());
			{
				Action a = () => {
					Console.WriteLine (s + c.GetType () + stringValue);
				};

				a ();
			}
		}
	}

	C c = new C ();
}

class B
{
}

class C
{
    public static void Main ()
    {
    }
}