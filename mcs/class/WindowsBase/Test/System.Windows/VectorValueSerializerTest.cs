using System;
using System.Windows;
using System.Windows.Converters;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class VectorValueSerializerTest
	{
		[Test]
		public void CanConvertFromString ()
		{
			var serializer = new VectorValueSerializer ();
			Assert.IsTrue (serializer.CanConvertFromString ("", null));
		}

		[Test]
		public void CanConvertToString ()
		{
			var serializer = new VectorValueSerializer ();
			Assert.IsTrue (serializer.CanConvertToString (new Vector (0, 0), null));
			Assert.IsFalse (serializer.CanConvertToString ("", null));
		}

		[Test]
		public void ConvertFromString ()
		{
			var serializer = new VectorValueSerializer ();
			object obj = serializer.ConvertFromString ("3.14,4.15", null);
			Assert.AreEqual (typeof (Vector), obj.GetType ());
			Assert.AreEqual (new Vector (3.14, 4.15), obj);
		}

		[Test]
		public void RoundTripConvert()
		{ 
			var serializer = new VectorValueSerializer ();
			var Vector = new Vector (1.234, 2.678);
			var obj = serializer.ConvertFromString (serializer.ConvertToString (Vector, null), null);
			Assert.AreEqual (Vector, obj);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringHasInvalidFormat ()
		{
			var serializer = new VectorValueSerializer ();
			serializer.ConvertFromString ("a,b", null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromStringShouldThrowExceptionWhenStringIsNull ()
		{
			var serializer = new VectorValueSerializer ();
			serializer.ConvertFromString (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertToStringShouldThrowExceptionWhenInvalidType ()
		{
			var serializer = new VectorValueSerializer ();
			serializer.ConvertToString (10, null);
		}
	}

}
