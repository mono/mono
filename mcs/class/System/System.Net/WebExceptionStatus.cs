// WebExceptionStatus.cs
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
	/// <para> Defines status codes for the <see cref="T:System.Net.WebException" /> class.
	///  </para>
	/// </summary>
	/// <remarks>
	/// <para>This enumeration defines the status
	///  codes assigned to the <see cref="P:System.Net.WebException.Status" />
	///  property.</para>
	/// </remarks>
	public enum WebExceptionStatus {

		/// <summary><para> No error was encountered.
		///  </para><para><block subset="none" type="note">This is the default value for
		///  <see cref="P:System.Net.WebException.Status" /> . </block></para></summary>
		Success = 0,

		/// <summary><para>
		///        The name resolver service could not resolve the host name.
		///     </para></summary>
		NameResolutionFailure = 1,

		/// <summary><para> The remote service point could not be contacted at the transport level.
		///  </para></summary>
		ConnectFailure = 2,

		/// <summary><para>
		///        A complete response was not received from the remote server.
		///     </para></summary>
		ReceiveFailure = 3,

		/// <summary><para>
		///        A complete request could not be sent to the remote server.
		///     </para></summary>
		SendFailure = 4,

		/// <summary></summary>
		PipelineFailure = 5,

		/// <summary><para> The request was canceled or the <see cref="M:System.Net.WebRequest.Abort" qualify="true" /> method was called.
		///    </para></summary>
		RequestCanceled = 6,

		/// <summary><para> 
		///  The response received from the server was complete
		///  but indicated a protocol-level error.
		///  </para><para><block subset="none" type="note">For example, an HTTP protocol error such 
		///  as 401 Access Denied would use this status. </block></para></summary>
		ProtocolError = 7,

		/// <summary><para>
		///        The connection was prematurely closed.
		///     </para></summary>
		ConnectionClosed = 8,

		/// <summary><para>
		///        A server certificate could not be validated.
		///     </para></summary>
		TrustFailure = 9,

		/// <summary><para>
		///        An error occurred in a secure channel link.
		///     </para></summary>
		SecureChannelFailure = 10,

		/// <summary><para>The server response was not a valid HTTP response.</para></summary>
		ServerProtocolViolation = 11,

		/// <summary><para>The connection for a request that specifies the Keep-alive 
		///       header was closed unexpectedly.</para></summary>
		KeepAliveFailure = 12,

		/// <summary><para> An internal asynchronous request is pending.</para></summary>
		Pending = 13,

		/// <summary><para>No response was received during the timeout period for a request.</para></summary>
		Timeout = 14,

		/// <summary><para>The name resolver service could not resolve the proxy host name.</para></summary>
		ProxyNameResolutionFailure = 15,
	} // WebExceptionStatus

} // System.Net
