//
// System.Windows.Forms.UICuesEventArgs
//
// Author:
//	 stubbed out by Stefan Warnke (StefanW@POBox.com)
//   Dennis Hayes (dennish@Raytek.com)
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

//		// Specifies the state of the user interface
		public enum UICues {
			// FIXME add real enums values
			Changed,		// The state of the focus cues and keyboard cues has changed. 
			ChangeFocus,	// The state of the focus cues has changed. 
			ChangeKeyboard,	// The state of the keyboard cues has changed. 
			None,			// No change was made.
			ShowFocus,		// Focus rectangles are displayed after the change.
			ShowKeyboard,	// Keyboard cues are underlined after the change. 
			Shown			// Focus rectangles are displayed and keyboard cues are underlined 
							// after the change. 
		};
		UICues uicues;
//		/// --- Constructor ---
		public UICuesEventArgs(UICues uicues) 
		{
			this.uicues = uicues;
		}

//		/// --- Destructor ---
//		~UICuesEventArgs() {
//			throw new NotImplementedException ();
//		}
//
//		/// --- Public Properties ---
//		#region Public Properties
//
//		// Gets the bitwise combination of the UICues values
//		public UICues Changed {
//			get {
//				throw new NotImplementedException ();
//			}
//		}
//		
//		// Gets a value indicating whether the state of the focus cues has changed.
//		public bool ChangeFocus {
//			get {
//				throw new NotImplementedException ();
//			}
//		}
//
//		// Gets a value indicating whether the state of the keyboard cues has changed
//		public bool ChangeKeyboard {
//			get {
//				throw new NotImplementedException ();
//			}
//		}
//
//		// Gets a value indicating whether focus rectangles are shown after the change
//		public bool ShowFocus {
//			get {
//				throw new NotImplementedException ();
//			}
//		}
//
//		// Gets a value indicating whether keyboard cues are underlined after the change
//		public bool ShowKeyboard {
//			get {
//				throw new NotImplementedException ();
//			}
//		}
//		#endregion // Public Properties
//
//		/// --- Public Methods ---
//		#region Public Methods
//
//		// Returns a String that represents the current Object
//		public override string ToString() {			
//			throw new NotImplementedException ();
//		}
//		#endregion // Public Methods

	}
}
