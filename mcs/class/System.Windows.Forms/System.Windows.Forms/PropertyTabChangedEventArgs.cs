//
// System.Windows.Forms.PropertyTabChangedEventArgs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//
// (C) Ximian, Inc., 2002
//

//using System.Drawing;
//using System.Drawing.Printing;
//using System.ComponentModel;
//using System.Collections;
using System.Windows.Forms.Design;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides data for the PropertyTabChanged event of a PropertyGrid.
	///
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	[ComVisible(true)]
	public class PropertyTabChangedEventArgs : EventArgs	{
		#region Constructor
		[MonoTODO]
//		[ComVisible(true)]
		public PropertyTabChangedEventArgs(PropertyTab oldTab,PropertyTab newTab) {
			throw new NotImplementedException ();
		}
		#endregion
		
		
		
		#region Properties
		[MonoTODO]
		[ComVisible(true)]
		public PropertyTab NewTab  {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		[ComVisible(true)]
		public PropertyTab OldTab {
			get { throw new NotImplementedException (); }
		}
		#endregion
	}
}
