//
// System.ComponentModel.ListChangedEventArgs.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System.ComponentModel;

namespace System.ComponentModel {
	public class ListChangedEventArgs : EventArgs
	{
	
		ListChangedType changedType;
		int oldIndex;
		int newIndex;
		PropertyDescriptor propDesc;	// What to do with this?
	
		public ListChangedEventArgs (ListChangedType listChangedType,
					     int newIndex)
		{
			this.changedType = listChangedType;
			this.newIndex = newIndex;
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
		 
		public ListChangedType ListChangedType {
			get { return changedType; }
		}
	
		public int OldIndex {
			get { return oldIndex; }
		}
	
		public int NewIndex {
			get { return newIndex; }
		}
	}
}
