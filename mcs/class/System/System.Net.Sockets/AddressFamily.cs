// AddressFamily.cs
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
	/// <para>Specifies the addressing schemes used by the <see cref="T:System.Net.Sockets.Socket" /> class.</para>
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="T:System.Net.Sockets.AddressFamily" /> member is required when instantiating the
	/// <see cref="T:System.Net.Sockets.Socket" /> class 
	///    to specify the addressing scheme that the instance uses to resolve an address. For example,
	/// <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> indicates that an IP version 4 address is 
	///    expected when a <see cref="T:System.Net.Sockets.Socket" />
	///    instance connects to an endpoint.</para>
	/// </remarks>
	public enum AddressFamily {

		/// <summary><para>Used to indicate an uninitialized state. This member is not to be 
		///       used when instantiating the <see cref="T:System.Net.Sockets.Socket" /> class.</para></summary>
		Unknown = -1,

		/// <summary><para>Unspecified address family.</para></summary>
		Unspecified = 0,

		/// <summary><para>Address is local to the host.</para></summary>
		Unix = 1,

		/// <summary><para> Address for IP version 4.</para></summary>
		InterNetwork = 2,

		/// <summary><para>ARPANET IMP address.</para></summary>
		ImpLink = 3,

		/// <summary><para>Address for PUP protocols.</para></summary>
		Pup = 4,

		/// <summary><para>Address for MIT CHAOS protocols.</para></summary>
		Chaos = 5,

		/// <summary><para>Address for Xerox NS protocols.</para></summary>
		NS = 6,

		/// <summary><para>Internetwork Packet Exchange (IPX) or Sequenced Packet 
		///  Exchange (SPX) address.</para></summary>
		Ipx = 6,

		/// <summary><para>Address for ISO protocols.</para><block subset="none" type="note"><para>Multiple names are defined for this value based on prior art. 
		///          This value is identical to <see cref="F:System.Net.Sockets.AddressFamily.Osi" />.</para></block></summary>
		Iso = 7,

		/// <summary><para>Address for ISO protocols.</para><block subset="none" type="note"><para>Multiple names are defined for this value based on prior art. 
		///          This value is identical to <see cref="F:System.Net.Sockets.AddressFamily.Iso" />.</para></block></summary>
		Osi = 7,

		/// <summary><para>European Computer Manufacturers Association (ECMA) address.</para></summary>
		Ecma = 8,

		/// <summary><para>Address for Datakit protocols.</para></summary>
		DataKit = 9,

		/// <summary><para>Addresses for CCITT protocols, such as X.25.</para></summary>
		Ccitt = 10,

		/// <summary><para>IBM SNA address.</para></summary>
		Sna = 11,

		/// <summary><para> DECnet address.</para></summary>
		DecNet = 12,

		/// <summary><para> Direct data-link interface address.</para></summary>
		DataLink = 13,

		/// <summary><para> LAT address.</para></summary>
		Lat = 14,

		/// <summary><para> NSC Hyperchannel address.</para></summary>
		HyperChannel = 15,

		/// <summary><para> AppleTalk address.</para></summary>
		AppleTalk = 16,

		/// <summary><para> NetBios address.</para></summary>
		NetBios = 17,

		/// <summary><para>VoiceView address.</para></summary>
		VoiceView = 18,

		/// <summary><para> FireFox address.</para></summary>
		FireFox = 19,

		/// <summary><para> Banyan address.</para></summary>
		Banyan = 21,

		/// <summary><para>Native Asynchronous Transfer Mode (ATM) services address.</para></summary>
		Atm = 22,

		/// <summary><para>Address for IP version 6.</para></summary>
		InterNetworkV6 = 23,

		/// <summary><para>Address for Microsoft cluster products.</para></summary>
		Cluster = 24,

		/// <summary><para> IEEE 1284.4 workgroup address.</para></summary>
		Ieee12844 = 25,

		/// <summary><para> Infrared Data Association (IrDA) address.</para></summary>
		Irda = 26,

		/// <summary><para> Address for Network Designers OSI gateway-enabled protocols.</para></summary>
		NetworkDesigners = 28,
	} // AddressFamily

} // System.Net.Sockets
