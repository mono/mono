//
// System.Windows.Forms.UICuesEventArgs
//
// Author:
//	 stubbed out by Stefan Warnke (StefanW@POBox.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//
using System;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	// </summary>


	/// <summary>
	/// UICuesEventArgs specifies which user interface feature changed and its new value.
	/// </summary>
	public class UICuesEventArgs : EventArgs {
		
		private UICues uicues;
//		/// --- Constructor ---
		public UICuesEventArgs(UICues uicues) 
		{
			this.uicues = uicues;
		}

		/// --- Public Properties ---
		#region Public Properties

		// Gets the bitwise combination of the UICues values
		public UICues Changed {
			get {
				throw new NotImplementedException ();
			}
		}
		
		// Gets a value indicating whether the state of the focus cues has changed.
		public bool ChangeFocus {
			get {
				throw new NotImplementedException ();
			}
		}

		// Gets a value indicating whether the state of the keyboard cues has changed
		public bool ChangeKeyboard {
			get {
				throw new NotImplementedException ();
			}
		}

		// Gets a value indicating whether focus rectangles are shown after the change
		public bool ShowFocus {
			get {
				throw new NotImplementedException ();
			}
		}

		// Gets a value indicating whether keyboard cues are underlined after the change
		public bool ShowKeyboard {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion // Public Properties

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two UICuesEventArgs objects.
		///	The return value is based on the equivalence of
		///	Changed Property
		///	of the two UICuesEventArgs.
		/// </remarks>
		public static bool operator == (UICuesEventArgs UICuesEventArgsA, UICuesEventArgs UICuesEventArgsB) 
		{
			return (UICuesEventArgsA.Changed == UICuesEventArgsB.Changed);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two UICuesEventArgs objects.
		///	The return value is based on the equivalence of
		///	Changed Property
		///	of the two UICuesEventArgs.
		/// </remarks>
		public static bool operator != (UICuesEventArgs UICuesEventArgsA, UICuesEventArgs UICuesEventArgsB) 
		{
			return (UICuesEventArgsA.Changed != UICuesEventArgsB.Changed);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	UICuesEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is UICuesEventArgs))return false;
			return (this == (UICuesEventArgs) obj);
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
		///	Formats the UICuesEventArgs as a string.
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
