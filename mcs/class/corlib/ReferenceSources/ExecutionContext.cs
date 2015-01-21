
using System.Security;

namespace System.Threading
{
	partial class ExecutionContext
	{
		internal static ExecutionContext Capture (ref StackCrawlMark stackMark, CaptureOptions options)
		{
			return Capture ((options & CaptureOptions.IgnoreSyncCtx) == 0, false);
		}

		internal static ExecutionContext FastCapture()
		{
			return Capture ();
		}

		[Flags]
		internal enum CaptureOptions
		{
			None = 0x00,

			IgnoreSyncCtx = 0x01,       //Don't flow SynchronizationContext

			OptimizeDefaultCase = 0x02, //Faster in the typical case, but can't show the result to users
			                            // because they could modify the shared default EC.
			                            // Use this only if you won't be exposing the captured EC to users.
		}

		private static readonly ExecutionContext s_dummyDefaultEC = new ExecutionContext();
		static internal ExecutionContext PreAllocatedDefault
		{
			[SecuritySafeCritical]
			get {
				return s_dummyDefaultEC;
			}
		}

		internal bool IsPreAllocatedDefault
		{
			get
			{
				return this == s_dummyDefaultEC;
			}
		}
	}

	[Serializable]
	internal enum StackCrawlMark
	{
		LookForMe = 0,
		LookForMyCaller = 1,
		LookForMyCallersCaller = 2,
		LookForThread = 3
	}
}