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
	
		public ListChangedEventArgs (ListChangedType listChangedType,
					     int newIndex)
		{
			this.changedType = listChangedType;
			this.newIndex = newIndex;
		}
	
		[MonoTODO]
		public ListChangedEventArgs (ListChangedType listChangedType,
					     PropertyDescriptor propDesc)
		{
			this.changedType = listChangedType;
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
