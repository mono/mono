// StreamingContextStates.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:45:18 UTC
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.Serialization {


	/// <summary>
	/// </summary>
	[Flags]
	public enum StreamingContextStates {

		/// <summary>
		/// </summary>
		CrossProcess = 1,

		/// <summary>
		/// </summary>
		CrossMachine = 2,

		/// <summary>
		/// </summary>
		File = 4,

		/// <summary>
		/// </summary>
		Persistence = 8,

		/// <summary>
		/// </summary>
		Remoting = 16,

		/// <summary>
		/// </summary>
		Other = 32,

		/// <summary>
		/// </summary>
		Clone = 64,

		CrossAppDomain = 128,

		/// <summary>
		/// </summary>
		All = 255,
	} // StreamingContextStates

} // System.Runtime.Serialization
