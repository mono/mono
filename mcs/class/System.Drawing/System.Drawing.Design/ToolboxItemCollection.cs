//
// System.Drawing.Design.ToolboxItemCollection.cs
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Collections;

namespace System.Drawing.Design
{
	public sealed class ToolboxItemCollection : ReadOnlyCollectionBase
	{

		public ToolboxItemCollection (ToolboxItem[] value)
		{
			InnerList.AddRange (value);
		}

		public ToolboxItemCollection (ToolboxItemCollection value)
		{
			InnerList.AddRange (value);
		}

		public ToolboxItem this [int index] {
			get { return (ToolboxItem) InnerList[index]; }
		}

		public bool Contains (ToolboxItem value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (ToolboxItem[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (ToolboxItem value)
		{
			return InnerList.IndexOf (value);
		}
	}
}
