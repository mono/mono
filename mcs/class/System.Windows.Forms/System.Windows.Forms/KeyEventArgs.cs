//
// System.Windows.Forms.KeyEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// Complete
	// </summary>

    public class KeyEventArgs : EventArgs {

		#region Fields
		
		private Keys keydata;
		private Keys keycode;			
		private Keys modifiers;

		private bool alt = false;
		private bool control = false;
		private bool handled = false;
		private bool shift = false;
		
		private int keyvalue = -1;
		
		#endregion
		//
		//  --- Constructor
		//
		public KeyEventArgs (Keys keyData)
		{
			keydata = keyData;
		}

		#region Public Properties

		[ComVisible(true)]
		public virtual bool Alt 
		{
			get {
				return alt;
			}
		}
		
		[ComVisible(true)]
		public bool Control 
		{
			get {
				return control;
			}
		}
		
		[ComVisible(true)]
		public bool Handled 
		{
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}
		
		[ComVisible(true)]
		public Keys KeyCode 
		{
			get {
				return keycode;
			}
		}
		
		[ComVisible(true)]
		public Keys KeyData 
		{
			get {
				return keydata;
			}
		}
		
		[ComVisible(true)]
		public int KeyValue 
		{
			get {
				return keyvalue;
			}
		}
		
		[ComVisible(true)]
		public Keys Modifiers 
		{
			get {
				return modifiers;
			}
		}
		
		[ComVisible(true)]
		public bool Shift 
		{
			get {
				return shift;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two KeyEventArgs objects.
		///	The return value is based on the equivalence of
		///	alt, control, handled, keycode, keydata, keyvalue, modifiers, shift Property
		///	of the two KeyEventArgs.
		/// </remarks>
		public static bool operator == (KeyEventArgs KeyEventArgsA, KeyEventArgs KeyEventArgsB) 
		{
			return (KeyEventArgsA.Alt == KeyEventArgsB.Alt) &&
				   (KeyEventArgsA.Control == KeyEventArgsB.Control) &&
				   (KeyEventArgsA.Handled == KeyEventArgsB.Handled) &&
				   (KeyEventArgsA.KeyCode == KeyEventArgsB.KeyCode) &&
				   (KeyEventArgsA.KeyData == KeyEventArgsB.KeyData) &&
				   (KeyEventArgsA.KeyValue == KeyEventArgsB.KeyValue) &&
				   (KeyEventArgsA.Modifiers == KeyEventArgsB.Modifiers) &&
				   (KeyEventArgsA.Shift == KeyEventArgsB.Shift);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two KeyEventArgs objects.
		///	The return value is based on the equivalence of
		///	alt, control, handled, keycode, keydata, keyvalue, modifiers, shift Property
		///	of the two KeyEventArgs.
		/// </remarks>
		public static bool operator != (KeyEventArgs KeyEventArgsA, KeyEventArgs KeyEventArgsB) 
		{
			return (KeyEventArgsA.Alt != KeyEventArgsB.Alt) ||
				(KeyEventArgsA.Control != KeyEventArgsB.Control) ||
				(KeyEventArgsA.Handled != KeyEventArgsB.Handled) ||
				(KeyEventArgsA.KeyCode != KeyEventArgsB.KeyCode) ||
				(KeyEventArgsA.KeyData != KeyEventArgsB.KeyData) ||
				(KeyEventArgsA.KeyValue != KeyEventArgsB.KeyValue) ||
				(KeyEventArgsA.Modifiers != KeyEventArgsB.Modifiers) ||
				(KeyEventArgsA.Shift != KeyEventArgsB.Shift);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	KeyEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is KeyEventArgs))return false;
			return (this == (KeyEventArgs) obj);
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
			return base.ToString() + " KeyEventArgs";
		}


		#endregion
	}
}
