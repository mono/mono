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
	 }
}
