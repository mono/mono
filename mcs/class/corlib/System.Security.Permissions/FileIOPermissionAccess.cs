// FileIOPermissionAccess.cs
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
	/// <para> Represents access to files and
	///  directories.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="none" type="note">This enumeration is used by the <see cref="T:System.Security.Permissions.FileIOPermission" /> class. </block>
	/// </para>
	/// </remarks>
	[Flags]
	public enum FileIOPermissionAccess {

		/// <summary><para> Specifies no access to a file or directory.
		///  </para></summary>
		NoAccess = 0x00000000,

		/// <summary>
		///  Specifies read access to a file or directory.
		/// </summary>
		Read = 0x00000001,

		/// <summary><para> Specifies write access to a file or
		///  directory. Write access includes deleting and overwriting files or directories.</para></summary>
		Write = 0x00000002,

		/// <summary><para> Specifies append access to a file or directory. Append
		///  access includes the ability to create a new file or directory.</para></summary>
		Append = 0x00000004,

		/// <summary><para> Specifies access to the path information for a file or directory. <see cref="F:System.Security.Permissions.FileIOPermissionAccess.PathDiscovery" /> does not
		///  include access to the
		///  contents of a file or directory.</para><para><block subset="none" type="note">This permission is used to protect
		///  sensitive information in the path, such as user names, as well as information
		///  about the directory structure revealed in the
		///  path. </block></para></summary>
		PathDiscovery = 0x00000008,

		/// <summary><para>Specifies <see cref="F:System.Security.Permissions.FileIOPermissionAccess.Append" />, <see cref="F:System.Security.Permissions.FileIOPermissionAccess.Read" />
		/// , <see cref="F:System.Security.Permissions.FileIOPermissionAccess.Write" />, and
		/// <see cref="F:System.Security.Permissions.FileIOPermissionAccess.PathDiscovery" /> access to a file or directory.</para></summary>
		AllAccess = Read | Write | Append | PathDiscovery,
	} // FileIOPermissionAccess

} // System.Security.Permissions
