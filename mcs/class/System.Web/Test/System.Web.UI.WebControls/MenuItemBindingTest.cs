//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com)
//
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;
#if NET_4_0
namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class MenuItemBindingTest
	{
		const string TO_STRING_EMPTY_VALUE = "(Empty)";

		static readonly SortedDictionary <string, string> toStringValues = new SortedDictionary<string, string> (StringComparer.OrdinalIgnoreCase) {
			{"DataMember", "value"},
			{"Depth", TO_STRING_EMPTY_VALUE},
			{"Enabled", TO_STRING_EMPTY_VALUE},
			{"EnabledField", TO_STRING_EMPTY_VALUE},
			{"FormatString", TO_STRING_EMPTY_VALUE},
			{"ImageUrl", TO_STRING_EMPTY_VALUE},
			{"ImageUrlField", TO_STRING_EMPTY_VALUE},
			{"NavigateUrl", TO_STRING_EMPTY_VALUE},
			{"Selectable", TO_STRING_EMPTY_VALUE},
			{"SelectableField", TO_STRING_EMPTY_VALUE},
			{"Target", TO_STRING_EMPTY_VALUE},
			{"TargetField", TO_STRING_EMPTY_VALUE},
			{"Text", TO_STRING_EMPTY_VALUE},
			{"TextField", TO_STRING_EMPTY_VALUE},
			{"ToolTip", TO_STRING_EMPTY_VALUE},
			{"ToolTipField", TO_STRING_EMPTY_VALUE},
			{"Value", TO_STRING_EMPTY_VALUE},
			{"ValueField", TO_STRING_EMPTY_VALUE},
			{"PopOutImageUrl", TO_STRING_EMPTY_VALUE},
			{"PopOutImageUrlField", TO_STRING_EMPTY_VALUE},
			{"SeparatorImageUrl", TO_STRING_EMPTY_VALUE},
			{"SeparatorImageUrlField", TO_STRING_EMPTY_VALUE}
		};

		[Test]
		public void Test_ToString ()
		{
			var mib = new MenuItemBinding ();

			Assert.AreEqual ("(Empty)", mib.ToString (), "#A1");
			foreach (var entry in toStringValues)
				ToStringTestProperty (entry.Key, entry.Value);
		}

		void ToStringTestProperty (string propertyName, string expectedValue)
		{
			PropertyInfo pi = typeof (MenuItemBinding).GetProperty (propertyName, BindingFlags.Instance | BindingFlags.Public);
			if (pi == null)
				Assert.Fail ("Property '{0}' not found.", propertyName);

			object defaultValue = null;
			object[] attrs = pi.GetCustomAttributes (typeof (DefaultValueAttribute), false);
			Type t = pi.PropertyType;
			if (attrs != null && attrs.Length > 0) {
				var dva = attrs [0] as DefaultValueAttribute;
				defaultValue = dva.Value;
			} else {
				if (t == typeof (string))
					defaultValue = String.Empty;
				else if (t == typeof (bool))
					defaultValue = false;
				else if (t == typeof (int))
					defaultValue = Int32.MaxValue;
				else
					Assert.Fail ("Unsupported return type '{0}' for property '{1}'", t.FullName, propertyName);
			}

			object setToValue = null;
			if (t == typeof (string)) {
				string v = defaultValue as String;
				if (v == String.Empty || v != "value")
					setToValue = "value";
				else
					setToValue = "value123";
			} else if (t == typeof (bool)) {
				bool v = (bool) defaultValue;
				if (v)
					setToValue = false;
				else
					setToValue = true;
			} else if (t == typeof (int)) {
				int v = (int) defaultValue;
				if (v == Int32.MaxValue)
					v = Int32.MinValue;
				else
					v = Int32.MaxValue;
			} else
				Assert.Fail ("Unsupported return type '{0}' for property '{1}'", t.FullName, propertyName);

			var mib = new MenuItemBinding ();
			pi.SetValue (mib, setToValue, null);

			Assert.AreEqual (expectedValue, mib.ToString (), propertyName);
		}
	}
}
#endif