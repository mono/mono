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
