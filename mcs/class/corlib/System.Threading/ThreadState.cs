// ThreadState.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Threading {


	/// <summary>
	/// <para> Specifies the execution states of a <see cref="T:System.Threading.Thread" />.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="T:System.Threading.ThreadState" />
	/// defines the set of possible execution states for threads. Once a thread is
	/// created, it is in one or more of these states until it terminates. Not all
	/// combinations of <see langword="ThreadState" /> values are valid; for example,
	/// a thread cannot be in both the <see cref="F:System.Threading.ThreadState.Stopped" /> and <see cref="F:System.Threading.ThreadState.Unstarted" />
	/// states.</para>
	/// <para>The following table shows the actions that cause a thread to
	///    change state.</para>
	/// <list type="table">
	/// <listheader>
	/// <term>Action</term>
	/// <description>ThreadState after Action</description>
	/// </listheader>
	/// <item>
	/// <term> The
	///       thread is created</term>
	/// <description>Unstarted</description>
	/// </item>
	/// <item>
	/// <term>
	/// <see cref="M:System.Threading.Thread.Start" /> is invoked on the thread</term>
	/// <description>Running</description>
	/// </item>
	/// <item>
	/// <term> The thread calls <see cref="M:System.Threading.Thread.Sleep(System.Int32)" /></term>
	/// <description>WaitSleepJoin</description>
	/// </item>
	/// <item>
	/// <term> The thread calls <see cref="M:System.Threading.Monitor.Wait(System.Object,System.Int32,System.Boolean)" /> to wait
	///    on an object</term>
	/// <description>WaitSleepJoin</description>
	/// </item>
	/// <item>
	/// <term> The thread calls <see cref="M:System.Threading.Thread.Join" /> to wait for
	///    another thread to terminate</term>
	/// <description>WaitSleepJoin</description>
	/// </item>
	/// <item>
	/// <term> The 
	///    <see cref="T:System.Threading.ThreadStart" />
	///    delegate methods finish executing</term>
	/// <description>Stopped</description>
	/// </item>
	/// <item>
	/// <term> Another thread requests
	///       the thread to <see cref="M:System.Threading.Thread.Abort(System.Object)" /></term>
	/// <description>AbortRequested</description>
	/// </item>
	/// <item>
	/// <term> The thread accepts a <see cref="M:System.Threading.Thread.Abort(System.Object)" /> request</term>
	/// <description>Aborted</description>
	/// </item>
	/// </list>
	/// <para>In addition to the states noted above, there is also the <see cref="F:System.Threading.ThreadState.Background" /> state, which indicates whether the thread is
	/// running in the background or foreground.</para>
	/// The current state of a thread can be retrieved from the <see cref="P:System.Threading.Thread.ThreadState" /> property,
	/// whose value is a combination of the <see cref="T:System.Threading.ThreadState" /> values. Once a
	/// thread has reached a final state, (<see cref="F:System.Threading.ThreadState.Stopped" />
	/// or <see cref="F:System.Threading.ThreadState.Aborted" />), it cannot change to any other state.
	/// </remarks>
	[Flags]
	public enum ThreadState {

		/// <summary><para>The thread represented by an instance of <see cref="T:System.Threading.Thread" /> has been started, is not blocked or stopped, and has no pending <see cref="T:System.Threading.ThreadAbortException" />.</para></summary>
		Running = 0x00000000,

		/// <summary><para>The thread represented by an instance of <see cref="T:System.Threading.Thread" /> is being
		///  executed as a background thread, as opposed to a foreground thread. <block subset="none" type="note">This state is controlled by setting the <see cref="P:System.Threading.Thread.IsBackground" />
		///  
		///  property.</block></para></summary>
		Background = 0x00000004,

		/// <summary><para>The <see cref="M:System.Threading.Thread.Start" qualify="true" /> method 
		/// has not been invoked on the thread.</para></summary>
		Unstarted = 0x00000008,

		/// <summary><para>The thread represented by an instance of <see cref="T:System.Threading.Thread" />
		/// has terminated normally.</para></summary>
		Stopped = 0x00000010,

		/// <summary><para>The thread represented by an instance of <see cref="T:System.Threading.Thread" /> is
		///  blocked as
		///  a result of
		///  a call to <see cref="M:System.Threading.Monitor.Wait(System.Object,System.Int32,System.Boolean)" />, <see cref="M:System.Threading.Thread.Sleep(System.Int32)" />, or <see cref="M:System.Threading.Thread.Join" />.</para></summary>
		WaitSleepJoin = 0x00000020,

		/// <summary><para>The <see cref="M:System.Threading.Thread.Abort(System.Object)" /> method has been invoked on the thread, but the thread
		///  has not yet received the pending <see cref="T:System.Threading.ThreadAbortException" /> that will attempt to terminate it.</para></summary>
		AbortRequested = 0x00000080,

		/// <summary><para>The thread represented by an instance of <see cref="T:System.Threading.Thread" /> has terminated as a result of a call to <see cref="!:System.Threading.Abort" />.</para></summary>
		Aborted = 0x00000100,
	} // ThreadState

} // System.Threading
