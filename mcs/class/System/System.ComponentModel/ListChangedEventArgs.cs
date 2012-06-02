//
// System.ComponentModel.ListChangedEventArgs.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
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

using System.ComponentModel;

namespace System.ComponentModel {
	public class ListChangedEventArgs : EventArgs
	{
	
		ListChangedType changedType;
		int oldIndex;
		int newIndex;
		PropertyDescriptor propDesc;

		public ListChangedEventArgs (ListChangedType listChangedType,
					     int newIndex)
			: this (listChangedType, newIndex, -1)
		{
		}
	
		public ListChangedEventArgs (ListChangedType listChangedType,
					     PropertyDescriptor propDesc)
		{
			this.changedType = listChangedType;
			this.propDesc = propDesc;
		}
		
		public ListChangedEventArgs (ListChangedType listChangedType,
					     int newIndex, int oldIndex)
		{
			this.changedType = listChangedType;
			this.newIndex = newIndex;
			this.oldIndex = oldIndex;
		}

		public ListChangedEventArgs (ListChangedType listChangedType,
					     int newIndex,
					     PropertyDescriptor propDesc)
		{
			this.changedType = listChangedType;
			this.newIndex = newIndex;
			this.oldIndex = newIndex;
			this.propDesc = propDesc;
		}

		public ListChangedType ListChangedType {
			get { return changedType; }
		}
	
		public int OldIndex {
			get { return oldIndex; }
		}
	
		public int NewIndex {
			get { return newIndex; }
		}

		public PropertyDescriptor PropertyDescriptor {
			get { return propDesc; }
		}
	}
}
