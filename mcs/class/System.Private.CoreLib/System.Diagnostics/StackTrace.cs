using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	// Need our own stackframe class since the shared version has its own fields
	[StructLayout (LayoutKind.Sequential)]
	class MonoStackFrame
	{
		#region Keep in sync with object-internals.h
		internal int ilOffset;
		internal int nativeOffset;
		// Unused
		internal long methodAddress;
		// Unused
		internal uint methodIndex;
		internal MethodBase methodBase;
		internal string fileName;
		internal int lineNumber;
		internal int columnNumber;
		// Unused
		internal string internalMethodName;
		#endregion
	}

	partial class StackTrace
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static MonoStackFrame[] get_trace (Exception e, int skipFrames, bool fNeedFileInfo);

		void InitializeForCurrentThread (int skipFrames, bool fNeedFileInfo)
		{
		}
		
		void InitializeForException (Exception e, int skipFrames, bool fNeedFileInfo)
		{
			var frames = get_trace (e, skipFrames, fNeedFileInfo);
			_numOfFrames = frames.Length;
			_stackFrames = new StackFrame [_numOfFrames];
			for (int i = 0; i < _numOfFrames; ++i) {
				_stackFrames [i] = new StackFrame (frames [i]);
			}
		}
	}
}