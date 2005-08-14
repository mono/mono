using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class ColorConverterFixture
	{
		Color col;
		Color colnamed;
		ColorConverter colconv;
		String colStr;
		String colStrInvariant;
		String colnamedStr;

		[SetUp]
		public void SetUp () {
			col = Color.FromArgb (10, 20, 30);
			colStr = string.Format ("10{0} 20{0} 30", CultureInfo.CurrentCulture.TextInfo.ListSeparator);
			colStrInvariant = string.Format ("10{0} 20{0} 30", CultureInfo.InvariantCulture.TextInfo.ListSeparator);

			colnamed = Color.ForestGreen;
			colnamedStr = "ForestGreen";

			colconv = (ColorConverter) TypeDescriptor.GetConverter (col);
		}

		[Test]
		public void CanConvertFrom () {
			Assert.IsTrue (colconv.CanConvertFrom (typeof (String)), "CCF#1");
			Assert.IsTrue (colconv.CanConvertFrom (null, typeof (String)), "CCF#1a");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (Rectangle)), "CCF#2");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (RectangleF)), "CCF#3");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (Point)), "CCF#4");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (PointF)), "CCF#5");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (Color)), "CCF#6");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (SizeF)), "CCF#7");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (Object)), "CCF#8");
			Assert.IsTrue (! colconv.CanConvertFrom (null, typeof (int)), "CCF#9");
		}

		[Test]
		public void CanConvertTo () {
			Assert.IsTrue (colconv.CanConvertTo (typeof (String)), "CCT#1");
			Assert.IsTrue (colconv.CanConvertTo (null, typeof (String)), "CCT#1a");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (Rectangle)), "CCT#2");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (RectangleF)), "CCT#3");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (Point)), "CCT#4");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (PointF)), "CCT#5");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (Color)), "CCT#6");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (SizeF)), "CCT#7");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (Object)), "CCT#8");
			Assert.IsTrue (! colconv.CanConvertTo (null, typeof (int)), "CCT#9");
			Assert.IsTrue (colconv.CanConvertTo (typeof (InstanceDescriptor)), "CCT#10");
		}

		[Test]
		public void ConvertFrom ()
		{
			Assert.AreEqual (col, (Color) colconv.ConvertFrom (null,
				CultureInfo.InvariantCulture, colStrInvariant), "CF#1");
			Assert.AreEqual (colnamed, (Color) colconv.ConvertFrom (null,
				CultureInfo.InvariantCulture, colnamedStr), "CF#2");

			Assert.AreEqual (Color.Empty, colconv.ConvertFrom (string.Empty), "CF#3");
			Assert.AreEqual (Color.Empty, colconv.ConvertFrom (" "), "CF#4");
			Assert.AreEqual (Color.Red, colconv.ConvertFrom ("Red"), "CF#5");
			Assert.AreEqual (Color.Red, colconv.ConvertFrom (" Red "), "CF#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFrom_x1 ()
		{
				colconv.ConvertFrom (null, CultureInfo.InvariantCulture, "10, 20");
		}


		[Test]
		//[ExpectedException (typeof (ArgumentException))]
		//use try-catch, because dotnet throws ArgumentException and 
		//mono throws ArgumentOutOfRangeException
		public void ConvertFrom_x2 ()
		{
			try {
				colconv.ConvertFrom ("-10, 20, 30");
				Assert.Fail ("ArgumentException was expected");
			}
			catch (ArgumentException) {
				Assert.IsTrue (true);
			}
		}


		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFrom_x3 ()
		{
			colconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					"1, 1, 1, 1, 1");
		}


		[Test]
		//[ExpectedException (typeof (Exception))]
		//use try-catch, because dotnet throws Exception and 
		//mono throws ArgumentException
		public void ConvertFrom_x4 ()
		{
			try {
				colconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					"*1, 1");
				Assert.Fail ("Exception was expected");
			}
			catch (Exception) {
				Assert.IsTrue (true);
			}
		}


		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_x5 ()
		{
			colconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new Point (10, 10));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_x6 ()
		{
			colconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new PointF (10, 10));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_x7 ()
		{
			colconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new Size (10, 10));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_x8 ()
		{
			colconv.ConvertFrom (null, CultureInfo.InvariantCulture,
					new SizeF (10, 10));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_x9 ()
		{
			colconv.ConvertFrom (null, CultureInfo.InvariantCulture, 0x10);
		}

		[Test]
		public void ConvertTo ()
		{
			Assert.AreEqual (colStrInvariant, colconv.ConvertTo (null, CultureInfo.InvariantCulture,
				Color.FromArgb (10, 20, 30), typeof (String)), "CT#1");
			Assert.AreEqual (colStrInvariant, colconv.ConvertTo (null, CultureInfo.InvariantCulture,
				Color.FromArgb (255, 10, 20, 30), typeof (String)), "CT#2");
			Assert.AreEqual ("10, 20, 30, 40", colconv.ConvertTo (null, CultureInfo.InvariantCulture,
				Color.FromArgb (10, 20, 30, 40), typeof (String)), "CT#3");
			Assert.AreEqual (colnamedStr, colconv.ConvertTo (null, CultureInfo.InvariantCulture,
				colnamed, typeof (String)), "CT#4");

			Assert.AreEqual (string.Empty, colconv.ConvertTo (Color.Empty, typeof (string)), "CT#5");
			Assert.AreEqual ("Red", colconv.ConvertTo (Color.Red, typeof (string)), "CT#6");
			Assert.AreEqual (string.Empty, colconv.ConvertTo (null, typeof (string)), "CT#7");
			Assert.AreEqual ("test", colconv.ConvertTo ("test", typeof (string)), "CT#8");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_x1 ()
		{
			colconv.ConvertTo (null, CultureInfo.InvariantCulture, col,
					typeof (Color));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_x2 ()
		{
			colconv.ConvertTo (null, CultureInfo.InvariantCulture, col,
					typeof (SizeF));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_x3 ()
		{
			colconv.ConvertTo (null, CultureInfo.InvariantCulture, col,
					typeof (Point));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_x4 ()
		{
			colconv.ConvertTo (null, CultureInfo.InvariantCulture, col,
					typeof (PointF));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_x5 ()
		{
			colconv.ConvertTo (null, CultureInfo.InvariantCulture, col,
					typeof (int));
		}

		[Test]
		public void GetCreateInstanceSupported ()
		{
			Assert.IsTrue (!colconv.GetCreateInstanceSupported (), "GCIS#1");
			Assert.IsTrue (!colconv.GetCreateInstanceSupported (null), "GCIS#2");
		}

		[Test]
		public void CreateInstance ()
		{
			Color colInstance;

			Hashtable ht = new Hashtable ();
			ht.Add ("R", 10); ht.Add ("G", 20); ht.Add ("B", 30);

			Assert.AreEqual (null, colconv.CreateInstance (ht), "CI#1");

			ht.Add ("Name", "ForestGreen");

			Assert.AreEqual (null, colconv.CreateInstance (null, ht), "CI#2");
		}

		[Test]
		public void GetPropertiesSupported () {
			Assert.IsTrue (!colconv.GetPropertiesSupported (), "GPS#1");
			Assert.IsTrue (!colconv.GetPropertiesSupported (null), "GPS#2");
		}

		[Test]
		public void GetProperties () {
			Attribute [] attrs;

			Assert.AreEqual (null, colconv.GetProperties (col), "GP1#1");

			Assert.AreEqual (null, colconv.GetProperties (null, col, null), "GP2#1");

			attrs = Attribute.GetCustomAttributes (typeof (Color), true);
			Assert.AreEqual (null, colconv.GetProperties (null, col, attrs), "GP3#5");
		}

		[Test]
		public void ConvertFromInvariantString_string () {
			Assert.AreEqual (col, colconv.ConvertFromInvariantString (colStrInvariant), "CFISS#1");
			Assert.AreEqual (colnamed, colconv.ConvertFromInvariantString (colnamedStr), "CFISS#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFromInvariantString_string_exc_1 () {
			colconv.ConvertFromInvariantString ("1, 2, 3, 4, 5");
		}

		[Test]
		[NUnit.Framework.Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFromInvariantString_string_exc_2 () {
			colconv.ConvertFromInvariantString ("hello");
		}

		[Test]
		public void ConvertFromString_string () {
			Assert.AreEqual (col, colconv.ConvertFromString (colStr), "CFSS#1");
			Assert.AreEqual (colnamed, colconv.ConvertFromString (colnamedStr), "CFSS#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFromString_string_exc_1 () {
			colconv.ConvertFromString ("1, 2, 3, 4, 5");
		}

		[Test]
		[NUnit.Framework.Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFromString_string_exc_2 () {
			colconv.ConvertFromString ("hello");
		}

		[Test]
		public void ConvertToInvariantString_string () {
			Assert.AreEqual (colStrInvariant, colconv.ConvertToInvariantString (col), "CFISS#1");
			Assert.AreEqual (colnamedStr, colconv.ConvertToInvariantString (colnamed), "CFISS#2");
		}

		[Test]
		public void ConvertToString_string () {
			Assert.AreEqual (colStr, colconv.ConvertToString (col), "CFISS#1");
			Assert.AreEqual (colnamedStr, colconv.ConvertToString (colnamed), "CFISS#2");
		}

		[Test]
		public void GetStandardValuesSupported () {
			Assert.IsTrue (colconv.GetStandardValuesSupported ());
		}

		[Test]
		public void GetStandardValues () {
			Assert.AreEqual (167, colconv.GetStandardValues ().Count);
			Assert.AreEqual (167, colconv.GetStandardValues (null).Count);
		}

		[Test]
		public void GetStandardValuesExclusive () {
			Assert.AreEqual (false, colconv.GetStandardValuesExclusive ());
		}

	}
}

