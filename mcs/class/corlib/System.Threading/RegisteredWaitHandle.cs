//
// System.Threading.RegisteredWaitHandle.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class RegisteredWaitHandle : MarshalByRefObject
	{
		internal RegisteredWaitHandle () {}

		[MonoTODO]
		public bool Unregister(WaitHandle waitObject) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		~RegisteredWaitHandle() {
			// FIXME
		}
	}
}
