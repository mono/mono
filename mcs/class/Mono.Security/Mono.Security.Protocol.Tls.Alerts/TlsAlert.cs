/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using Mono.Security.Protocol.Tls;

namespace Mono.Security.Protocol.Tls.Alerts
{
	#region ENUMS

	public enum TlsAlertLevel : byte
	{
		Warning = 1,
		Fatal	= 2
	}

	public enum TlsAlertDescription : byte
	{
		CloseNotify				= 0,
		UnexpectedMessage		= 10,
		BadRecordMAC			= 20,
		DecryptionFailed		= 21,
		RecordOverflow			= 22,
		DecompressionFailiure	= 30,
		HandshakeFailiure		= 40,
		BadCertificate			= 42,
		UnsupportedCertificate	= 43,
		CertificateRevoked		= 44,
		CertificateExpired		= 45,
		CertificateUnknown		= 46,
		IlegalParameter			= 47,
		UnknownCA				= 48,
		AccessDenied			= 49,
		DecodeError				= 50,
		DecryptError			= 51,
		ExportRestriction		= 60,
		ProtocolVersion			= 70,
		InsuficientSecurity		= 71,
		InternalError			= 80,
		UserCancelled			= 90,
		NoRenegotiation			= 100
	}

	#endregion
	
	internal abstract class TlsAlert : TlsStream
	{
		#region FIELDS

		private TlsSession			session;
		private TlsAlertLevel		level;
		private TlsAlertDescription description;

		#endregion

		#region PROPERTIES

		public TlsSession Session
		{
			get { return session; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsAlert(TlsSession session,
			TlsAlertLevel level,
			TlsAlertDescription description) : base()
		{
			this.session		= session;
			this.level			= level;
			this.description	= description;

			this.fill();
		}

		#endregion

		#region ABSTRACT_METHODS

		public abstract void UpdateSession();

		#endregion

		#region CONSTRUCTORS

		private void fill()
		{
			Write((byte)level);
			Write((byte)description);
		}

		#endregion

		#region STATIC_METHODS

		internal static string GetAlertMessage(TlsAlertDescription description)
		{
			switch (description)
			{
				case TlsAlertDescription.AccessDenied:
					return "An inappropriate message was received.";

				case TlsAlertDescription.BadCertificate:
					return "TLSCiphertext decrypted in an invalid way.";

				case TlsAlertDescription.BadRecordMAC:
					return "Record with an incorrect MAC.";

				case TlsAlertDescription.CertificateExpired:
					return "Certificate has expired or is not currently valid";

				case TlsAlertDescription.CertificateRevoked:
					return "Certificate was revoked by its signer.";
					
				case TlsAlertDescription.CertificateUnknown:
					return "Certificate Unknown.";

				case TlsAlertDescription.CloseNotify:
					return "Connection closed";

				case TlsAlertDescription.DecodeError:
					return "A message could not be decoded because some field was out of the specified range or the length of the message was incorrect.";

				case TlsAlertDescription.DecompressionFailiure:
					return "The decompression function received improper input (e.g. data that would expand to excessive length).";

				case TlsAlertDescription.DecryptError:
					return "TLSCiphertext decrypted in an invalid way: either it wasn`t an even multiple of the block length or its padding values, when checked, weren`t correct.";

				case TlsAlertDescription.DecryptionFailed:
					return "Handshake cryptographic operation failed, including being unable to correctly verify a signature, decrypt a key exchange, or validate finished message.";

				case TlsAlertDescription.ExportRestriction:
					return "Negotiation not in compliance with export restrictions was detected.";

				case TlsAlertDescription.HandshakeFailiure:
					return "Unable to negotiate an acceptable set of security parameters given the options available.";

				case TlsAlertDescription.IlegalParameter:
					return "A field in the handshake was out of range or inconsistent with other fields.";
					
				case TlsAlertDescription.InsuficientSecurity:
					return "Negotiation has failed specifically because the server requires ciphers more secure than those supported by the client.";
					
				case TlsAlertDescription.InternalError:
					return "Internal error unrelated to the peer or the correctness of the protocol makes it impossible to continue.";

				case TlsAlertDescription.NoRenegotiation:
					return "Invalid renegotiation.";

				case TlsAlertDescription.ProtocolVersion:
					return "Unsupported protocol version.";

				case TlsAlertDescription.RecordOverflow:
					return "Invalid length on TLSCiphertext record or TLSCompressed record.";

				case TlsAlertDescription.UnexpectedMessage:
					return "Invalid message receive.";

				case TlsAlertDescription.UnknownCA:
					return "CA can't be identified as a trusted CA.";

				case TlsAlertDescription.UnsupportedCertificate:
					return "Certificate was of an unsupported type.";

				case TlsAlertDescription.UserCancelled:
					return "Handshake cancelled by user.";

				default:
					return "";
			}
		}

		#endregion
	}
}
