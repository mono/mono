// UriHostNameType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System {


	/// <summary>
	/// <para>Specifies the format of host names.</para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// <block subset="none" type="note">The <see cref="T:System.UriHostNameType" /> enumeration defines the values returned by the <see cref="M:System.Uri.CheckHostName(System.String)" qualify="true" /> 
	/// method.</block>
	/// </para>
	/// </remarks>
	public enum UriHostNameType {

		/// <summary><para>Specifies the format of a host name is not known or no host information is present.</para></summary>
		Unknown = 0,

		/// <summary><para>Specifies that the host name is a domain name system (DNS) style host address.</para></summary>
		Dns = 2,

		/// <summary><para>Specifies that the host name is an Internet Protocol (IP) version 4 host address.</para></summary>
		IPv4 = 3,

		/// <summary><para>Specifies that the host name is an Internet Protocol (IP)
		///  version 6 host address.</para></summary>
		IPv6 = 4,
	} // UriHostNameType

} // System
