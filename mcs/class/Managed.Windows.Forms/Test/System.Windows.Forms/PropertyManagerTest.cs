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
// Authors:
//	Chris Toshok	toshok@ximian.com


using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class PropertyManagerTest {

		class TestClass {
			int prop;
			public TestClass ()
			{
				prop = 0;
			}

			public int Property {
				get { return prop; }
				set {
					prop = value;
					if (PropertyChanged != null)
						PropertyChanged (this, EventArgs.Empty);
				}
			}

			public event EventHandler PropertyChanged;
		}

		bool currentChangedRaised;

		void OnCurrentChanged (object sender, EventArgs args)
		{
			currentChangedRaised = true;
		}

		[Test]
		public void TestEvent ()
		{
			TestClass test = new TestClass();
			BindingContext bc = new BindingContext ();

			BindingManagerBase bm = bc[test, "Property"];
			Assert.IsTrue (typeof (PropertyManager).IsAssignableFrom (bm.GetType()), "A1");

			bm.CurrentChanged += new EventHandler (OnCurrentChanged);

			currentChangedRaised = false;
			test.Property = 5;
			Assert.IsTrue (currentChangedRaised, "A2");
		}
	}

}
