//
// System.Windows.Forms.NavigateEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//	 Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class NavigateEventArgs : EventArgs {
			
			#region Fields
			
			private bool isforward;

			#endregion

			//
			//  --- Constructor
			//
			public NavigateEventArgs(bool isForward)
			{
				isforward = isForward;
			}

			#region Public Properties

			[ComVisible(true)]
			public bool Forward 
			{
				get {
					return isforward;
				}
			}
			#endregion


			#region Public Methods

			/// <summary>
			///	Equality Operator
			/// </summary>
			///
			/// <remarks>
			///	Compares two NavigateEventArgs objects.
			///	The return value is based on the equivalence of
			///	Forward Property
			///	of the two NavigateEventArgs.
			/// </remarks>
			public static bool operator == (NavigateEventArgs NavigateEventArgsA, NavigateEventArgs NavigateEventArgsB) 
			{
				return (NavigateEventArgsA.Forward == NavigateEventArgsB.Forward);
			}
		
			/// <summary>
			///	Inequality Operator
			/// </summary>
			///
			/// <remarks>
			///	Compares two NavigateEventArgs objects.
			///	The return value is based on the equivalence of
			///	Forward Property
			///	of the two NavigateEventArgs.
			/// </remarks>
			public static bool operator != (NavigateEventArgs NavigateEventArgsA, NavigateEventArgs NavigateEventArgsB) 
			{
				return (NavigateEventArgsA.Forward != NavigateEventArgsB.Forward);
				}

			/// <summary>
			///	Equals Method
			/// </summary>
			///
			/// <remarks>
			///	Checks equivalence of this
			///	PropertyTabChangedEventArgs and another
			///	object.
			/// </remarks>
			public override bool Equals (object obj) 
			{
				if (!(obj is NavigateEventArgs))return false;
				return (this == (NavigateEventArgs) obj);
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
