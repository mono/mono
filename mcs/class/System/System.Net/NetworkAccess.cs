// NetworkAccess.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net {


	/// <summary>
	/// <para> Specifies network access permission types.</para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration is used to indicate whether a 
	///       permission object secures connect (client-side) or accept (server-side)
	///       operations.</para>
	/// <para>
	/// <block subset="none" type="note">The <see cref="T:System.Net.NetworkAccess" /> enumeration is used with the <see cref="T:System.Net.WebPermission" /> and <see cref="T:System.Net.SocketPermission" /> classes.</block>
	/// </para>
	/// </remarks>
	public enum NetworkAccess {

		/// <summary><para> Specifies accept operations.
		///       </para><para><block subset="none" type="note">This access type is typically used by
		///       servers. </block></para></summary>
		Accept = 128,

		/// <summary><para> Specifies connect operations.
		///       </para><para><block subset="none" type="note">This access type is typically used by clients. </block></para></summary>
		Connect = 64,
	} // NetworkAccess

} // System.Net
