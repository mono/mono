// IsolatedStorageScope.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.IO.IsolatedStorage {


	/// <summary>
	/// <para>
	///                   Enumerates the levels of isolated storage scope
	///                   supported. Combinations of the levels may be specified
	///                </para>
	/// </summary>
	/// <remarks>
	/// <para>
	///                   Use IsolatedStorageScope to specify the scoping to designate an isolated
	///                   storage context for storing data files.
	///                </para>
	/// </remarks>
	public enum IsolatedStorageScope {

		/// <summary>
		/// <para>
		///                   Indicates no isolated storage usage.
		///                </para>
		/// </summary>
		None = 0,

		/// <summary>
		/// <para>
		///                   Indicates isolated storage scoped by user identity.
		///                </para>
		/// </summary>
		User = 1,

		/// <summary>
		/// <para>
		///                   Indicates isolated storage scoped to the application domain identity.
		///                </para>
		/// </summary>
		Domain = 2,

		/// <summary>
		/// <para>
		///                   Indicates isolated storage scoped to the identity of the assembly.
		///                </para>
		/// </summary>
		Assembly = 4,
	} // IsolatedStorageScope

} // System.IO.IsolatedStorage
