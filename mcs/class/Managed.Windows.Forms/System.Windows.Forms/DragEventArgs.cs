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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//


// COMPLETE

namespace System.Windows.Forms {
	public class DragEventArgs : EventArgs {
		private int		x;
		private int		y;
		private int		keystate;
		private DragDropEffects	allowed_effect;
		private DragDropEffects	current_effect;
		private IDataObject	data_object;

		#region Public Constructors
		public DragEventArgs(IDataObject data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect) {
			this.x=x;
			this.y=y;
			this.keystate=keyState;
			this.allowed_effect=allowedEffect;	
			this.current_effect=effect;
			this.data_object=data;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public DragDropEffects AllowedEffect {
			get {
				return this.allowed_effect;
			}
		}

		public IDataObject Data {
			get {
				return this.data_object;
			}
		}

		public DragDropEffects Effect {
			get {
				return this.current_effect;
			}

			set {
				if ((value & this.allowed_effect)!=0) {
					this.current_effect=value;
				}
			}
		}

		public int KeyState {
			get {
				return this.keystate;
			}
		}

		public int X {
			get {
				return this.x;
			}
		}

		public int Y {
			get {
				return this.y;
			}
		}
		#endregion	// Public Instance Properties
	}
}
