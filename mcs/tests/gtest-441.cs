using System.Collections.Generic;

namespace Name
{
	public class Test
	{
		internal static List<int> List;
	}

	public class Subclass : Test
	{
		private List<int> list;

		public List<int> List
		{
			get { return list; }
		}

		public static void Main (string[] args)
		{
			Subclass c = new Subclass ();
		}
	}
}

