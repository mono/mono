using System.Collections.Generic;

class CantCastGenericListToArray
{
	public static void Main(string[] args)
	{
		IList<string> list = new string[] { "foo", "bar" };
		string[] array = (string[])list;
		if (list.Count != array.Length)
		{
			throw new System.ApplicationException();
		}
	}
}
