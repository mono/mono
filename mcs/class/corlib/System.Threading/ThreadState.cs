// ThreadState.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:30:30 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Threading {


	/// <summary>
	/// </summary>
	[Flags]
	public enum ThreadState {

		/// <summary>
		/// </summary>
		Running = 0x00000000,

		StopRequested = 0x00000001,
		SuspendRequested = 0x00000002,
		
		/// <summary>
		/// </summary>
		Background = 0x00000004,

		/// <summary>
		/// </summary>
		Unstarted = 0x00000008,

		/// <summary>
		/// </summary>
		Stopped = 0x00000010,

		/// <summary>
		/// </summary>
		WaitSleepJoin = 0x00000020,

		Suspended = 0x00000040,

		/// <summary>
		/// </summary>
		AbortRequested = 0x00000080,

		/// <summary>
		/// </summary>
		Aborted = 0x00000100,
   } // ThreadState

} // System.Threading
