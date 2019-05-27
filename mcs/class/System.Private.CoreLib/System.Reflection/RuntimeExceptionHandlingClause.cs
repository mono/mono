using System.Runtime.InteropServices;

namespace System.Reflection
{
	[StructLayout (LayoutKind.Sequential)]
	internal sealed class RuntimeExceptionHandlingClause : ExceptionHandlingClause
	{
		#region Keep in sync with MonoReflectionExceptionHandlingClause in object-internals.h
		internal Type catch_type;
		internal int filter_offset;
		internal ExceptionHandlingClauseOptions flags;
		internal int try_offset;
		internal int try_length;
		internal int handler_offset;
		internal int handler_length;
		#endregion

		public override ExceptionHandlingClauseOptions Flags => flags;
        public override int TryOffset => try_offset;
        public override int TryLength => try_length;
        public override int HandlerOffset => handler_offset;
        public override int HandlerLength => handler_length;
		public override int FilterOffset => filter_offset;
		public override Type? CatchType => catch_type;
	}

}
