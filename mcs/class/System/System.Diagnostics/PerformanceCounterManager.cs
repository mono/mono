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
	[Guid("82840be1-d273-11d2-b94a-00600893b17a")]
	public sealed class PerformanceCounterManager : ICollectData {

		[MonoTODO]
		public PerformanceCounterManager ()
		{
		}

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

