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
	// Just a template.
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
		//[ComVisible(true)] 
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

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two QueryContinueDragEventArgs objects.
		///	The return value is based on the equivalence of
		///	keystate, escaperessed and action Property
		///	of the two QueryContinueDragEventArgs.
		/// </remarks>
		public static bool operator == (QueryContinueDragEventArgs QueryContinueDragEventArgsA, QueryContinueDragEventArgs QueryContinueDragEventArgsB) 
		{
			return ((QueryContinueDragEventArgsA.EscapePressed == QueryContinueDragEventArgsB.EscapePressed) && (QueryContinueDragEventArgsA.KeyState == QueryContinueDragEventArgsB.KeyState) && (QueryContinueDragEventArgsA.Action == QueryContinueDragEventArgsB.Action));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ScrollEventArgs objects.
		///	The return value is based on the equivalence of
		///	newvalue and type Property
		///	of the two ScrollEventArgs.
		/// </remarks>
		public static bool operator != (QueryContinueDragEventArgs QueryContinueDragEventArgsA, QueryContinueDragEventArgs QueryContinueDragEventArgsB) 
		{
			return ((QueryContinueDragEventArgsA.EscapePressed != QueryContinueDragEventArgsB.EscapePressed) || (QueryContinueDragEventArgsA.KeyState != QueryContinueDragEventArgsB.KeyState) || (QueryContinueDragEventArgsA.Action != QueryContinueDragEventArgsB.Action));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	QueryContinueDragEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is QueryContinueDragEventArgs))return false;
			return (this == (QueryContinueDragEventArgs) obj);
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
