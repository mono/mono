// SeekOrigin.cs
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
	/// <para> Defines the seek reference positions.</para>
	/// </summary>
	/// <remarks>
	/// <para> The <see cref="T:System.IO.SeekOrigin" /> enumeration is used by the overrides of the <see cref="M:System.IO.Stream.Seek(System.Int64,System.IO.SeekOrigin)" qualify="true" /> method to set
	///    the seek reference point in a stream, which allows you to specify an offset from
	///    the reference point.</para>
	/// </remarks>
	public enum SeekOrigin {

		/// <summary><para> Indicates that the seek reference point is the beginning of a
		///       stream.</para></summary>
		Begin = 0,

		/// <summary><para> Indicates that the seek reference point is the current position
		///       within a stream.</para></summary>
		Current = 1,

		/// <summary><para> Indicates that the seek reference point is the first byte beyond the end of a stream.</para></summary>
		End = 2,
	} // SeekOrigin

} // System.IO
