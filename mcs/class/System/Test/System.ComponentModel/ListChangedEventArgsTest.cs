//
// System.ComponentModel.TypeConverter test cases
//
// Authors:
// 	Jonathan Pryor <jpryor@novell.com>
//
// (c) 2009 Novell, Inc. (http://novell.com)
//

using System;
using System.ComponentModel;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class ListChangedEventArgsTests
	{
		[Test]
		public void Ctor_ListChangedType_NewIndex()
		{
			ListChangedEventArgs e = new ListChangedEventArgs (ListChangedType.ItemAdded, 0);
			Assert.AreEqual (ListChangedType.ItemAdded, e.ListChangedType);
			Assert.AreEqual (0,   e.NewIndex);
			Assert.AreEqual (-1,  e.OldIndex);
		}

		[Test]
		public void Ctor_ListChangedType_NewIndex_OldIndex()
		{
			ListChangedEventArgs e = new ListChangedEventArgs (ListChangedType.ItemMoved, 1, 2);
			Assert.AreEqual (ListChangedType.ItemMoved, e.ListChangedType);
			Assert.AreEqual (1,  e.NewIndex);
			Assert.AreEqual (2,  e.OldIndex);
		}

		[Test]
		public void Ctor_ListChangedType_PropDesc()
		{
			PropertyDescriptor   p = null;
			ListChangedEventArgs e = new ListChangedEventArgs (ListChangedType.ItemMoved, p);
			Assert.AreEqual (ListChangedType.ItemMoved, e.ListChangedType);
			Assert.AreEqual (0,  e.NewIndex);
			Assert.AreEqual (0,  e.OldIndex);
		}

#if NET_2_0
		[Test]
		public void Ctor_ListChangedType_NewIndex_PropDesc()
		{
			PropertyDescriptor   p = null;
			ListChangedEventArgs e = new ListChangedEventArgs (ListChangedType.ItemMoved, 2, p);
			Assert.AreEqual (ListChangedType.ItemMoved, e.ListChangedType);
			Assert.AreEqual (2,  e.NewIndex);
			Assert.AreEqual (2,  e.OldIndex);
		}
#endif // NET_2_0
	}
}
