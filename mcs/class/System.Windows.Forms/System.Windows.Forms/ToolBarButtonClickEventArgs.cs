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
	}
}

