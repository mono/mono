using System;
using System.Resources;
using System.Reflection;

namespace Lib2
{
	public class Book
	{
		string name;
		Lib4.Class1 c1;
		Lib5.Book b;

		public Book ()
		{
			name = GetDefaultName ();
			b = new Lib5.Book ();
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		static string GetDefaultName ()
		{
			return new ResourceManager (
				"Lib2.Book", Assembly.GetExecutingAssembly ())
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
				"Lib2.Publisher", Assembly.GetExecutingAssembly ())
				.GetString ("defaultName");
		}
	}


}
