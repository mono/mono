//
// System.Windows.Forms.UpDownEventArgs
//
// Author:
//	 stubbed out by Dennis Hayes(dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//
using System;
namespace System.Windows.Forms {

	/// <summary>
	/// Summary description for UpDownEventArgs.
	/// </summary>
	public class UpDownEventArgs : EventArgs {
		private int buttonID;

		public UpDownEventArgs(int buttonPushed){
			buttonID = buttonPushed;
		}

		public int ButtonID {
			get {
				return buttonID;
			}
		}
	}
}
