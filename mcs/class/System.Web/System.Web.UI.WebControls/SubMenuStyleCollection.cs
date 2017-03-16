//
// System.Web.UI.WebControls.SubMenuStyleCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class SubMenuStyleCollection: StateManagedCollection
	{
		static Type[] types = new Type[] { typeof (SubMenuStyle) };
		
		internal SubMenuStyleCollection ()
		{
		}
		
		public int Add (SubMenuStyle style)
		{
			return ((IList)this).Add (style);
		}
		
		public bool Contains (SubMenuStyle style)
		{
			return ((IList)this).Contains (style);
		}
		
		public void CopyTo (SubMenuStyle[] styleArray, int index)
		{
			((IList)this).CopyTo (styleArray, index);
		}
		
		protected override object CreateKnownType (int index)
		{
			return new SubMenuStyle ();
		}
		
		protected override Type[] GetKnownTypes ()
		{
			return types;
		}
		
		public int IndexOf (SubMenuStyle style)
		{
			return ((IList)this).IndexOf (style);
		}
		
		public void Insert (int index, SubMenuStyle style)
		{
			((IList)this).Insert (index, style);
		}
		
		public void Remove (SubMenuStyle style)
		{
			((IList)this).Remove (style);
		}
		
		public void RemoveAt (int index)
		{
			((IList)this).RemoveAt (index);
		}
		
		public SubMenuStyle this [int i] {
			get { return (SubMenuStyle) ((IList)this) [i]; }
			set { ((IList)this) [i] = value; }
		}
		
		protected override void SetDirtyObject (object o)
		{
			((SubMenuStyle)o).SetDirty ();
		}
	}
}

