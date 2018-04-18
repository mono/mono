using System;
using System.Windows;
using System.Windows.Converters;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class PointValueSerializerTest
	{
		[Test]
		public void CanConvertFromString ()
		{
			var serializer = new PointValueSerializer ();
			Assert.IsTrue (serializer.CanConvertFromString ("", null));
		}

		[Test]
		public void CanConvertToString ()
		{
			var serializer = new PointValueSerializer ();
			Assert.IsTrue (serializer.CanConvertToString (new Point (0, 0), null));
			Assert.IsFalse (serializer.CanConvertToString ("", null));
		}

		[Test]
		public void ConvertFromString ()
		{
			var serializer = new PointValueSerializer ();
			object obj = serializer.ConvertFromString ("3.14,4.15", null);
			Assert.AreEqual (typeof (Point), obj.GetType ());
			Assert.AreEqual (new Point (3.14, 4.15), obj);
		}

		[Test]
		public void RoundTripConvert()
		{ 
			var serializer = new PointValueSerializer ();
			var Point = new Point (1.234, 2.678);
			var obj = serializer.ConvertFromString (serializer.ConvertToString (Point, null), null);
			Assert.AreEqual (Point, obj);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringHasInvalidFormat ()
		{
			var serializer = new PointValueSerializer ();
			serializer.ConvertFromString ("a,b", null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringIsNull ()
		{
			var serializer = new PointValueSerializer ();
			serializer.ConvertFromString (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToStringShouldThrowExceptionWhenInvalidType ()
		{
			var serializer = new PointValueSerializer ();
			serializer.ConvertToString (10, null);
		}
	}

}
