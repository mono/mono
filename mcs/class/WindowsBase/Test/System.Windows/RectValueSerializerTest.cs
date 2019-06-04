using System;
using System.Windows;
using System.Windows.Converters;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class RectValueSerializerTest
	{
		[Test]
		public void CanConvertFromString ()
		{
			var serializer = new RectValueSerializer ();
			Assert.IsTrue (serializer.CanConvertFromString ("", null));
		}

		[Test]
		public void CanConvertToString ()
		{
			var serializer = new RectValueSerializer ();
			Assert.IsTrue (serializer.CanConvertToString (new Rect (0, 0, 0, 0), null));
			Assert.IsFalse (serializer.CanConvertToString ("", null));
		}

		[Test]
		public void ConvertFromString ()
		{
			var serializer = new RectValueSerializer ();
			object obj = serializer.ConvertFromString ("3.14,4.15,5.16,6.17", null);
			Assert.AreEqual (typeof (Rect), obj.GetType ());
			Assert.AreEqual (new Rect (3.14, 4.15, 5.16, 6.17), obj);
		}

		[Test]
		public void RoundTripConvert()
		{ 
			var serializer = new RectValueSerializer ();
			var rect = new Rect (1.234, 2.678, 3.123, 4.567);
			var obj = serializer.ConvertFromString (serializer.ConvertToString (rect, null), null);
			Assert.AreEqual (rect, obj);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringHasInvalidFormat ()
		{
			var serializer = new RectValueSerializer ();
			serializer.ConvertFromString ("a,b,c,d", null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringIsNull ()
		{
			var serializer = new RectValueSerializer ();
			serializer.ConvertFromString (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToStringShouldThrowExceptionWhenInvalidType ()
		{
			var serializer = new RectValueSerializer ();
			serializer.ConvertToString (10, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertToStringShouldThrowExceptionWhenHeightOrWidthIsNegative ()
		{
			var serializer = new RectValueSerializer ();
			var result = serializer.ConvertFromString ("1,2,-1,-2", null);
		}
	}

}
