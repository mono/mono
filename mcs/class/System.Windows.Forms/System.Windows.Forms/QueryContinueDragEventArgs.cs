//
// System.Windows.Forms.QueryContinueDragEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class QueryContinueDragEventArgs : EventArgs {
		private int keystate;
		private bool escapepressed;
		private DragAction action;
		//
		//  --- Constructor
		//
		[ComVisible(true)] 
		public QueryContinueDragEventArgs(int keyState, bool escapePressed, DragAction action)
		{
			keystate = keyState;
			escapepressed = escapePressed;
			this.action = action;
		}

		//
		//  --- Public Properties
		//
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

		//
		//  --- Public Methods
		//
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
