//
// System.Windows.Forms.DragEventArgs
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Implemented by Richard Baumann <biochem333@nyc.rr.com>
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
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	///	Provides data for the DragDrop, DragEnter, or DragOver event.
	/// </summary>
	[ComVisible(true)]
	public class DragEventArgs : EventArgs {

		#region Fields
		private DragDropEffects allowedEffect;
		private IDataObject data;
		private DragDropEffects effect;
		private int keyState;
		private int x;
		private int y;
		#endregion

		//
		//  --- Constructors/Destructors
		//
		public DragEventArgs(IDataObject data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect) : base()
		{
			this.data = data;
			this.keyState = keyState;
			this.x = x;
			this.y = y;
			this.allowedEffect = allowedEffect;
			this.effect = effect;
		}

		#region Public Properties

		[ComVisible(true)]
		public DragDropEffects AllowedEffect {
			get { 
					return allowedEffect; 
			}
		}

		[ComVisible(true)]
		public IDataObject Data {
			get { 
					return data; 
			}
		}

		[ComVisible(true)]
		public DragDropEffects Effect {
			get { 
					return effect; 
			}
			set { 
					effect = value; 
			}
		}

		[ComVisible(true)]
		public int KeyState {
			get { 
					return keyState; 
			}
		}

		[ComVisible(true)]
		public int X {
			get { 
					return x; 
			}
		}

		[ComVisible(true)]
		public int Y {
			get { 
					return y; 
			}
		}

		#endregion
	}
}
