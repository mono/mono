// ReflectionPermissionFlag.cs
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
	/// <para> Represents levels of access to the metadata for 
	///       non-public types accessed using reflection.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="none" type="note">This enumeration is used by <see cref="T:System.Security.Permissions.ReflectionPermission" />
	/// . </block>
	/// </para>
	/// </remarks>
	[Flags]
	public enum ReflectionPermissionFlag {

		/// <summary><para>Specifies no access to non-public types or information about non-public
		///       types using reflection.</para><para><block subset="none" type="note">When this access level is granted via a <see cref="T:System.Security.Permissions.ReflectionPermission" />, only 
		///    those elements in metadata that can be accessed using early binding are
		///    accessible.</block></para></summary>
		NoFlags = 0x00000000,

		/// <summary><para> Specifies access to information about non-public types using 
		///       reflection.</para></summary>
		TypeInformation = 0x00000001,

		/// <summary><para> Specifies access
		///       to members of non-public types using reflection. Access includes the ability to perform operations on the members.</para></summary>
		MemberAccess = 0x00000002,

		/// <summary><para>Specifies the <see cref="F:System.Security.Permissions.ReflectionPermissionFlag.TypeInformation" /> 
		/// and <see cref="F:System.Security.Permissions.ReflectionPermissionFlag.MemberAccess" />
		/// values.</para></summary>
		AllFlags = TypeInformation | MemberAccess,
	} // ReflectionPermissionFlag

} // System.Security.Permissions
