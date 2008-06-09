using System;

namespace ParamMismatch
{
	public class TestCase
	{
		public static void Main()
		{
		}
		
		public TestCase()
		{
		}

		public event EventHandler Culprit
		{
			add
			{
				// even when this contained something, compiling would fail
			}

			remove
			{
				// even when this contained something, compiling would fail
			}
		}
		~TestCase()
		{
		}
	}
}
