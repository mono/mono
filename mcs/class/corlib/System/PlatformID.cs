// PlatformID.cs
//
// This code was originally generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System {


	/// <summary>
	///   Describes the platforms supported by an
	///   assembly. These flags are used to bind to an assembly.
	/// </summary>
	public enum PlatformID {

		/// <summary>
		///   The operating system is Win32s running on a 16-bit version of Windows.
		///   The Win32s is a layer used on 16-bit versions of Windows to provide
		///   access to 32-bit applications.
		/// </summary>
		Win32S = 0,

		/// <summary>
		///   Determines whether the operating system is Windows 95 or later.
		/// </summary>
		Win32Windows = 1,

		/// <summary>
		///   <para>
		///     Determines whether the operating system is Windows NT.
		///   </para>
		/// </summary>
		Win32NT = 2,

		///
		/// <summary>
		/// <para>
		Unix = 128
	} // PlatformID

} // System
