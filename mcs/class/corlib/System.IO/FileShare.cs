// FileShare.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.IO {


	/// <summary>
	/// <para> Specifies the level of
	///       access permitted for a file that is already in use.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used to specify the way
	///       in which multiple threads access the same file. The level of access is set by the
	///       first thread that requests access to the file. For example, if a thread opens a
	///       file and specifies <see langword="FileShare.Read" />,
	///       other
	///       threads are permitted to open the file for reading but not for writing.</para>
	/// </remarks>
	[Flags]
	public enum FileShare {

		/// <summary><para> Specifies that the file may not be accessed by
		///       additional threads.</para></summary>
		None = 0x00000000,

		/// <summary><para> Specifies that additional threads can
		///       share read access to the file. This value does not determine whether such
		///       access is granted, however.</para></summary>
		Read = 0x00000001,

		/// <summary><para> 
		///       Specifies that additional threads can share write access to the file. This value does
		///       not determine whether such access is granted, however.
		///       </para></summary>
		Write = 0x00000002,

		/// <summary><para> Specifies that additional threads can
		///       share read and/or write access to the file. This value does not determine whether
		///       such access is granted, however.</para></summary>
		ReadWrite = Read | Write,
	} // FileShare

} // System.IO
