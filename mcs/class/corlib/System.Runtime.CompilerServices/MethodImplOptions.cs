// MethodImplOptions.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.CompilerServices {


	/// <summary>
	/// <para>Defines the details of how a method is implemented.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by <see cref="T:System.Runtime.CompilerServices.MethodImplAttribute" />.
	///    </para>
	/// </remarks>
	public enum MethodImplOptions {

		/// <summary><para>Specifies that the method is implemented in unmanaged code.</para></summary>
		Unmanaged = 4,

		/// <summary><para> Specifies that the method is declared, but its implementation is
		///       provided elsewhere. </para><para><block subset="none" type="note">For most languages, it is recommended 
		///       that the notion of "forward" be attached to methods using language syntax
		///       instead of custom attributes. </block></para></summary>
		ForwardRef = 16,

		/// <summary><para>Specifies an internal call.</para><para><block subset="none" type="note">An internal call
		///  is a call to a method implemented within the system itself, providing
		///  additional functionality that regular managed code cannot provide. <see cref="!:System.Object.MemberWiseClone" />
		///  is an example of an internally called method.</block></para></summary>
		InternalCall = 4096,

		/// <summary><para>Specifies the method can be executed by only one thread at a time.</para><para>This option specifies that before a thread can execute the target method, the
		///       thread is required to acquire a lock on either the current
		///       instance or the <see cref="T:System.Type" />
		///       object for the method's class. If the target method is an instance method, the
		///       lock is on the current instance. If the target is a static method, the lock is
		///       on the <see cref="T:System.Type" /> object. Specifying this option causes the target method to behave as though its
		///       statements are enclosed by <see cref="M:System.Threading.Monitor.Enter(System.Object)" /> and <see cref="M:System.Threading.Monitor.Exit(System.Object)" />
		///       statements locking the previous described object. This option and the <see cref="T:System.Threading.Monitor" /> methods are functionally equivalent, and both
		///       are functionally equivalent to enclosing the target method's code in a C# lock
		///       (this)
		///       statement. </para><block subset="none" type="note"><para>Because this option holds the lock for
		///          the duration of the target method, it should be used only when the entire method
		///          must be single threaded. Use the <see cref="T:System.Threading.Monitor" /> methods (or the C#
		///          lock statement) if the object lock can be taken after the method begins, or
		///          released before the method ends. Any mechanism that uses locks can cause an
		///          application to experience deadlocks and performance degradation; for these
		///          reasons, use this option with care.</para><para> For most languages, it is recommended that the notion of
		///          "synchronized" be attached to methods using language syntax instead of custom
		///          attributes.</para></block></summary>
		Synchronized = 32,

		/// <summary><para>Specifies that the method is not permitted to be inlined.</para></summary>
		NoInlining = 8,
	} // MethodImplOptions

} // System.Runtime.CompilerServices
