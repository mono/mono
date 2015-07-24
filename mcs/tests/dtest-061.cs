using System;

namespace Test
{
	public class Program
	{
		static int[] testValues = {0, -1, 200, -200, 32, -32, 64, -128, 333, 5};

		dynamic dynBase;
		dynamic dynAmt;

		int? optBase;
		int? optAmt;

		int normBase;
		int normAmt;

		dynamic uDynBase;

		public static void Main ()
		{
			var tester = new Program ();

			foreach (int baseVal in testValues)
				foreach (int amt in testValues)
					tester.ShiftTest (baseVal, amt);
		}

		public static void AreEqual<A, B> (A a, B b)
		{
			if (!a.Equals (b))
				throw new Exception (
					String.Format (
						"Shift Equality Assertion Failed: Had {0} and expected {1}", a, b));
		}

		public void ShiftTest (int shiftBase, int shiftAmt)
		{
			optBase = dynBase = normBase = shiftBase;
			optAmt = dynAmt = normAmt = shiftAmt;
			int immediate = shiftBase << shiftAmt;

			AreEqual<int?, int?> (dynBase << dynAmt, immediate);
			AreEqual<int?, int?> (dynBase << optAmt, immediate);
			AreEqual<int?, int?> (dynBase << normAmt, immediate);

			AreEqual<int?, int?> (optBase << dynAmt, immediate);
			AreEqual<int?, int?> (optBase << optAmt, immediate);
			AreEqual<int?, int?> (optBase << normAmt, immediate);

			AreEqual<int?, int?> (normBase << dynAmt, immediate);
			AreEqual<int?, int?> (normBase << optAmt, immediate);
			AreEqual<int?, int?> (normBase << normAmt, immediate);

			uint uShiftBase = (uint)shiftBase;
			uDynBase = uShiftBase;

			AreEqual<uint?, uint?> (uShiftBase << dynAmt, uDynBase << dynAmt);
			AreEqual<uint?, uint?> (uShiftBase << optAmt, uDynBase << optAmt);
			AreEqual<uint?, uint?> (uShiftBase << normAmt, uDynBase << normAmt);
		}
	}
}
