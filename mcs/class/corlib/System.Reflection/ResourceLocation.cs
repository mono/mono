// ResourceLocation.cs
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
	/// <para>Specifies the resource location.</para>
	/// </summary>
	[Flags]
	public enum ResourceLocation {

		/// <summary>
		/// <para>Specifies an embedded (that is, non-linked) resource.</para>
		/// </summary>
		Embedded = 1,

		/// <summary>
		/// <para>Specifies that the resource is contained in another assembly.</para>
		/// </summary>
		ContainedInAnotherAssembly = 2,

		/// <summary>
		/// <para>Specifies that the resource is contained in the manifest file.</para>
		/// </summary>
		ContainedInManifestFile = 4,
	} // ResourceLocation

} // System.Reflection
