//
// System.Threading.IOCompletionCallback.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	// 'unsafe' wasn't in the spec, but the compiler insists because of
	// the pointer.
	[Serializable]
	[CLSCompliant(false)]
	public unsafe delegate void IOCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped *pOVERLAP);
}
