// AssemblyNameFlags.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:38:33 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection {


	/// <summary>
	/// </summary>
	[Flags]
	[Serializable]
	public enum AssemblyNameFlags {

		/// <summary>
		/// </summary>
		None = 0,

		/// <summary>
		///  Not sure about the ECMA spec, but this is what is in mscorlib...
		///  Perhaps this has changed since the beta.
		/// </summary>
		PublicKey = 1,

#if NET_1_1
		Retargetable = 256
#endif
	} // AssemblyNameFlags

} // System.Reflection
