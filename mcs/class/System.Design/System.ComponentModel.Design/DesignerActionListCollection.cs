//
// System.ComponentModel.Design.DesignerActionListCollection
//
// Authors:		
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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

#if NET_2_0

using System;
using System.Collections;
using System.ComponentModel;

namespace System.ComponentModel.Design
{
	[System.Runtime.InteropServices.ComVisible (true)]
	public class DesignerActionListCollection : CollectionBase
	{
		
		public DesignerActionListCollection ()
		{
		}

		public DesignerActionListCollection (DesignerActionList[] value)
		{
			AddRange (value);
		}

		public DesignerActionList this[int index] {
			get { return (DesignerActionList) base.List[index]; }
			set { base.List[index] = value; }
		}
			
		public int Add (DesignerActionList value)
		{
			return base.List.Add (value);
		}

		public void AddRange (DesignerActionList[] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			foreach (DesignerActionList actionList in value)
				Add (actionList);
		}
		
		public void AddRange (DesignerActionListCollection value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			foreach (DesignerActionList actionList in value)
				Add (actionList);
		}

		public bool Contains (DesignerActionList value)
		{
			return base.List.Contains (value);
		}

		public void CopyTo (DesignerActionList[] array, int index)
		{
			base.List.CopyTo (array, index);
		}

		public int IndexOf (DesignerActionList value)
		{
			return base.List.IndexOf (value);
		}

		public void Insert (int index, DesignerActionList value) 
		{
			base.List.Insert (index, value);
		}

		public void Remove (DesignerActionList value)
		{
			base.List.Remove (value);
		}		
			
		protected override void OnClear ()
		{
		}

		protected override void OnInsert (int index, object value)
		{
		}
		
		protected override void OnRemove (int index, object value)
		{
		}
		
		protected override void OnSet (int index, object oldValue, object newValue)
		{
		}

		protected override void OnValidate (object value)
		{
		}
		
	}
}
#endif
