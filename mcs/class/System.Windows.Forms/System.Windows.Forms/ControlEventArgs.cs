//
// System.Windows.Forms.ControlEventArgs.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)

// (C) Ximian, Inc., 2002


namespace System.Windows.Forms {

	/// <summary>
	/// Complete.
	/// </summary>

	public class ControlEventArgs : EventArgs {

		#region Fields
		Control control;
		#endregion

		public ControlEventArgs(Control control) 
		{
			this.control = control;
		}
		
		#region Public Properties
		public Control Control 
		{
			get 
				{ 
					return control; 
				}
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ControlEventArgs objects.
		///	The return value is based on the equivalence of
		///	control Property
		///	of the two ControlEventArgs.
		/// </remarks>
		public static bool operator == (ControlEventArgs ControlEventArgsA, ControlEventArgs ControlEventArgsB) 
		{
			return (ControlEventArgsA.Control == ControlEventArgsB.Control);

		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ControlEventArgs objects.
		///	The return value is based on the equivalence of
		///	control Property
		///	of the two ControlEventArgs.
		/// </remarks>
		public static bool operator != (ControlEventArgs ControlEventArgsA, ControlEventArgs ControlEventArgsB) 
		{
			return (ControlEventArgsA.Control != ControlEventArgsB.Control);

		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ControlEventArgs and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ControlEventArgs))return false;
			return (this == (ControlEventArgs) obj);
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
			return base.ToString() + " ControlEventArgs";
		}


		#endregion

	}
}
