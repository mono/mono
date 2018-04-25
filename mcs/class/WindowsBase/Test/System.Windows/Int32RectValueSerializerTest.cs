using System;
using System.Windows;
using System.Windows.Converters;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class Int32RectValueSerializerTest
	{
		[Test]
		public void CanConvertFromString ()
		{
			var serializer = new Int32RectValueSerializer ();
			Assert.IsTrue (serializer.CanConvertFromString ("", null));
		}

		[Test]
		public void CanConvertToString ()
		{
			var serializer = new Int32RectValueSerializer ();
			Assert.IsTrue (serializer.CanConvertToString (new Int32Rect (0, 0, 0, 0), null));
			Assert.IsFalse (serializer.CanConvertToString ("", null));
		}

		[Test]
		public void ConvertFromString ()
		{
			var serializer = new Int32RectValueSerializer ();
			object obj = serializer.ConvertFromString ("3,4,5,6", null);
			Assert.AreEqual (typeof (Int32Rect), obj.GetType ());
			Assert.AreEqual (new Int32Rect (3, 4, 5, 6), obj);
		}

		[Test]
		public void RoundTripConvert()
		{
			var serializer = new Int32RectValueSerializer ();
			var size = new Int32Rect (7, 8, 9, 10);
			var obj = serializer.ConvertFromString (serializer.ConvertToString (size, null), null);
			Assert.AreEqual (size, obj);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringHasInvalidFormat ()
		{
			var serializer = new Int32RectValueSerializer ();
			serializer.ConvertFromString ("a,b,c,d", null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringIsNull ()
		{
			var serializer = new Int32RectValueSerializer ();
			serializer.ConvertFromString (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToStringShouldThrowExceptionWhenInvalidType ()
		{
			var serializer = new Int32RectValueSerializer ();
			serializer.ConvertToString (10, null);
		}
	}

}
