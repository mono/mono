// WebStatus.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: all.xml
// URL: http://devresource.hp.com/devresource/Docs/TechPapers/CSharp/all.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Net {


	/// <summary>
	/// <para>
	///                   Specifies the status of a network request.
	///                </para>
	/// </summary>
	public enum WebStatus {

		/// <summary>
		/// <para>
		///                   No error was encountered.
		///                </para>
		/// </summary>
		Success = 0,

		/// <summary>
		/// <para>
		///                   The name resolver service could not resolve the host name.
		///                </para>
		/// </summary>
		NameResolutionFailure = 1,

		/// <summary>
		/// <para>
		///                   The remote service point could not be contacted at the transport level.
		///                </para>
		/// </summary>
		ConnectFailure = 2,

		/// <summary>
		/// <para>
		///                   A complete response was not received from the remote server.
		///                </para>
		/// </summary>
		ReceiveFailure = 3,

		/// <summary>
		/// <para>
		///                   A complete request could not be sent to the remote server.
		///                </para>
		/// </summary>
		SendFailure = 4,

		/// <summary>
		/// </summary>
		PipelineFailure = 5,

		/// <summary>
		/// <para>
		///                   The request was cancelled.
		///                </para>
		/// </summary>
		RequestCanceled = 6,

		/// <summary>
		/// <para>
		///                   The response received from the server was complete but indicated a
		///                   protocol-level error. For example, an HTTP protocol error such as 401 Access
		///                   Denied would use this status.
		///                </para>
		/// </summary>
		ProtocolError = 7,

		/// <summary>
		/// <para>
		///                   The connection was prematurely closed.
		///                </para>
		/// </summary>
		ConnectionClosed = 8,

		/// <summary>
		/// <para>
		///                   A server certificate could not be validated.
		///                </para>
		/// </summary>
		TrustFailure = 9,

		/// <summary>
		/// <para>
		///                   An error occurred in a secure channel link.
		///                </para>
		/// </summary>
		SecureChannelFailure = 10,
		ServerProtocolViolation = 11,
		KeepAliveFailure = 12,
		Pending = 13,
	} // WebStatus

} // System.Net
