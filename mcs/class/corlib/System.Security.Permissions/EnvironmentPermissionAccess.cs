// EnvironmentPermissionAccess.cs
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
	/// <para> Represents access to
	///  environment variables.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="none" type="note">This enumeration is used by the <see cref="T:System.Security.Permissions.EnvironmentPermission" />
	/// class.</block>
	/// </para>
	/// </remarks>
	[Flags]
	public enum EnvironmentPermissionAccess {

		/// <summary><para> Specifies no access to one or more environment variables.</para></summary>
		NoAccess = 0x00000000,

		/// <summary><para> 
		///       Specifies read access to one or more environment variables</para><para><block subset="none" type="note">Changing, deleting and creating 
		///       environment variables is not included in this access level.</block></para></summary>
		Read = 0x00000001,

		/// <summary><para> Specifies write access to one or more environment variables. Write
		///       access includes creating and deleting environment variables as well as changing existing values.</para><para><block subset="none" type="note">Reading environment variables is not 
		///       included in this access level.</block></para></summary>
		Write = 0x00000002,

		/// <summary><para>Specifies read and write access to one or more environment variables.</para></summary>
		AllAccess = Read | Write,
	} // EnvironmentPermissionAccess

} // System.Security.Permissions
