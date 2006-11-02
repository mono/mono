//
// Tests for System.Web.UI.WebControls.ValidatedControlConverterTest.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
//

using NUnit.Framework;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Web;
using ComponentSpace = System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class ValidatedControlConverterTest {
		public class ControlContainer : IContainer {
			ComponentCollection	col;

			public ControlContainer(ICollection collection) {
				Control[] controls = new Control[collection.Count];
				int i;

				i = 0;
				foreach(Control c in collection) {
					controls[i++] = c;
				}

				col = new ComponentCollection(controls);
			}

			public ComponentCollection Components {
				get { return col; }
			}

			public void Remove(IComponent component) { }
			public void Add(IComponent component, string name) { }
			void ComponentSpace.IContainer.Add(IComponent component) { }
			public void Dispose() { }
		}

		public class ControlTypeDescriptorContext : ITypeDescriptorContext {
			ControlContainer	cc;

			public ControlTypeDescriptorContext (ICollection collection) {
				cc = new ControlContainer(collection);
			}


			public IContainer Container {
				get {
					return cc;
				}
			}

			public void OnComponentChanged() { }
			public bool OnComponentChanging() { return false; }
			public object Instance { get { return null; } }
			public PropertyDescriptor PropertyDescriptor { get { return null; } }
			public object GetService(Type serviceType) {  return null; }
		}

		public class NamingContainer : WebControl, INamingContainer {

		}

		[Test]
        [NUnit.Framework.Category("NotWorking")]
		public void Basic () {
			string[]				result;
			int					i;
			ValidatedControlConverter		conv;
			TypeConverter.StandardValuesCollection	values;
			NamingContainer				container;
			TextBox			ctl1, ctl2;
			DropDownList				ddl;
			Button					btn;
			ControlTypeDescriptorContext		context;

			container = new NamingContainer ();
			ctl1 = new TextBox ();
			ctl2 = new TextBox ();
			ddl = new DropDownList();

			// Button has no ValidationProperty and will not show in the list
			btn = new Button();

			container.Controls.Add (ctl1);
			container.Controls.Add (ctl2);
			container.Controls.Add (btn);
			container.Controls.Add (ddl);
			
			container.ID = "naming";
			ctl1.ID = "fooid";
			ctl2.ID = "blahid";
			ddl.ID = "ddlid";
			btn.ID = "buttonid";

			context = new ControlTypeDescriptorContext(container.Controls);
			conv = new ValidatedControlConverter();

			values = conv.GetStandardValues(context);
#if NET_2_0
			Assert.IsNull (values, "B1");
#else
			Assert.AreEqual(3, values.Count, "B1");

			result = new string[values.Count];
			i = 0;
			foreach (string s in values) {
				result[i++] = s;
			}

			Assert.AreEqual(new string[] { "blahid", "ddlid", "fooid"}, result, "B2");	// Alphabetical?
			Assert.AreEqual(false, conv.GetStandardValuesExclusive(null), "B3");
			Assert.AreEqual(true, conv.GetStandardValuesSupported(null), "B4");
			Assert.AreEqual(null, conv.GetStandardValues(null), "B5");
#endif
		}
	}
}
