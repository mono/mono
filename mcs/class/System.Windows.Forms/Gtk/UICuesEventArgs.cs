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
				return uicues;
			}
		}
		
		// Gets a value indicating whether the state of the focus cues has changed.
		public bool ChangeFocus {
			get {
				return (uicues & UICues.ChangeFocus) == 0;
			}
		}

		// Gets a value indicating whether the state of the keyboard cues has changed
		public bool ChangeKeyboard {
			get {
				return (uicues & UICues.ChangeKeyboard) == 0;
			}
		}

		// Gets a value indicating whether focus rectangles are shown after the change
		public bool ShowFocus {
			get {
				return (uicues & UICues.ShowFocus) == 0;
			}
		}

		// Gets a value indicating whether keyboard cues are underlined after the change
		public bool ShowKeyboard {
			get {
				return (uicues & UICues.ShowKeyboard) == 0;

			}
		}
		#endregion // Public Properties
	}
}
