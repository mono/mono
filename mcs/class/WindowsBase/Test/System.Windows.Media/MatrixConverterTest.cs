using NUnit.Framework;
using System;
using System.Globalization;
using System.Windows.Media;

namespace MonoTests.System.Windows.Media {

	[TestFixture]
	public class MatrixConverterTest {
		const double DELTA = 0.000000001d;

		void CheckMatrix (Matrix expected, Matrix actual)
		{
			Assert.AreEqual (expected.M11, actual.M11, DELTA);
			Assert.AreEqual (expected.M12, actual.M12, DELTA);
			Assert.AreEqual (expected.M21, actual.M21, DELTA);
			Assert.AreEqual (expected.M22, actual.M22, DELTA);
			Assert.AreEqual (expected.OffsetX, actual.OffsetX, DELTA);
			Assert.AreEqual (expected.OffsetY, actual.OffsetY, DELTA);
		}

		[Test]
		public void CanConvertFrom ()
		{
			var conv = new MatrixConverter ();
			Assert.IsTrue (conv.CanConvertFrom (typeof (string)));
			Assert.IsFalse (conv.CanConvertFrom (typeof (Matrix)));
		}

		[Test]
		public void CanConvertTo ()
		{
			var conv = new MatrixConverter ();
			Assert.IsTrue (conv.CanConvertTo (typeof (string)));
			Assert.IsFalse (conv.CanConvertTo (typeof (Matrix)));
		}

		[Test]
		public void ConvertFrom ()
		{
			var conv = new MatrixConverter ();
			object obj = conv.ConvertFrom ("1, 2, 3, 4, 5, 6");
			Assert.AreEqual (typeof (Matrix), obj.GetType ());
			CheckMatrix (new Matrix (1, 2, 3, 4, 5, 6), (Matrix)obj);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromInvalidType ()
		{
			var conv = new MatrixConverter ();
			conv.ConvertFrom (new Matrix (10, 20, 30, 40, 50, 60));
		}

		[Test]
		public void ConvertTo ()
		{
			var conv = new MatrixConverter ();
			var matrix = new Matrix (1, 2, 3, 4, 5, 6);
			object obj = conv.ConvertTo (null, CultureInfo.InvariantCulture, matrix, typeof (string));
			Assert.AreEqual (typeof (string), obj.GetType ());
			Assert.AreEqual ("1,2,3,4,5,6", (string)obj);
		}
	}
}