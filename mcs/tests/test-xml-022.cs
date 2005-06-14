// Compiler options: -doc:xml-022.xml
//
// Combined tests (for detecting incorrect markup targeting).
//
using System;

/// <summary>
/// xml comment is not allowed here.
/// </summary>
namespace Testing
{
	/// <summary>
	/// </incorrect>
	public class Test2
	{
		/**
			another documentation style (Java-mimic)
		*/
		public static void Foo ()
		{
			/// here is an extraneous comment
		}

		public static void Main ()
		{
		}
	}

	/// testing indentation <summary> test test ;-)
	/// comment for struct
	/// </summary>
	public struct StructTest
	{
	}

	/// <summary>
	/// comment for interface
	/// </summary>
	public interface InterfaceTest
	{
	}

	/// <summary>
	/// comment for enum type
	/// </summary>
	public enum EnumTest
	{
		/// <summary>
		/// comment for enum field
		/// </summary>
		Foo,
		Bar,
	}

	/// <summary>
	/// comment for dummy type
	/// </summary>
	public class Dummy {}

	/// <summary>
	/// comment for delegate type
	/// </summary>
	public delegate void MyDelegate (object o, EventArgs e);

	/// <summary>
	/// description for class Test
	/// </summary>
	public class Test
	{
		/// comment for const declaration
		const string Constant = "CONSTANT STRING";

		/// comment for public field
		public string BadPublicField;

		/// comment for private field
		private string PrivateField;

		/// comment for public property
		public string PublicProperty {
			/// comment for private property getter
			get { return null; }
		}

		/// comment for private property
		private string PrivateProperty {
			get { return null; }
			/// comment for private property setter
			set { }
		}

		int x;

		/// public event EventHandler MyEvent ;-)
		public event EventHandler MyEvent;

		int y;

		/// here is a documentation!!!
		public static void Foo ()
		{
		}

		/// here is a documentation with parameters
		public static void Foo (long l, Test t, System.Collections.ArrayList al)
		{
		}

		/// comment for indexer
		public string this [int i] {
			get { return null; }
		}

		/// comment for indexer wit multiple parameters
		public string this [int i, Test t] {
			get { return null; }
		}

		/// <summary>
		/// comment for unary operator
		/// </summary>
		public static bool operator ! (Test t)
		{
			return false;
		}

		/// <summary>
		/// comment for binary operator
		/// </summary>
		public static int operator + (Test t, int b)
		{
			return b;
		}

		/// comment for destructor
		~Test ()
		{
		}

		/// comment for .ctor()
		public Test ()
		{
		}

		/// comment for .ctor(string arg, string [] args)
		public Test (string arg, string [] args)
		{
		}

		/// comment for internal class
		public class InternalClass
		{
		}

		/// comment for internal struct
		public struct InternalStruct
		{
		}
	}
}

