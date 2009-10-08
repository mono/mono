using System;
using System.Resources;
using System.Reflection;

namespace Lib5
{
	public class Book
	{
		string name;

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
				"Lib5.Book", Assembly.GetExecutingAssembly ())
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
				"Lib5.Publisher", Assembly.GetExecutingAssembly ())
				.GetString ("defaultName");
		}
	}


}
