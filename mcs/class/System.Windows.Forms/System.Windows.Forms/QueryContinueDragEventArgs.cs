//
// System.Windows.Forms.QueryContinueDragEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gterzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class QueryContinueDragEventArgs : EventArgs {

		#region Fields

		private int keystate;
		private bool escapepressed;
		private DragAction action;

		#endregion
		//
		//  --- Constructor
		//
		public QueryContinueDragEventArgs(int keyState, bool escapePressed, DragAction action)
		{
			this.keystate = keyState;
			this.escapepressed = escapePressed;
			this.action = action;
		}

		#region Public Properties

		[ComVisible(true)]
		public DragAction Action {
			get {
				return action;
			}
			set {
				action = value;
			}
		}

		[ComVisible(true)] 
		public bool EscapePressed {
			get {
				return escapepressed;
			}
		}

		[ComVisible(true)]
		public int KeyState {
			get {
				return keystate;
			}
		}
		#endregion

	}
}
