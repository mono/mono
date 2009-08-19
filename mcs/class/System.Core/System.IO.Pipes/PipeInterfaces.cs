using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes
{
	// Common interfaces

	interface IPipe
	{
		SafePipeHandle Handle { get; }
		void WaitForPipeDrain ();
	}

	interface IAnonymousPipeClient : IPipe
	{
	}

	interface IAnonymousPipeServer : IPipe
	{
		SafePipeHandle ClientHandle { get; }
		void DisposeLocalCopyOfClientHandle ();
	}

	interface INamedPipeClient : IPipe
	{
		void Connect ();
		void Connect (int timeout);
		int NumberOfServerInstances { get; }
		bool IsAsync { get; }
	}

	interface INamedPipeServer : IPipe
	{
		void Disconnect ();
		void WaitForConnection ();
	}
}
