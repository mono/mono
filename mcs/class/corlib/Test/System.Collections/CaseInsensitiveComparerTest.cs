// CaseInsensitiveComparerTest

using System;
using System.Collections;

using NUnit.Framework;



namespace MonoTests.System.Collections {


	/// <summary>CaseInsensitiveComparer test suite.</summary>
	public class CaseInsensitiveComparerTest : TestCase {
		protected override void SetUp ()
		{
		}

		public void TestDefaultInstance ()
		{
			// Make sure the instance returned by Default
			// is really a CaseInsensitiveComparer.
			Assert((CaseInsensitiveComparer.Default
			        as CaseInsensitiveComparer) != null);
		}

		public void TestCompare () {
			CaseInsensitiveComparer cic = new CaseInsensitiveComparer ();

			Assert(cic.Compare ("WILD WEST", "Wild West") == 0);
			Assert(cic.Compare ("WILD WEST", "wild west") == 0);
			Assert(cic.Compare ("Zeus", "Mars") > 0);
			Assert(cic.Compare ("Earth", "Venus") < 0);
		}
			
	}

}
