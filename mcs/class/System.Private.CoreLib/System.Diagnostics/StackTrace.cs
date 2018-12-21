namespace System.Diagnostics
{
	partial class StackTrace
	{
		internal StackTrace (StackFrame[] frames)
		{
			throw new NotImplementedException ();
		}

		void InitializeForCurrentThread (int skipFrames, bool fNeedFileInfo)
		{

		}
		
		void InitializeForException (Exception e, int skipFrames, bool fNeedFileInfo)
		{

		}
	}
}