namespace System.IO.Pipes
{
	public sealed partial class AnonymousPipeServerStream
	{
		public AnonymousPipeServerStream (PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
			: this (direction, inheritability, bufferSize)
		{
			if (pipeSecurity != null) {
				throw new PlatformNotSupportedException ();
			}
		}
	}
}
