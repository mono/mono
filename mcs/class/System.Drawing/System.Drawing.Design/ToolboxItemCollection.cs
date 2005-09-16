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

//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Security.Permissions;

namespace System.Drawing.Design
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class ToolboxItemCollection : ReadOnlyCollectionBase
	{

		public ToolboxItemCollection (ToolboxItem[] value) : base()
		{
			InnerList.AddRange (value);
		}

		public ToolboxItemCollection (ToolboxItemCollection value) : base()
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
