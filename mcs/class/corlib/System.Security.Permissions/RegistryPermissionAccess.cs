// RegistryPermissionAccess.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Security.Permissions {


	/// <summary>
	/// <para> 
	///                   Specifies the permitted access to registry keys and
	///                   values.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="!:System.Security.Permissions.RegistryPermissionFlag" /> 
	///             values are independent; rights to one type of access do not
	///             imply rights to another. For instance, <see langword="Write" /> permission does
	///             not imply permission to <see langword="Read" /> or <see langword="Create" />. </para>
	/// </remarks>
	public enum RegistryPermissionAccess {

		/// <summary>
		/// <para> No access to registry variables.</para>
		/// </summary>
		NoAccess = 0,

		/// <summary>
		/// <para> Read access to registry variables.</para>
		/// </summary>
		Read = 1,

		/// <summary>
		/// <para> Write access to registry variables.</para>
		/// </summary>
		Write = 2,

		/// <summary>
		/// <para> Create access to registry variables.</para>
		/// </summary>
		Create = 4,

		/// <summary>
		/// <para>Create, read and write access to registry variables.</para>
		/// </summary>
		AllAccess = 7,
	} // RegistryPermissionAccess

} // System.Security.Permissions
