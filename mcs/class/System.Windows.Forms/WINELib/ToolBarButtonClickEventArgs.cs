//
// System.Windows.Forms.ToolBarButtonClickEventArgs
//
// Author:
//	 stubbed out by Dennis Hayes(dennish@raytek.com)
//   stub ammended by Jaak Simm (jaaksimm@firm.ee)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//
using System;

namespace System.Windows.Forms {

	/// <summary>
	/// Summary description for ToolBarButtonClickEventArgs.
	/// </summary>
	[MonoTODO]
	public class ToolBarButtonClickEventArgs : EventArgs {

		#region Field
		ToolBarButton button;
		#endregion
		
		#region Constructor
		public ToolBarButtonClickEventArgs(ToolBarButton button)
		{
			this.button=button;
		}
		#endregion
		
		#region Properties
		public ToolBarButton Button {
			get { return button; }
			set { button=value; }
		}
		#endregion

		#region Public Methods

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ToolBarButtonClickEventArgs objects.
		///	The return value is based on the equivalence of
		///	Button Property
		///	of the two ToolBarButtonClickEventArgs.
		/// </remarks>
		public static bool operator == (ToolBarButtonClickEventArgs ToolBarButtonClickEventArgsA, ToolBarButtonClickEventArgs ToolBarButtonClickEventArgsB) 
		{
			return (ToolBarButtonClickEventArgsA.Button == ToolBarButtonClickEventArgsB.Button);
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two ToolBarButtonClickEventArgs objects.
		///	The return value is based on the equivalence of
		///	Button Property
		///	of the two ToolBarButtonClickEventArgs.
		/// </remarks>
		public static bool operator != (ToolBarButtonClickEventArgs ToolBarButtonClickEventArgsA, ToolBarButtonClickEventArgs ToolBarButtonClickEventArgsB) 
		{
			return (ToolBarButtonClickEventArgsA.Button != ToolBarButtonClickEventArgsB.Button);
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this
		///	ToolBarButtonClickEventArgsA and another
		///	object.
		/// </remarks>
		public override bool Equals (object obj) 
		{
			if (!(obj is ToolBarButtonClickEventArgs))return false;
			return (this == (ToolBarButtonClickEventArgs) obj);
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
		///	Formats the ToolBarButtonClickEventArgsA as a string.
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

