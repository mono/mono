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

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

