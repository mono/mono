// Compiler options: -doc:xml-060.xml /warnaserror /warn:4

	// gmcs documents generic members incorrectly
	using System.Collections.Generic;

	namespace Test {
		/// <remarks>T:Test.DocMe</remarks>
		/// <seealso cref="T:Test.DocMe`1" />
		class DocMe {

			/// <remarks>M:Test.DocMe.UseList(System.Collections.Generic.List{System.Int32})</remarks>
			public static void UseList (List<int> list) {}

			/// <remarks>M:Test.DocMe.Main</remarks>
			public static void Main ()
			{
			}
		}

		/// <remarks>T:Test.DocMe`1</remarks>
		class DocMe<T> {
			/// <remarks>M:Test.DocMe`1.UseList(System.Collections.Generic.List{`0})</remarks>
			public void UseList (List<T> list) {}

			/// <remarks>M:Test.DocMe`1.UseList`1(System.Collections.Generic.List{``0})</remarks>
			public void UseList<U> (List<U> list) {}

			/// <remarks>M:Test.DocMe`1.RefMethod`1(`0@,``0@)</remarks>
			public void RefMethod<U> (ref T t, ref U u) {}
		}
	}

