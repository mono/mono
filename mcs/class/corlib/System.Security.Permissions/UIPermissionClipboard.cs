// UIPermissionClipboard.cs
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
	/// <para>Specifies the types of clipboard access allowed to the calling code.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by <see cref="T:System.Security.Permissions.UIPermission" />.</para>
	/// </remarks>
	public enum UIPermissionClipboard {

		/// <summary>
		/// <para>Clipboard cannot be used.</para>
		/// </summary>
		NoClipboard = 0,

		/// <summary>
		/// <para>
		///                   Clipboard can only be used to paste content from the same application domain.
		///                   The ability to put data on the clipboard (Copy, Cut) is unrestricted.
		///                </para>
		/// </summary>
		OwnClipboard = 1,

		/// <summary>
		/// <para>
		///                   Clipboard can be used without restriction.
		///                </para>
		/// </summary>
		AllClipboard = 2,
	} // UIPermissionClipboard

} // System.Security.Permissions
