//
// System.Windows.Forms.PropertyValueChangedEventArgs
//
// Author:
//	 stubbed out by Dennis Hayes(dennish@raytek.com)
//   completed by Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//
using System;

namespace System.Windows.Forms {

	/// <summary>
	/// Summary description for PropertyValueChangedEventArgs.
	/// </summary>
	public class PropertyValueChangedEventArgs : EventArgs {

		#region Fields
		private GridItem changedItem;
		private object oldValue;
		#endregion

		public PropertyValueChangedEventArgs(GridItem changedItem, object oldValue)
		{
			this.changedItem = changedItem;
			this.oldValue = oldValue;
		}

		#region Public Properties
		
		// ChangedItem Property
		public GridItem ChangedItem 
		{
			get 
			{
				return changedItem;
			}
		}

		// OldValue Property
		public object OldValue 
		{
			get 
			{
				return oldValue;
			}
		}
		#endregion
	}
}
