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
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class DependencyPropertyTest {
		class ObjectPoker : DependencyObject {
			public static readonly DependencyProperty TestProp1 = DependencyProperty.Register ("property1", typeof (string), typeof (ObjectPoker));
		}

		class SubclassPoker : ObjectPoker {
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // "'p1' property was already registered by 'ObjectPoker'."
	        public void TestMultipleRegisters ()
		{
			DependencyProperty.Register ("p1", typeof (string), typeof (ObjectPoker));
			DependencyProperty.Register ("p1", typeof (string), typeof (ObjectPoker));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // "'property1' property was already registered by 'SubclassPoker'."
		public void TestMultipleAddOwner ()
		{
			ObjectPoker.TestProp1.AddOwner (typeof (SubclassPoker), new PropertyMetadata());
			ObjectPoker.TestProp1.AddOwner (typeof (SubclassPoker), new PropertyMetadata());
		}

		[Test]
		public void TestDefaultMetadata ()
		{
			DependencyProperty p;
			p = DependencyProperty.Register ("TestDefaultMetadata1", typeof (string), typeof (ObjectPoker));
			Assert.IsNotNull (p.DefaultMetadata);

			p = DependencyProperty.Register ("TestDefaultMetadata2", typeof (string), typeof (ObjectPoker), new PropertyMetadata ("hi"));
			Assert.IsNotNull (p.DefaultMetadata);
			Assert.AreEqual ("hi", p.DefaultMetadata.DefaultValue);
		}

		[Test]
		public void TestAddOwnerNullMetadata()
		{
			DependencyProperty p = DependencyProperty.Register ("TestAddOwnerNullMetadata", typeof (string), typeof (ObjectPoker));
			p.AddOwner (typeof (SubclassPoker), null);

			PropertyMetadata pm = p.GetMetadata (typeof (SubclassPoker));
			Assert.IsNotNull (pm);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestOverrideMetadataNullMetadata()
		{
			DependencyProperty p = DependencyProperty.Register ("TestOverrideMetadataNullMetadata", typeof (string), typeof (ObjectPoker));
			p.OverrideMetadata (typeof (SubclassPoker), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestOverrideMetadataNullType()
		{
			DependencyProperty p = DependencyProperty.Register ("TestOverrideMetadataNullType", typeof (string), typeof (ObjectPoker));
			p.OverrideMetadata (null, new PropertyMetadata());
		}

		[Test]
 		[ExpectedException (typeof (InvalidOperationException))]
		public void TestReadonlyOverrideMetadata ()
		{
			DependencyPropertyKey ro_key = DependencyProperty.RegisterReadOnly ("readonly-prop1",
											    typeof(double),
											    typeof(ObjectPoker),
											    new PropertyMetadata(double.NaN));
			ro_key.DependencyProperty.OverrideMetadata (typeof (SubclassPoker), new PropertyMetadataPoker());
		}

		[Test]
		public void TestReadonlyOverrideMetadataFromKey ()
		{
			DependencyPropertyKey ro_key = DependencyProperty.RegisterReadOnly ("readonly-prop2",
											    typeof(double),
											    typeof(ObjectPoker),
											    new PropertyMetadata(double.NaN));
			ro_key.OverrideMetadata (typeof (SubclassPoker), new PropertyMetadataPoker ());
		}
	}

}
