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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two PropertyValueChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	ChangedItem and OldValue Property
		///	of the two PropertyValueChangedEventArgs.
		/// </remarks>
		public static bool operator == (PropertyValueChangedEventArgs PropertyValueChangedEventArgsA, PropertyValueChangedEventArgs PropertyValueChangedEventArgsB) 
		{
			return (PropertyValueChangedEventArgsA.ChangedItem == PropertyValueChangedEventArgsB.ChangedItem) && (PropertyValueChangedEventArgsA.OldValue == PropertyValueChangedEventArgsB.OldValue);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two PropertyValueChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	ChangedItem and OldValue Property
		///	of the two PropertyValueChangedEventArgs.
		/// </remarks>
		public static bool operator != (PropertyValueChangedEventArgs PropertyValueChangedEventArgsA, PropertyValueChangedEventArgs PropertyValueChangedEventArgsB) 
		{
			return (PropertyValueChangedEventArgsA.ChangedItem != PropertyValueChangedEventArgsB.ChangedItem) || (PropertyValueChangedEventArgsA.OldValue != PropertyValueChangedEventArgsB.OldValue);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	PropertyValueChangedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is PropertyValueChangedEventArgs))return false;
			return (this == (PropertyValueChangedEventArgs) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the PropertyValueChangedEventArgs as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString();
		}

		#endregion


	}
}
