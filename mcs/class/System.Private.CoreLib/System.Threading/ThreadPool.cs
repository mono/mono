namespace System.Threading
{
	partial class ThreadPool
	{
		internal static void UnsafeQueueUserWorkItemInternal (object callBack, bool preferLocal) => throw new NotImplementedException ();

		internal static bool TryPopCustomWorkItem (object workItem) => throw new NotImplementedException ();

		public static bool QueueUserWorkItem<TState> (Action<TState> callBack, TState state, bool preferLocal) => throw new NotImplementedException ();

		public static bool UnsafeQueueUserWorkItem<TState> (Action<TState> callBack, TState state, bool preferLocal) => throw new NotImplementedException ();

		public static bool UnsafeQueueUserWorkItem (IThreadPoolWorkItem callBack, bool preferLocal) => throw new NotImplementedException ();
	}
}