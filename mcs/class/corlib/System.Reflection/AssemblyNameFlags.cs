// AssemblyNameFlags.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// <para> Provides information about an <see cref="T:System.Reflection.Assembly" /> .</para>
	/// </summary>
	[Flags]
	public enum AssemblyNameFlags {

		/// <summary>
		/// <para>Specifies that no flags are in effect.</para>
		/// </summary>
		None = 0,

		/// <summary>
		/// <para> Specifies that an originator is formed from the full public key rather than the token.</para>
		/// </summary>
		FullOriginator = 1,
	} // AssemblyNameFlags

} // System.Reflection
