//
// System.Windows.Forms.Clipboard.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Provides methods to place data on and retrieve data from the system Clipboard. This class cannot be inherited.
	///
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	public sealed class Clipboard
	{
		// --- Methods ---
		[MonoTODO]
		public static IDataObject GetDataObject() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void SetDataObject(object data) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void SetDataObject(object data,bool copy) {
			throw new NotImplementedException ();
		}
	}
}
