// Compiler options: -doc:xml-027.xml
using ZZZ = Testing.Test;

namespace Testing
{
	/// <summary>
	/// <see />
	/// <see cref='!!!!!' />
	/// <see cref='nonexist' />
	/// <see cref='Test' />
	/// <see cref='ZZZ' />
	/// <see cref='T:Test' />
	/// <see cref='_:Test' />
	/// <see cref='P:Bar' />
	/// <see cref='F:Bar' />
	/// <see cref='Bar' />
	/// <see cref='P:Baz' />
	/// <see cref='F:Baz' />
	/// <see cref='Baz' />
	/// <see cref='nonexist.Foo()' />
	/// <see cref='Test.Foo()' />
	/// <see cref='ZZZ.Foo()' />
	/// <see cref='Test.Bar()' />
	/// <see cref='Test.Foo(System.Int32)' />
	/// </summary>
	class Test
	{
		public static void Main () { System.Console.Error.WriteLine ("xml-027 is running fine ;-)"); }

		// I don't put any documentation here, but cref still works.
		public void Foo () {}

		public string Bar;

		public string Baz { get { return ""; } }
	}
}

