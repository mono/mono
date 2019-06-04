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

	class TestDepObj : DependencyObject {
		public static readonly DependencyProperty TestProp1 = DependencyProperty.Register ("property1", typeof (string), typeof (TestDepObj));
		public static readonly DependencyProperty TestProp2 = DependencyProperty.Register ("property2", typeof (string), typeof (TestDepObj));
		public static readonly DependencyProperty TestProp3 = DependencyProperty.Register ("property3", typeof (string), typeof (TestDepObj));

		public static readonly DependencyProperty TestProp4 = DependencyProperty.Register ("property4", typeof (string), typeof (TestDepObj), new PropertyMetadata ("default", changed, coerce));

		static void changed (DependencyObject d, DependencyPropertyChangedEventArgs e) { }
		static object coerce (DependencyObject d, object baseValue) { return baseValue; }
	}

	class TestSubclass : TestDepObj {
	}
	

	public class PropertyMetadataPoker : PropertyMetadata {

		public bool BaseIsSealed {
			get { return base.IsSealed; }
		}

		public void CallApply ()
		{
			OnApply (TestDepObj.TestProp1, typeof (TestDepObj));
		}

		public void CallMerge (PropertyMetadata baseMetadata, DependencyProperty dp)
		{
			Merge (baseMetadata, dp);
		}

		protected override void Merge (PropertyMetadata baseMetadata, DependencyProperty dp)
		{
//			Console.WriteLine (Environment.StackTrace);
			base.Merge (baseMetadata, dp);
		}

		protected override void OnApply (DependencyProperty dp, Type targetType)
		{
			base.OnApply (dp, targetType);
//			Console.WriteLine ("IsSealed in OnApply? {0}", IsSealed);
//			Console.WriteLine (Environment.StackTrace);
		}
	}


	[TestFixture]
	public class PropertyMetadataTest {

		[Test]
		public void DefaultValues ()
		{
			PropertyMetadataPoker m = new PropertyMetadataPoker ();
			Assert.AreEqual (null, m.DefaultValue);
			Assert.AreEqual (null, m.PropertyChangedCallback);
			Assert.AreEqual (null, m.CoerceValueCallback);
		}

		[Test]
		public void IsSealed ()
		{
			PropertyMetadataPoker m;

			Console.WriteLine (1);
			// calling OnApply isn't what sets the metadata to be sealed
			m = new PropertyMetadataPoker();
			Assert.IsFalse (m.BaseIsSealed);
			m.CallApply ();
			Assert.IsFalse (m.BaseIsSealed);

			Console.WriteLine (2);
			// calling OverrideMetadata does, however
			m = new PropertyMetadataPoker ();
			TestDepObj.TestProp1.OverrideMetadata (typeof (TestSubclass), m);
			Assert.IsTrue (m.BaseIsSealed);

			Console.WriteLine (3);
			// calling DependencyProperty.AddOwner does too, but only because it calls OverrideMetadata
			m = new PropertyMetadataPoker ();
			TestDepObj.TestProp2.AddOwner (typeof (TestSubclass), m);
			Assert.IsTrue (m.BaseIsSealed);

			Console.WriteLine (4);
			// lastly, calling DependencyProperty.Register does.
			m = new PropertyMetadataPoker ();
			DependencyProperty.Register ("xxx", typeof (string), typeof (TestDepObj), m);
			Assert.IsTrue (m.BaseIsSealed);
		}

		[Test]
		public void TestAddOwnerResult()
		{
			PropertyMetadataPoker m = new PropertyMetadataPoker ();
			DependencyProperty p = TestDepObj.TestProp3.AddOwner (typeof (TestSubclass), m);

			// they're the same object
			Assert.AreSame (p, TestDepObj.TestProp3);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyAfterSealed1 ()
		{
			PropertyMetadataPoker m = new PropertyMetadataPoker ();
			DependencyProperty.Register ("p1", typeof (string), typeof (TestDepObj), m);
			Assert.IsTrue (m.BaseIsSealed);

			m.CoerceValueCallback = null;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyAfterSealed2 ()
		{
			PropertyMetadataPoker m = new PropertyMetadataPoker ();
			DependencyProperty.Register ("p2", typeof (string), typeof (TestDepObj), m);
			Assert.IsTrue (m.BaseIsSealed);

			m.PropertyChangedCallback = null;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ModifyAfterSealed3 ()
		{
			PropertyMetadataPoker m = new PropertyMetadataPoker ();
			DependencyProperty.Register ("p3", typeof (string), typeof (TestDepObj), m);
			Assert.IsTrue (m.BaseIsSealed);

			m.DefaultValue = "hi";
		}

		[Test]
		public void TestMerge ()
		{
			PropertyMetadataPoker m = new PropertyMetadataPoker ();
			m.CallMerge (TestDepObj.TestProp4.GetMetadata (typeof (TestDepObj)), TestDepObj.TestProp4);
			Assert.AreEqual ("default", m.DefaultValue);
			Assert.IsNotNull (m.CoerceValueCallback);
			Assert.IsNotNull (m.PropertyChangedCallback);

			m = new PropertyMetadataPoker ();
			m.DefaultValue = "non-default";
			m.CallMerge (TestDepObj.TestProp4.GetMetadata (typeof (TestDepObj)), TestDepObj.TestProp4);
			Assert.AreEqual ("non-default", m.DefaultValue);
			Assert.IsNotNull (m.CoerceValueCallback);
			Assert.IsNotNull (m.PropertyChangedCallback);

			// XXX should check overriding of coerce and
			// property changed callbacks, but we'll trust
			// they behave the same..
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // "Default value cannot be 'Unset'."
		public void TestSetDefaultToUnsetValue ()
		{
			PropertyMetadata m = new PropertyMetadata ();
			m.DefaultValue = DependencyProperty.UnsetValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // "Default value cannot be 'Unset'."
		public void TestInitDefaultToUnsetValue ()
		{
			new PropertyMetadata (DependencyProperty.UnsetValue);
		}
	}

}
