using System;
using System.IO;

namespace Mono.Debugger.Soft
{
	public enum ILExceptionHandlerType
	{
		Catch = ExceptionClauseFlags.None,
		Filter = ExceptionClauseFlags.Filter,
		Finally = ExceptionClauseFlags.Finally,
		Fault = ExceptionClauseFlags.Fault,
	}

	public class ILExceptionHandler
	{
		public int TryOffset { get; internal set; }
		public int TryLength { get; internal set; }
		public ILExceptionHandlerType HandlerType { get; internal set; }
		public int HandlerOffset { get; internal set; }
		public int HandlerLength { get; internal set;}
		public int FilterOffset { get; internal set; }
		public TypeMirror CatchType { get; internal set; }

		internal ILExceptionHandler (int try_offset, int try_length, ILExceptionHandlerType handler_type, int handler_offset, int handler_length)
		{
			TryOffset = try_offset;
			TryLength = try_length;
			HandlerType = handler_type;
			HandlerOffset = handler_offset;
			HandlerLength = handler_length;
		}
	}
}
