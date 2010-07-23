//
// System.Web.UI.WebControls.TreeNodeStyleCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class TreeNodeStyleCollection: StateManagedCollection
	{
		static Type[] types = new Type[] { typeof (TreeNodeStyle) };
		
		internal TreeNodeStyleCollection ()
		{
		}
		
		public int Add (TreeNodeStyle style)
		{
			// It looks weird, but it seems that there must be at least one style saved in the control state, or
			// otherwise tests fail. The line below makes sure that, whether or not modified by user, the Underline
			// property will always have unaltered value _and_ the style collection will contain at least one item.
			style.Font.Underline = style.Font.Underline;
			return ((IList)this).Add (style);
		}
		
		public bool Contains (TreeNodeStyle style)
		{
			return ((IList)this).Contains (style);
		}
		
		public void CopyTo (TreeNodeStyle[] array, int index)
		{
			((IList)this).CopyTo (array, index);
		}
		
		protected override object CreateKnownType (int index)
		{
			return new TreeNodeStyle ();
		}
		
		protected override Type[] GetKnownTypes ()
		{
			return types;
		}
		
		public int IndexOf (TreeNodeStyle style)
		{
			return ((IList)this).IndexOf (style);
		}
		
		public void Insert (int index, TreeNodeStyle style)
		{
			((IList)this).Insert (index, style);
		}
		
		public void Remove (TreeNodeStyle style)
		{
			((IList)this).Remove (style);
		}
		
		public void RemoveAt (int index)
		{
			((IList)this).RemoveAt (index);
		}
		
		public TreeNodeStyle this [int i] {
			get { return (TreeNodeStyle) ((IList)this) [i]; }
			set { ((IList)this) [i] = value; }
		}
		
		protected override void SetDirtyObject (object o)
		{
			((TreeNodeStyle)o).SetDirty ();
		}

		protected override void OnInsert (int index, object value)
		{
			// Why override?
			base.OnInsert (index, value);
		}
	}
}

#endif
