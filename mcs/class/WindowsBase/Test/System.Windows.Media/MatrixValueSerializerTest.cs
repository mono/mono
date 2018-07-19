using NUnit.Framework;
using System;
using System.Windows.Media.Converters;
using System.Windows.Media;

namespace MonoTests.System.Windows.Media {

	[TestFixture]
	public class MatrixValueSerializerTest {
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
		public void CanConvertFromString ()
		{
			var serializer = new MatrixValueSerializer ();
			Assert.IsTrue (serializer.CanConvertFromString ("", null));
		}

		[Test]
		public void CanConvertToString ()
		{
			var serializer = new MatrixValueSerializer ();
			Assert.IsTrue (serializer.CanConvertToString (new Matrix (1, 2, 3, 4, 5 ,6), null));
			Assert.IsFalse (serializer.CanConvertToString ("", null));
		}

		[Test]
		public void ConvertFromString ()
		{
			var serializer = new MatrixValueSerializer ();
			object obj = serializer.ConvertFromString ("1, 2, 3, 4, 5 ,6", null);
			Assert.AreEqual (typeof (Matrix), obj.GetType ());
			CheckMatrix (new Matrix (1, 2, 3, 4, 5, 6), (Matrix)obj);
		}

		[Test]
		public void RoundTripConvert()
		{
			var serializer = new MatrixValueSerializer ();
			var matrix = new Matrix (1.1, 2.2, 3.3, 4.4, 5.5, 6.6);
			var obj = serializer.ConvertFromString (serializer.ConvertToString (matrix, null), null);
			CheckMatrix (matrix, (Matrix)obj);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringHasInvalidFormat ()
		{
			var serializer = new MatrixValueSerializer ();
			serializer.ConvertFromString ("a,b,c,d,e,f", null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringIsNull ()
		{
			var serializer = new MatrixValueSerializer ();
			serializer.ConvertFromString (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToStringShouldThrowExceptionWhenInvalidType ()
		{
			var serializer = new MatrixValueSerializer ();
			serializer.ConvertToString (10, null);
		}
	}
}