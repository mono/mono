// FileMode.cs
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
	/// <para>Specifies how the operating system should open a file.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see langword="FileMode" /> values specify whether a
	///    file is created if one does not exist, and determines whether the contents of existing
	///    files are retained or overwritten.</para>
	/// </remarks>
	public enum FileMode {

		/// <summary><para> Requests a new file be created. An exception
		///       is thrown if the file already exists.</para></summary>
		CreateNew = 1,

		/// <summary><para> Requests a new file be created if it does not exist. The 
		///       file contents are overwritten if it does exist. This value is equivalent to
		///       requesting that if the file does not exist, use <see cref="F:System.IO.FileMode.CreateNew" />;
		///       otherwise, use <see cref="F:System.IO.FileMode.Truncate" />.</para></summary>
		Create = 2,

		/// <summary><para>Requests an existing file be opened. An exception is
		///       thrown if the file does not exist.</para></summary>
		Open = 3,

		/// <summary><para> Requests a file be opened. The file is
		///       created if it does not exist.</para></summary>
		OpenOrCreate = 4,

		/// <summary><para> Requests an existing file be opened; existing contents
		///       are deleted. This value is valid only for <see cref="F:System.IO.FileAccess.Write" qualify="true" />
		///       access. Attempts to read from a file opened with <see langword="Truncate" />
		///       cause
		///       an exception.</para></summary>
		Truncate = 5,

		/// <summary><para> Requests a file be opened. If the file exists, its contents
		///       are preserved. This value is valid only for <see cref="F:System.IO.FileAccess.Write" qualify="true" />
		///       access. Attempts to read from a file opened with <see langword="Append" />
		///       cause an
		///       exception.</para></summary>
		Append = 6,
	} // FileMode

} // System.IO
