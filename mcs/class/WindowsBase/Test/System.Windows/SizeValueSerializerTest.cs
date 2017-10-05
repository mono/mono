using System;
using System.Windows;
using System.Windows.Converters;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class SizeValueSerializerTest
	{
		[Test]
		public void CanConvertFromString ()
		{
			var serializer = new SizeValueSerializer ();
			Assert.IsTrue (serializer.CanConvertFromString ("", null));
		}

		[Test]
		public void CanConvertToString ()
		{
			var serializer = new SizeValueSerializer ();
			Assert.IsTrue (serializer.CanConvertToString (new Size (0, 0), null));
			Assert.IsFalse (serializer.CanConvertToString ("", null));
		}

		[Test]
		public void ConvertFromString ()
		{
			var serializer = new SizeValueSerializer ();
			object obj = serializer.ConvertFromString ("3,4", null);
			Assert.AreEqual (typeof (Size), obj.GetType ());
			Assert.AreEqual (new Size (3, 4), obj);
		}

		[Test]
		public void RoundTripConvert()
		{
			var serializer = new SizeValueSerializer ();
			var size = new Size (1.234, 5.678);
			var obj = serializer.ConvertFromString (serializer.ConvertToString (size, null), null);
			Assert.AreEqual (size, obj);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringHasInvalidFormat ()
		{
			var serializer = new SizeValueSerializer ();
			serializer.ConvertFromString ("a,b", null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringIsNull ()
		{
			var serializer = new SizeValueSerializer ();
			serializer.ConvertFromString (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToStringShouldThrowExceptionWhenInvalidType ()
		{
			var serializer = new SizeValueSerializer ();
			serializer.ConvertToString (10, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertToStringShouldThrowExceptionWhenHeightOrWidthIsNegative ()
		{
			var serializer = new SizeValueSerializer ();
			var result = serializer.ConvertFromString ("-1,-4", null);
		}
	}

}
