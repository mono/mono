//
// System.Windows.Forms.KeyPressEventArgs.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gterzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	// </summary>

    public class KeyPressEventArgs : EventArgs {

		#region Fields

		private char keychar;
		private bool handled = false;	//Gian : Initialize?
		
		#endregion

		//
		//  --- Constructor
		//
		public KeyPressEventArgs (char keyChar)
		{
			this.keychar = keyChar;
		}

		#region Public Properties
		[ComVisible(true)]
		public bool Handled {
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}

		[ComVisible(true)]
		public char KeyChar {
			get {
				return keychar;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two KeyPressEventArgs objects.
		///	The return value is based on the equivalence of
		///	Handled and KeyChar Property
		///	of the two KeyPressEventArgs.
		/// </remarks>
		public static bool operator == (KeyPressEventArgs KeyPressEventArgsA, KeyPressEventArgs KeyPressEventArgsB) 
		{
			return (KeyPressEventArgsA.Handled == KeyPressEventArgsB.Handled) && (KeyPressEventArgsA.KeyChar == KeyPressEventArgsB.KeyChar);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two KeyPressEventArgs objects.
		///	The return value is based on the equivalence of
		///	Handled and KeyChar Property
		///	of the two KeyPressEventArgs.
		/// </remarks>
		public static bool operator != (KeyPressEventArgs KeyPressEventArgsA, KeyPressEventArgs KeyPressEventArgsB) 
		{
			return (KeyPressEventArgsA.Handled != KeyPressEventArgsB.Handled) || (KeyPressEventArgsA.KeyChar != KeyPressEventArgsB.KeyChar);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	KeyPressEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is KeyPressEventArgs))return false;
			return (this == (KeyPressEventArgs) obj);
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
			return base.ToString() + " KeyPressEventArgs";
		}


		#endregion

	}
}
