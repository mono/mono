using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Project01
{
	class Program
	{
		static Lib5.Book b;
		static void Main (string [] args)
		{
			//FIXME: reqd?
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

			Lib1.Book book = new Lib1.Book ();
			Console.WriteLine ("Book: default name: {0}", book.Name);

			Lib1.Publisher publisher = new Lib1.Publisher ();
			Console.WriteLine ("Publisher default name: {0}", publisher.Name);

			Foo f = new Foo ();
			Bar b = new Bar ();
		}
	}
}
