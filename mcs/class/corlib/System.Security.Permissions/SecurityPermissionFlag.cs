// SecurityPermissionFlag.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// <para>Specifies a set of security permissions applied to a
	///    <see cref="T:System.Security.Permissions.SecurityPermission" /> instance.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by <see cref="T:System.Security.Permissions.SecurityPermission" />.</para>
	/// <para>
	/// <see cref="T:System.Security.Permissions.SecurityPermissionFlag" /> is a bitfield. Specify
	/// multiple values of this enumeration using the bit-wise OR
	/// operator.</para>
	/// <para>
	/// <block subset="ECMAOnly" type="ECMA Only">For more information on security, see Partition 
	///    II of the CLI Specification.</block>
	/// </para>
	/// <para>
	/// <block subset="none" type="note">Many of these flags are powerful and
	///    should only be granted to highly trusted code. </block>
	/// </para>
	/// </remarks>
	[Flags]
	public enum SecurityPermissionFlag {

		/// <summary><para> Specifies that none of the permissions in this 
		///       enumeration are available.</para></summary>
		NoFlags = 0x00000000,

		/// <summary><para> Specifies the ability to assert<see langword=" " />
		/// that all of this code's
		/// callers have one
		/// or more permissions.</para></summary>
		Assertion = 0x00000001,

		/// <summary><para> Specifies the ability to call unmanaged code.</para><block subset="none" type="note"><para> 
		///          Because unmanaged code potentially allows other permissions to be bypassed,
		///          this permission should be used with caution. It is used for applications calling native
		///          code using PInvoke.</para></block></summary>
		UnmanagedCode = 0x00000002,

		/// <summary><para>Specifies the ability to skip verification of code in an assembly.</para><para><block subset="none" type="note">Code that is unverifiable can execute if 
		///       this permission is granted.</block></para></summary>
		SkipVerification = 0x00000004,

		/// <summary><para> Specifies permission for the code to run. Without this
		///  permission managed code cannot execute.</para></summary>
		Execution = 0x00000008,

		/// <summary><para> Specifies the ability to use advanced operations on 
		///       threads. These operations include <see cref="M:System.Threading.Thread.Abort(System.Object)" />
		///       and <see cref="M:System.Threading.Thread.ResetAbort" /> .</para></summary>
		ControlThread = 0x00000010,

		/// <summary><para>Specifies all of the permissions defined by this
		///       enumeration.</para><para><block subset="ECMAOnly" type="ECMA Only">The value of this constant is equal to the
		///       values of this enumeration combined with a bitwise-OR operation. The higher-order bits in
		///       this constant that are not specified in the CLI have been reserved for implementation-specific
		///       use.</block></para></summary>
		AllFlags = Assertion | UnmanagedCode | SkipVerification | Execution | ControlThread,
	} // SecurityPermissionFlag

} // System.Security.Permissions
