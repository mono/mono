using System;

namespace System.Runtime.DurableInstancing
{
	public enum InstanceKeyState
	{
		Unknown,
		Associated,
		Completed,
	}
	
	public enum InstanceState
	{
		Unknown,
		Uninitialized,
		Initialized,
		Completed,
	}
	
	[Flags]
	public enum InstanceValueConsistency
	{
		None = 0,
		InDoubt = 1,
		Partial = 2
	}
	
	[Flags]
	public enum InstanceValueOptions
	{
		None = 0,
		Optional = 1,
		WriteOnly = 2
	}
}
