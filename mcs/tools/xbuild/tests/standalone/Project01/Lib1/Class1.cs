using System;
using System.Resources;
using System.Reflection;

namespace Lib1
{
	public class Book
	{
		string name;
		//this is here just to create a ref to lib2
		Lib2.Book b;

		public Book ()
		{
			name = GetDefaultName ();
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		static string GetDefaultName ()
		{
			return new ResourceManager (
				"Lib1.Book", Assembly.GetExecutingAssembly ())
				.GetString ("defaultName");
		}
	}

	public class Publisher
	{
		string name;

		public Publisher ()
		{
			name = GetDefaultName ();
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		static string GetDefaultName ()
		{
			return new ResourceManager (
				"Lib1.Publisher", Assembly.GetExecutingAssembly ())
				.GetString ("defaultName");
		}
	}


}
