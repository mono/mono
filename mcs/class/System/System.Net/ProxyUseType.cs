// ProxyUseType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net {

	public enum ProxyUseType {

		/// <summary>
		/// <para>
		///                   Specifies that the proxy is a standard proxy.
		///                </para>
		/// </summary>
		Standard = 0,

		/// <summary>
		/// <para>
		///                   Specifies that the proxy is a tunneling proxy.
		///                </para>
		/// </summary>
		Tunnel = 1,

		/// <summary>
		/// <para>
		///                   Specifies that the proxy is an FTP gateway.
		///                </para>
		/// </summary>
		FtpGateway = 2,

		/// <summary>
		/// <para>
		///                   Specifies that the proxy is a SOCKS server.
		///                </para>
		/// </summary>
		Socks = 3,
	} // ProxyUseType

} // System.Net
