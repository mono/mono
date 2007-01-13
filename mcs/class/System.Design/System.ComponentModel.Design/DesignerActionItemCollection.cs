//
// System.ComponentModel.Design.DesignerActionItemCollection.cs
//
// Authors:
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright 2006 Novell, Inc
//

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
using System.Windows.Forms;
using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesignerActionItemCollection : CollectionBase
	{
		//
		// Constructors
		//
		public DesignerActionItemCollection ()
		{
		}

		//
		// Properties
		//
		public DesignerActionItem this [int index]
		{
			get {
				return (DesignerActionItem) List [index];
			}
			set {
				List [index] = value;
			}
		}

		//
		// Methods
		//
		public int Add (DesignerActionItem value)
		{
			return List.Add (value); 
		}

		public bool Contains (DesignerActionItem value)
		{
			return List.Contains (value);
		}
		
		public void CopyTo (DesignerActionItem[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (DesignerActionItem value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, DesignerActionItem value)
		{
			List.Insert (index, value);
		}

		public void Remove (DesignerActionItem value)
		{
			List.Remove (value);
		}
	}
}
#endif
