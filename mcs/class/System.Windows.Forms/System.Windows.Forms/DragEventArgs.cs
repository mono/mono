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
