//
// System.ComponentModel.PropertyDescriptor test cases
//
// Authors:
// 	Chris Toshok (toshok@ximian.com)
//
// (c) 2006 Novell, Inc. (http://www.novell.com/)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class PropertyDescriptorTests
	{
		class ReadOnlyProperty_test
		{
			public int Prop {
				get { return 5; }
			}
		}

		class ReadOnlyAttribute_test
		{
			[ReadOnly (true)]
			public int Prop {
				get { return 5; }
				set { }
			}
		}

		class ConflictingReadOnly_test
		{
			[ReadOnly (false)]
			public int Prop {
				get { return 5; }
			}
		}

		class ShouldSerialize_public_test
		{
			public int Prop {
				get { return 5; }
			}

			public bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerialize_protected_test
		{
			public int Prop {
				get { return 5; }
			}

			protected bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerialize_private_test
		{
			public int Prop {
				get { return 5; }
			}

			private bool ShouldSerializeProp()
			{
				return false;
			}
		}

		class ShouldSerializeFalseEffectOnCanReset_test
		{
			public int Prop {
				get { return 5; }
				set { }
			}

			public bool ShouldSerializeProp()
			{
				return false;
			}

			public void ResetProp()
			{
			}
		}

		class NoSerializeOrResetProp_test
		{
			public int Prop {
				get { return 5; }
			}
		}

		class CanReset_public_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			public void ResetProp()
			{
				prop = 10;
			}
		}

		class CanReset_protected_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			protected void ResetProp()
			{
				prop = 10;
			}
		}

		class CanReset_private_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
				set { prop = value; }
			}

			private void ResetProp()
			{
				prop = 10;
			}
		}

		class CanResetNoSetter_test
		{
			int prop = 5;
			public int Prop {
				get { return prop; }
			}

			private void ResetProp()
			{
				prop = 10;
			}
		}

		[Test]
		public void ShouldSerializeTest_public ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_public_test))["Prop"];
			ShouldSerialize_public_test test = new ShouldSerialize_public_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_protected ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_protected_test))["Prop"];
			ShouldSerialize_protected_test test = new ShouldSerialize_protected_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void ShouldSerializeTest_private ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerialize_protected_test))["Prop"];
			ShouldSerialize_protected_test test = new ShouldSerialize_protected_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
		}

		[Test]
		public void CanResetTest_public ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_public_test))["Prop"];
			CanReset_public_test test = new CanReset_public_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTest_protected ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_protected_test))["Prop"];
			CanReset_protected_test test = new CanReset_protected_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTest_private ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanReset_private_test))["Prop"];
			CanReset_private_test test = new CanReset_private_test ();

			Assert.IsTrue (p.CanResetValue (test), "1");
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void CanResetTestNoSetterTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (CanResetNoSetter_test))["Prop"];
			CanResetNoSetter_test test = new CanResetNoSetter_test ();

#if NET_2_0
			Assert.IsFalse (p.CanResetValue (test), "1");
#else
			Assert.IsTrue (p.CanResetValue (test), "1");
#endif
			Assert.AreEqual (5, test.Prop, "2");
			p.ResetValue (test);
			Assert.AreEqual (10, test.Prop, "3");
		}

		[Test]
		public void NoSerializeOrResetPropTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (NoSerializeOrResetProp_test))["Prop"];
			NoSerializeOrResetProp_test test = new NoSerializeOrResetProp_test ();

			Assert.IsFalse (p.CanResetValue (test), "1");
			Assert.IsFalse (p.ShouldSerializeValue (test), "2");
		}

		[Test]
		public void ShouldSerializeFalseEffectOnCanResetTest ()
		{
			PropertyDescriptor p = TypeDescriptor.GetProperties (typeof (ShouldSerializeFalseEffectOnCanReset_test))["Prop"];
			ShouldSerializeFalseEffectOnCanReset_test test = new ShouldSerializeFalseEffectOnCanReset_test ();

			Assert.IsFalse (p.ShouldSerializeValue (test), "1");
			Assert.IsFalse (p.CanResetValue (test), "2");
		}

		[Test]
		public void ReadOnlyPropertyTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ReadOnlyProperty_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test]
		public void ReadOnlyAttributeTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ReadOnlyAttribute_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}

		[Test]
		public void ReadOnlyConflictingTest ()
		{
			PropertyDescriptorCollection col = TypeDescriptor.GetProperties (typeof (ConflictingReadOnly_test));
			Assert.IsTrue (col["Prop"].IsReadOnly, "1");
		}
	}
}
