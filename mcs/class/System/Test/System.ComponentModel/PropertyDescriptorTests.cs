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
