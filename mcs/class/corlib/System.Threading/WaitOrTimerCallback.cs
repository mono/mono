//
// System.Threading.WaitOrTimerCallback.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public delegate void WaitOrTimerCallback(object state, bool timedOut);
}
