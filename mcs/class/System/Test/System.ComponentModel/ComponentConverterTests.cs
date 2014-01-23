//
// System.ComponentModel.TypeConverter test cases
//
// Authors:
// 	Marek Habersack (mhabersack@novell.com)
//
// (c) 2008 Novell, Inc. (http://novell.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class ComponentConverterTests
	{
		[Test]
		[NUnit.Framework.Category ("MobileNotWorking")] // IComponent doesn't have the TypeConverter attribute
		public void DataSetConversions ()
		{
			TypeConverter converter = TypeDescriptor.GetConverter (typeof (DataSet));
			Assert.AreEqual (typeof (ComponentConverter), converter != null ? converter.GetType () : null, "A1");

			DataSet ds = new DataSet ();
			string s = (string) converter.ConvertTo (null, CultureInfo.InvariantCulture, ds, typeof (string));
			Assert.AreEqual (String.Empty, s, "A2");

			object obj = converter.ConvertFrom (null, CultureInfo.InvariantCulture, s);
			Assert.IsNull (obj, "A3");
		}
	}
}
