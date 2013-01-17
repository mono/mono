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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Converters;
using System.Windows.Markup;
using NUnit.Framework;

namespace MonoTests.System.Windows.Markup {

	[TestFixture]
	public class ValueSerializerTest
	{
		class Context : IValueSerializerContext {
			public ValueSerializer GetValueSerializerFor (PropertyDescriptor descriptor)
			{
				Console.WriteLine ("GetValueSerializerFor ({0})", descriptor);
				return null;
			}

			public ValueSerializer GetValueSerializerFor (Type type)
			{
				Console.WriteLine ("GetValueSerializerFor ({0})", type);
				return null;
			}

			public void OnComponentChanged ()
			{
			}

			public bool OnComponentChanging ()
			{
				return false;
			}

			public IContainer Container {
				get { return null; }
			}

			public object Instance {
				get { return null; }
			}

			public PropertyDescriptor PropertyDescriptor {
				get { return null; }
			}

			public object GetService (Type serviceType)
			{
				return null;
			}
		}

		[Test]
#if NET_4_0
		[NUnit.Framework.CategoryAttribute ("NotWorking")]
		// Since ValueSerializer has moved to System.Xaml.dll while the type
		// this test expects is in WindowsBase, there should be some additional
		// support code in this assembly. Until someone does that job, this
		// test won't pass.
#endif
		public void GetSerializerForType ()
		{
			Assert.IsNull (ValueSerializer.GetSerializerFor (typeof (DependencyObject)));

			Assert.AreEqual ("VectorValueSerializer", ValueSerializer.GetSerializerFor (typeof (Vector)).GetType().Name);
			Assert.AreEqual ("Int32RectValueSerializer", ValueSerializer.GetSerializerFor (typeof (Int32Rect)).GetType().Name);
		}
	}
}

