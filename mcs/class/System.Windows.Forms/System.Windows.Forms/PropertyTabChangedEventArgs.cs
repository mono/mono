//
// System.Windows.Forms.PropertyTabChangedEventArgs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the PropertyTabChanged event of a PropertyGrid.
	///
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	[ComVisible(true)]
	public class PropertyTabChangedEventArgs : EventArgs	{

		#region Fields

			private PropertyTab oldtab;
			private PropertyTab newtab;

		#endregion

		#region Constructor
		//[ComVisible(true)]
		public PropertyTabChangedEventArgs(PropertyTab oldTab, PropertyTab newTab){
			
			this.oldtab = oldTab;
			this.newtab = newTab;

		}
		#endregion
				
		#region Public Properties

		[ComVisible(true)]
		public PropertyTab NewTab  {
			get {
				return newtab;
			}
		}

		[ComVisible(true)]
		public PropertyTab OldTab {
			get { 
				return oldtab;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two PropertyTabChangedEventArgs objects.
		///	The return value is based on the equivalence of
		///	oldtab and newtab Property
		///	of the two PropertyTabChangedEventArgs.
		/// </remarks>
		public static bool operator == (PropertyTabChangedEventArgs PropertyTabChangedEventArgsA, PropertyTabChangedEventArgs PropertyTabChangedEventArgsB) 
		{
			return (PropertyTabChangedEventArgsA.NewTab == PropertyTabChangedEventArgsB.NewTab) && (PropertyTabChangedEventArgsA.OldTab == PropertyTabChangedEventArgsB.OldTab);
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
		public static bool operator != (PropertyTabChangedEventArgs PropertyTabChangedEventArgsA, PropertyTabChangedEventArgs PropertyTabChangedEventArgsB) 
		{
			return (PropertyTabChangedEventArgsA.NewTab != PropertyTabChangedEventArgsB.NewTab) || (PropertyTabChangedEventArgsA.OldTab != PropertyTabChangedEventArgsB.OldTab);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	PropertyTabChangedEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is PropertyTabChangedEventArgs))return false;
			return (this == (PropertyTabChangedEventArgs) obj);
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
		///	Formats the object as a string.
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
