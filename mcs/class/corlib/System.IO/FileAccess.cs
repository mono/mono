// FileAccess.cs
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
	/// <para> Defines constants used to specify the level of file access being requested.</para>
	/// </summary>
	[Flags]
	public enum FileAccess {

		/// <summary><para> Specifies read access for a file.</para></summary>
		Read = 0x00000001,

		/// <summary><para> Specifies write access for
		///       a file.</para></summary>
		Write = 0x00000002,

		/// <summary><para> 
		///       Specifies read and write access for a file.</para></summary>
		ReadWrite = Read | Write,
	} // FileAccess

} // System.IO
