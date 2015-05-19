//
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//      Chris Toshok (toshok@ximian.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Remoting;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.Design
{
	[TestFixture]
	public class ControlBindingsConverterTest : TestHelper
	{

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void TestProperties ()
		{
			Control c = new Control ();
			ControlBindingsCollection col = c.DataBindings;

			TypeConverter cvt = TypeDescriptor.GetConverter (col);

			Assert.IsNotNull (cvt, "1");

			Assert.IsTrue (cvt.GetPropertiesSupported (null), "2");
			
			PropertyDescriptorCollection props = cvt.GetProperties (null, col, null);
			
			Assert.AreEqual (3, props.Count, "3");

			Assert.AreEqual ("Tag", props[0].Name, "4");
			Console.WriteLine (props[0].GetType());
			Console.WriteLine ("tag value = {0}", props[0].GetValue (col));
			Console.WriteLine ("tag converter = {0}", props[0].Converter);
			Console.WriteLine ("tag localizable = {0}", props[0].IsLocalizable);
			Console.WriteLine ("tag readonly = {0}", props[0].IsReadOnly);
			Console.WriteLine ("tag type = {0}", props[0].PropertyType);
			Console.WriteLine ("tag category = {0}", props[0].Category);
			Console.WriteLine ("tag description = {0}", props[0].Description);
			Console.WriteLine ("tag displaynem = {0}", props[0].DisplayName);
			Console.WriteLine ("tag has {0} attributes", props[0].Attributes.Count);

			Assert.AreEqual ("Text", props[1].Name, "5");
			Console.WriteLine (props[1].GetType());
			Console.WriteLine ("text value = {0}", props[1].GetValue (col));
			Console.WriteLine ("text converter = {0}", props[1].Converter);
			Console.WriteLine ("text localizable = {0}", props[1].IsLocalizable);
			Console.WriteLine ("text readonly = {0}", props[1].IsReadOnly);
			Console.WriteLine ("text type = {0}", props[1].PropertyType);
			Console.WriteLine ("text category = {0}", props[1].Category);
			Console.WriteLine ("text description = {0}", props[1].Description);
			Console.WriteLine ("text displaynem = {0}", props[1].DisplayName);
			Console.WriteLine ("text has {0} attributes", props[1].Attributes.Count);

			Assert.AreEqual ("(Advanced)", props[2].Name, "6");
			Console.WriteLine (props[2].GetType());
			Console.WriteLine ("advanced value = {0}", props[2].GetValue (col));
			TypeConverter propcvt = props[2].Converter;
			Console.WriteLine ("advanced converter = {0}", propcvt.GetType());
			Console.WriteLine ("");
			if (null == propcvt.GetProperties(props[2].GetValue (col)))
				Console.WriteLine ("null properties");
			else
				Console.WriteLine ("  {0} properties", propcvt.GetProperties(props[2].GetValue (col)).Count);
			Console.WriteLine ("advanced converter = {0}/{1}/{2}",
					   propcvt.GetPropertiesSupported (),
					   propcvt.GetStandardValuesSupported (),
					   propcvt.GetCreateInstanceSupported ());
			Console.WriteLine ("advanced localizable = {0}", props[2].IsLocalizable);
			Console.WriteLine ("advanced readonly = {0}", props[2].IsReadOnly);
			Console.WriteLine ("advanced type = {0}", props[2].PropertyType);
			Console.WriteLine ("advanced category = {0}", props[2].Category);
			Console.WriteLine ("advanced description = {0}", props[2].Description);
			Console.WriteLine ("advanced displaynem = {0}", props[2].DisplayName);
			Console.WriteLine ("advanced has {0} attributes", props[2].Attributes.Count);
			foreach (Attribute a in props[2].Attributes) {
				Console.WriteLine ("   attribute =  {0}", a);
			}
		}
	}
}
