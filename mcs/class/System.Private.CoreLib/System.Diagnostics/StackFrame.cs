using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Diagnostics
{
	partial class StackFrame
	{
		internal StackFrame (MonoStackFrame monoStackFrame)
		{
			_method = monoStackFrame.methodBase;
			_nativeOffset = monoStackFrame.nativeOffset;
			_ilOffset = monoStackFrame.ilOffset;
			_fileName = monoStackFrame.fileName;
			_lineNumber = monoStackFrame.lineNumber;
			_columnNumber = monoStackFrame.columnNumber;
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		void BuildStackFrame (int skipFrames, bool needFileInfo)
		{
			if (get_frame_info (skipFrames + 3, needFileInfo, out var method, out var ilOffset, out var nativeOffset, out var fileName, out var line, out var column)) {
				_method = method;
				_ilOffset = ilOffset;
				_nativeOffset = nativeOffset;
				_fileName = fileName;
				_lineNumber = line;
				_columnNumber = column;
			}
		}

		bool AppendStackFrameWithoutMethodBase (StringBuilder sb) => false;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool get_frame_info (int skipFrames, bool needFileInfo,
			out MethodBase method, out int ilOffset, out int nativeOffset, out string file, out int line, out int column);

	}
}