// PermissionState.cs
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
	/// <para> Represents a value specifying whether an entity, at
	///  creation, should have full or no access to resources.</para>
	/// </summary>
	/// <remarks>
	/// <block subset="none" type="note">
	/// <para> Code access permission objects supply a constructor that takes a <see cref="T:System.Security.Permissions.PermissionState" /> value specifying that the new
	///  instance is either fully restricted (<see cref="F:System.Security.Permissions.PermissionState.None" />) or unrestricted (<see cref="F:System.Security.Permissions.PermissionState.Unrestricted" />). A fully restricted permission
	///  object disallows access to a resource; an unrestricted permission object allows
	///  full access to a resource. For example, a fully restricted <see cref="T:System.Security.Permissions.FileIOPermission" /> object disallows access to files
	///  and directories, while an unrestricted object of the same
	///  
	///  type allows full access to all
	///  files and directories in the file system.</para>
	/// </block>
	/// </remarks>
	public enum PermissionState {

		/// <summary><para> Specifies full access to the resource protected by the permission.</para></summary>
		Unrestricted = 1,

		/// <summary><para>Specifies access to the resource protected
		///  by the permission is not allowed.</para></summary>
		None = 0,
	} // PermissionState

} // System.Security.Permissions
