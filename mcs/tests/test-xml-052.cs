// Compiler options: -doc:xml-052.xml -warnaserror
	// mcs /doc test for nested types

	/// <summary>Global delegate</summary>
	public delegate void GlobalDel ();


	/// <summary>Outer class</summary>
	public class Outer {
		/// <summary>Inner Class</summary>
		public delegate void Del ();

		static void Main ()
		{
		}
	}

