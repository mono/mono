//
// System.Diagnostics.PerformanceCounterManager.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Diagnostics {

	[ComVisible(true)]
	// [Guid("")]
	public class PerformanceCounterManager : ICollectData {

//		[MonoTODO]
//		public PerformanceCounterManager ()
//		{
//			throw new NotImplementedException ();
//		}
//
		[MonoTODO]
		void ICollectData.CloseData ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollectData.CollectData (
			int callIdx,
			IntPtr valueNamePtr,
			IntPtr dataPtr,
			int totalBytes,
			out IntPtr res)
		{
			throw new NotImplementedException ();
		}
	}
}

