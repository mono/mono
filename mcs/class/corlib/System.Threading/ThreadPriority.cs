// ThreadPriority.cs
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
	/// <para> Specifies the scheduling priority of a <see cref="T:System.Threading.Thread" />.
	///  </para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="T:System.Threading.ThreadPriority" />
	/// 
	/// values specify the relative scheduling priority of
	/// threads.</para>
	/// <para>
	/// <block subset="ECMAOnly" type="ECMA Only">Operating systems are not guaranteed to support 
	///  preemptive scheduling. Also, the concept of "thread priority" may not exist at
	///  all or its meaning may vary, depending on the underlying operating system.
	///  Implementors of this type are required to describe how the notion of thread
	///  priority maps to operating system priority. For more information about threads,
	///  see the <see cref="T:System.Threading.Thread" />
	///  class. </block>
	/// </para>
	/// <para> The <see cref="P:System.Threading.Thread.Priority" /> 
	/// property sets and returns the priority value information for a thread.
	/// Applications can request a scheduling priority for a thread by setting the
	/// <see cref="P:System.Threading.Thread.Priority" /> property to the appropriate 
	/// <see langword="ThreadPriority" />
	/// value. The
	/// default thread priority is <see cref="F:System.Threading.ThreadPriority.Normal" />
	/// .</para>
	/// <para>The priority of a thread does not affect the thread's 
	///  state; the state of the thread must be <see cref="F:System.Threading.ThreadState.Running" /> before the operating system can schedule
	///  it. </para>
	/// </remarks>
	public enum ThreadPriority {

		/// <summary><para><SPAN>Threads with
		///       this priority may be scheduled after threads with any other
		///       priority.</SPAN></para></summary>
		Lowest = 0,

		/// <summary><para>Threads with this priority may be scheduled
		///       after threads with <see cref="F:System.Threading.ThreadPriority.Normal" /> priority, and before those with <see cref="F:System.Threading.ThreadPriority.Lowest" />
		///       priority.
		///       </para></summary>
		BelowNormal = 1,

		/// <summary><para>Threads with this priority may be scheduled after threads with <see cref="F:System.Threading.ThreadPriority.AboveNormal" /> priority and before those with <see cref="F:System.Threading.ThreadPriority.BelowNormal" />
		/// priority.</para><para>Threads have <see cref="F:System.Threading.ThreadPriority.Normal" /> priority by
		/// default.</para></summary>
		Normal = 2,

		/// <summary><para> Threads with this priority may be scheduled after threads
		///       with <see cref="F:System.Threading.ThreadPriority.Highest" /> priority and before those with <see cref="F:System.Threading.ThreadPriority.Normal" /> priority.</para></summary>
		AboveNormal = 3,

		/// <summary><para><SPAN>Threads with this priority may be scheduled before threads with any
		///       other priority.</SPAN></para></summary>
		Highest = 4,
	} // ThreadPriority

} // System.Threading
