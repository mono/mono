//
// System.Windows.Forms.PropertyTabChangedEventArgs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the PropertyTabChanged event of a PropertyGrid.
	///
	/// </summary>

	[ComVisible(true)]
	public class PropertyTabChangedEventArgs : EventArgs	{

		#region Fields

			private PropertyTab oldtab;
			private PropertyTab newtab;

		#endregion

		#region Constructor
		public PropertyTabChangedEventArgs(PropertyTab oldTab, PropertyTab newTab){
			
			this.oldtab = oldTab;
			this.newtab = newTab;

		}
		#endregion
				
		#region Public Properties

		[ComVisible(true)]
		public PropertyTab NewTab  {
			get {
				return newtab;
			}
		}

		[ComVisible(true)]
		public PropertyTab OldTab {
			get { 
				return oldtab;
			}
		}
		#endregion
	}
}
