// ProtocolType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:32:24 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


namespace System.Net.Sockets {


	/// <summary>
	/// </summary>
	public enum ProtocolType {

		/// <summary>
		/// </summary>
		IP = 0,

		/// <summary>
		/// </summary>
		Icmp = 1,

		/// <summary>
		/// </summary>
		Igmp = 2,

		/// <summary>
		/// </summary>
		Ggp = 3,

		/// <summary>
		/// </summary>
		Tcp = 6,

		/// <summary>
		/// </summary>
		Pup = 12,

		/// <summary>
		/// </summary>
		Udp = 17,

		/// <summary>
		/// </summary>
		Idp = 22,

		/// <summary>
		/// </summary>
		IPv6 = 41,

		/// <summary>
		/// </summary>
		ND = 77,

		/// <summary>
		/// </summary>
		Raw = 255,

		/// <summary>
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// </summary>
		Ipx = 1000,

		/// <summary>
		/// </summary>
		Spx = 1256,

		/// <summary>
		/// </summary>
		SpxII = 1257,

		/// <summary>
		/// </summary>
		Unknown = -1,

		IPv4 = 4,
		IPv6RoutingHeader = 43,
		IPv6FragmentHeader = 44,
		IPSecEncapsulatingSecurityPayload = 50,
		IPSecAuthenticationHeader = 51,
		IcmpV6 = 58,
		IPv6NoNextHeader = 59,
		IPv6DestinationOptions = 60,
		IPv6HopByHopOptions = 0,
	} // ProtocolType

} // System.Net.Sockets
