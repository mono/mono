
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace System.Threading
{
	internal sealed class MonoRuntimeWorkItem : IThreadPoolWorkItem
	{
		AsyncResult ares;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void ExecuteWorkItem();

		public void MarkAborted(ThreadAbortException tae)
		{
		}
	}
}
