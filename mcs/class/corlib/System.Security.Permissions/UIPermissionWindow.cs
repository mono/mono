// UIPermissionWindow.cs
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
	/// <para>Specifies the type of windows that code is allowed to use.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used by <see cref="T:System.Security.Permissions.UIPermission" />.</para>
	/// </remarks>
	public enum UIPermissionWindow {

		/// <summary>
		/// <para>Users have no ability to use any windows or user interface events. Restricted to being
		///                   without user interface.</para>
		/// </summary>
		NoWindows = 0,

		/// <summary>
		/// <para>Users can only use safe subwindows for drawing, and can only use user input events for user
		///                   interface.</para>
		/// </summary>
		SafeSubWindows = 1,

		/// <summary>
		/// <para>Users can only use safe top-level windows for drawing, and can only use user input events for the
		///                   user interface. Special safe windows for use by partially trusted code are guaranteed
		///                   to be clearly labeled and have minimum and maximum size restrictions.
		///                   These restrictions prevent potentially malicious code from spoofing, such as
		///                   imitating trusted system dialogs.</para>
		/// </summary>
		SafeTopLevelWindows = 2,

		/// <summary>
		/// <para>
		///                   Users can use all windows and user input events without restriction.
		///                </para>
		/// </summary>
		AllWindows = 3,
	} // UIPermissionWindow

} // System.Security.Permissions
