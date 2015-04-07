using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Net.Sockets
{
	internal sealed class MonoSocketRuntimeWorkItem : IThreadPoolWorkItem
	{
		SocketAsyncResult socket_async_result;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void ExecuteWorkItem();

		public void MarkAborted(ThreadAbortException tae)
		{
		}
	}
}