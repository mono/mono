// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2006 Novell, Inc.
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using NUnit.Framework;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class PaddingConverterTest : TestHelper
	{
		[Test]
		public void CanConvertFrom ()
		{
			PaddingConverter c = new PaddingConverter ();

			Assert.IsTrue (c.CanConvertFrom (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (object)), "4");
		}

		[Test]
		public void CanConvertTo ()
		{
			PaddingConverter c = new PaddingConverter ();

			Assert.IsTrue (c.CanConvertTo (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertTo (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertTo (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertTo (null, typeof (object)), "4");
		}
		
		[Test]
		public void RoundTrip ()
		{
			Padding p1 = new Padding (1, 2, 3, 4);
			Padding p2 = new Padding (1);
			Padding p3 = new Padding ();

			Assert.AreEqual (p1, RoundTripPadding (p1), "B1");
			Assert.AreEqual (p2, RoundTripPadding (p2), "B2");
			Assert.AreEqual (p3, RoundTripPadding (p3), "B3");
			
		}

		[Test]
		public void ConvertFrom ()
		{
			string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			PaddingConverter pc = new PaddingConverter ();
			Assert.AreEqual (new Padding (1, 2, 3, 4), pc.ConvertFrom (
				string.Format ("1{0} 2{0} 3{0} 4", listSeparator)), "A1");
			Assert.AreEqual (new Padding (1, 2, 3, 4), pc.ConvertFrom (
				string.Format ("1{0}2{0}3{0}4", listSeparator)), "A2");
			Assert.AreEqual (new Padding (1, 2, 3, 4), pc.ConvertFrom (
				string.Format ("1{0}  2{0}  3{0}  4", listSeparator)), "A3");
			Assert.AreEqual (new Padding (1), pc.ConvertFrom (string.Format (
				"1{0} 1{0} 1{0} 1", listSeparator)), "A4");
			Assert.AreEqual (new Padding (), pc.ConvertFrom (string.Format (
				"0{0} 0{0} 0{0} 0", listSeparator)), "A5");
		}

		[Test]
		public void ConvertTo ()
		{
			PaddingConverter pc = new PaddingConverter ();

			Assert.AreEqual (string.Format (CultureInfo.CurrentCulture,
				"1{0} 2{0} 3{0} 4", CultureInfo.CurrentCulture.TextInfo.ListSeparator),
				(string) pc.ConvertTo (new Padding (1, 2, 3, 4), typeof (string)), "A1");
			Assert.AreEqual (string.Format (CultureInfo.CurrentCulture,
				"1{0} 1{0} 1{0} 1", CultureInfo.CurrentCulture.TextInfo.ListSeparator),
				(string) pc.ConvertTo (new Padding (1), typeof (string)), "A2");
			Assert.AreEqual (string.Format (CultureInfo.CurrentCulture,
				"0{0} 0{0} 0{0} 0", CultureInfo.CurrentCulture.TextInfo.ListSeparator),
				(string) pc.ConvertTo (Padding.Empty, typeof (string)), "A3");
		}

		private Padding RoundTripPadding (Padding p)
		{
			PaddingConverter pc = new PaddingConverter ();
			
			string s = (string)pc.ConvertTo (p, typeof (string));
			return (Padding)pc.ConvertFrom (s);
		}
		
		[Test]
		public void CreateInstanceSupported ()
		{
			PaddingConverter pc = new PaddingConverter ();
			
			Assert.AreEqual (true, pc.GetCreateInstanceSupported (null), "A1");
			Assert.AreEqual (true, pc.GetPropertiesSupported (null), "A2");
		}

		[Test]
		public void ConvertTo_InstanceDescriptor()
		{
			PaddingConverter c = new PaddingConverter();
			Padding originalPadding = new Padding (1, 10, 5, 9);
			InstanceDescriptor instanceDescriptor = (InstanceDescriptor) c.ConvertTo (originalPadding, 
										  typeof (InstanceDescriptor));
			Padding resultedPadding = (Padding) instanceDescriptor.Invoke ();
			Assert.AreEqual (originalPadding, resultedPadding, "#1");

			originalPadding = new Padding (99);
			instanceDescriptor = (InstanceDescriptor) c.ConvertTo (originalPadding, 
										  typeof (InstanceDescriptor));
			resultedPadding = (Padding) instanceDescriptor.Invoke ();
			Assert.AreEqual (originalPadding, resultedPadding, "#2");
		}

		#region FakeITypeDescriptorContext
		class FakeITypeDescriptorContext : ITypeDescriptorContext
		{
			// Only the Instance and PropertyDescriptor members are required for testing.
			//
			PropertyDescriptor propertyDescriptor;
			Object instance;

			internal FakeITypeDescriptorContext (PropertyDescriptor pd, object instance)
			{
				if (pd == null)
					throw new ArgumentNullException ("pd");
				if (instance == null)
					throw new ArgumentNullException ("instance");
				propertyDescriptor = pd;
				this.instance = instance;
			}

			#region ITypeDescriptorContext Members

			IContainer ITypeDescriptorContext.Container {
				get { throw new NotImplementedException (); }
			}

			object ITypeDescriptorContext.Instance {
				get { return instance; }
			}

			void ITypeDescriptorContext.OnComponentChanged ()
			{
				throw new NotImplementedException ();
			}

			bool ITypeDescriptorContext.OnComponentChanging ()
			{
				throw new NotImplementedException ();
			}

			PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor {
				get { return propertyDescriptor; }
			}

			#endregion

			#region IServiceProvider Members

			object IServiceProvider.GetService (Type serviceType)
			{
				throw new NotImplementedException ();
			}

			#endregion
		}
		#endregion

		class MyObjectWithMarginProperty
		{
			private Padding margin;

			public Padding Margin {
				get { return margin; }
				set { margin = value; }
			}
		}

		private static ITypeDescriptorContext GetTypeDescriptorContext (Padding paddingValue)
		{
			MyObjectWithMarginProperty obj = new MyObjectWithMarginProperty();
			obj.Margin = paddingValue;
			PropertyDescriptor pd = TypeDescriptor.GetProperties (obj)["Margin"];
			return new FakeITypeDescriptorContext (pd, obj);
		}

		private static Hashtable GetPropertiesTable (int all, int left, int top, int right, int bottom)
		{
			Hashtable newValues = new Hashtable();
			newValues.Add ("All", all);
			newValues.Add ("Left", left);
			newValues.Add ("Right", right);
			newValues.Add ("Top", top);
			newValues.Add ("Bottom", bottom);
			return newValues;
		}

		[Test]
		public void CreateInstance ()
		{
			PaddingConverter c = new PaddingConverter();
			Padding modified, expected;

			// Non-"All" Tests
			//
			ITypeDescriptorContext context = GetTypeDescriptorContext (new Padding (1, 2, 30, 40));

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (-1, 1, 2, 30, 40));
			expected = new Padding (1, 2, 30, 40);
			Assert.AreEqual (expected, modified, "NonAll_NoChange");

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (-1, 111, 2, 30, 40));
			expected = new Padding (111, 2, 30, 40);
			Assert.AreEqual (expected, modified, "NonAll_ChangeLeft");

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (-1, 1, 222, 30, 40));
			expected = new Padding (1, 222, 30, 40);
			Assert.AreEqual (expected, modified, "NonAll_ChangeTop");

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (555, 1, 2, 30, 40));
			expected = new Padding (555);
			Assert.AreEqual (expected, modified, "NonAll_ChangeAll");

			// "All" tests
			//
			context = GetTypeDescriptorContext (new Padding (1));

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (1, 1, 1, 1, 1));
			expected = new Padding (1, 1, 1, 1);
			Assert.AreEqual (expected, modified, "All_NoChange");

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (1, 111, 1, 1, 1));
			expected = new Padding (111, 1, 1, 1);
			Assert.AreEqual (expected, modified, "All_ChangeLeft");

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (1, 1, 222, 1, 1));
			expected = new Padding (1, 222, 1, 1);
			Assert.AreEqual (expected, modified, "All_ChangeTop");

			modified = (Padding) c.CreateInstance (context, GetPropertiesTable (555, 1, 1, 1, 1));
			expected = new Padding (555);
			Assert.AreEqual (expected, modified, "All_ChangeAll");
		}

		[Test]
		public void CreateInstance_NullArguments ()
		{
			PaddingConverter c = new PaddingConverter ();
			try {
				c.CreateInstance (null, GetPropertiesTable (1, 1, 1, 1, 1));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) { }
			try {
				c.CreateInstance (GetTypeDescriptorContext (Padding.Empty), null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException ex) { }
		}
	}
}
#endif
