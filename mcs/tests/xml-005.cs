// Compiler options: -doc:xml-005.xml
using System;

namespace Testing
{
	/// <summary>
	/// comment for interface
	/// </summary>
	public interface InterfaceTest
	{
	}

	/// <summary>
	/// incorrect markup comment for interface
	/// </incorrect>
	public interface InterfaceTest2
	{
	}

	/**
		<summary>
		Java style comment for interface
		</summary>
	*/
	public interface InterfaceTest3
	{
	}

	public class Test
	{
		public static void Main ()
		{
		}
	}
}

