using System;
using System.Collections;

namespace NUnit.Core
{
	public class CategoryManager
	{
		private static Hashtable categories = new Hashtable();

		public static void Add(string name) 
		{
			categories[name] = name;
		}

		public static void Add(IList list) 
		{
			foreach(string name in list) 
			{
				Add(name);
			}
		}

		public static ICollection Categories 
		{
			get { return categories.Values; }
		}

		public static void Clear() 
		{
			categories = new Hashtable();
		}
	}
}
