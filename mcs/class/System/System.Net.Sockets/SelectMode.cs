// SelectMode.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net.Sockets {


	/// <summary>
	/// <para> Specifies the mode used by the <see cref="M:System.Net.Sockets.Socket.Poll(System.Int32,System.Net.Sockets.SelectMode)" /> method of
	///    the <see cref="T:System.Net.Sockets.Socket" /> class.
	///    </para>
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="T:System.Net.Sockets.SelectMode" /> member specifies the
	///    status information (read, write, or error) to retrieve from the
	///    current <see cref="T:System.Net.Sockets.Socket" /> instance.</para>
	/// </remarks>
	public enum SelectMode {

		/// <summary><para>Determine the read status of the current <see cref="T:System.Net.Sockets.Socket" /> 
		/// instance.</para></summary>
		SelectRead = 0,

		/// <summary><para>Determine the write status of the current <see cref="T:System.Net.Sockets.Socket" /> instance.</para></summary>
		SelectWrite = 1,

		/// <summary><para>Determine the error status of the current <see cref="T:System.Net.Sockets.Socket" /> 
		/// instance.</para></summary>
		SelectError = 2,
	} // SelectMode

} // System.Net.Sockets
