//
// System.ComponentModel.ArrayConverter test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2006 Gert Driesen
//

using System;
using System.ComponentModel;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class ArrayConverterTests
	{
		private ArrayConverter converter;
		
		[SetUp]
		public void SetUp ()
		{
			converter = new ArrayConverter ();
		}

		[Test]
		public void ConvertTo ()
		{
			int [] numbers = new int [] { 5, 7 };
			string text = (string) converter.ConvertTo (null, CultureInfo.InvariantCulture,
				numbers, typeof (string));
			Assert.AreEqual ("Int32[] Array", text);
		}

		[Test]
		public void ConvertTo_DestinationType_Null ()
		{
			int[] numbers = new int[] { 5, 7 };

			try {
				converter.ConvertTo (null, CultureInfo.InvariantCulture,
					numbers, (Type) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual ("destinationType", ex.ParamName, "#2");
			}
		}

		[Test]
		public void GetProperties ()
		{
			int [] numbers = new int [] { 5, 7 };
			PropertyDescriptorCollection pds = converter.GetProperties (null,
				numbers, null);
			Assert.IsNotNull (pds, "#A1");
			Assert.AreEqual (2, pds.Count, "#A2");

			PropertyDescriptor pd = pds [0];
			Assert.AreEqual (numbers.GetType (), pd.ComponentType, "#B1");
			Assert.AreEqual (false, pd.IsReadOnly, "#B2");
			Assert.AreEqual ("[0]", pd.Name, "#B3");
			Assert.AreEqual (typeof (int), pd.PropertyType, "#B4");
			Assert.IsFalse (pd.CanResetValue (numbers), "#B5");
			Assert.AreEqual (5, pd.GetValue (numbers), "#B6");
			pd.SetValue (numbers, 9);
			Assert.AreEqual (9, pd.GetValue (numbers), "#B7");
			pd.ResetValue (numbers);
			Assert.AreEqual (9, pd.GetValue (numbers), "#B8");
			Assert.IsFalse (pd.ShouldSerializeValue (numbers), "#B9");

			pd = pds [1];
			Assert.AreEqual (numbers.GetType (), pd.ComponentType, "#C1");
			Assert.AreEqual (false, pd.IsReadOnly, "#C2");
			Assert.AreEqual ("[1]", pd.Name, "#C3");
			Assert.AreEqual (typeof (int), pd.PropertyType, "#C4");
			Assert.IsFalse (pd.CanResetValue (numbers), "#C5");
			Assert.AreEqual (7, pd.GetValue (numbers), "#C6");
			pd.SetValue (numbers, 3);
			Assert.AreEqual (3, pd.GetValue (numbers), "#C7");
			pd.ResetValue (numbers);
			Assert.AreEqual (3, pd.GetValue (numbers), "#C8");
			Assert.IsFalse (pd.ShouldSerializeValue (numbers), "#C9");
		}

		[Test]
		public void GetProperties_Value_Null ()
		{
			converter.GetProperties (null, null, null);
		}

		[Test]
		public void GetPropertiesSupported ()
		{
			Assert.IsTrue (converter.GetPropertiesSupported (null));
		}
	}
}
