// Compiler options: -doc:xml-013.xml
using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// comment for public property
		/// </summary>
		public string PublicProperty {
			/// <summary>
			/// On public getter - no effect
			/// </summary>
			get { return null; }
			/// <summary>
			/// On public setter - no effect
			/// </summary>
			set { }
		}

		/// <summary>
		/// incorrect comment for public property
		/// </incorrect>
		public string PublicProperty2 {
			get { return null; }
		}

		/**
		<summary>
		Javadoc comment for public property
		</summary>
		*/
		public string PublicProperty3 {
			/**
			<summary>
			On public getter - no effect
			</summary>
			*/
			get { return null; }
			/**
			<summary>
			On public setter - no effect
			</summary>
			*/
			set { }
		}
	}
}

