// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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

using System;
using Mono.Security.Protocol.Tls;

namespace Mono.Security.Protocol.Tls
{
	#region Enumerations

	[Serializable]
	internal enum AlertLevel : byte
	{
		Warning = 1,
		Fatal	= 2
	}

	[Serializable]
	internal enum AlertDescription : byte
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
	
	internal class Alert
	{
		#region Fields

		private AlertLevel			level;
		private AlertDescription	description;

		#endregion

		#region Properties

		public AlertLevel Level
		{
			get { return this.level; }
		}

		public AlertDescription Description
		{
			get { return this.description; }
		}

		public string Message
		{
			get { return Alert.GetAlertMessage(this.description); }
		}

		public bool IsWarning
		{
			get { return this.level == AlertLevel.Warning ? true : false; }
		}

		/*
		public bool IsFatal
		{
			get { return this.level == AlertLevel.Fatal ? true : false; }
		}
		*/

		public bool IsCloseNotify
		{
			get
			{
				if (this.IsWarning &&
					this.description == AlertDescription.CloseNotify)
				{
					return true;
				}

				return false;
			}
		}

		#endregion

		#region Constructors

		public Alert(AlertDescription description)
		{
			this.inferAlertLevel();
			this.description = description;
		}

		public Alert(
			AlertLevel			level,
			AlertDescription	description)
		{
			this.level			= level;
			this.description	= description;
		}

		#endregion

		#region Private Methods

		private void inferAlertLevel()
		{
			switch (description)
			{
				case AlertDescription.CloseNotify:
				case AlertDescription.NoRenegotiation:
				case AlertDescription.UserCancelled:
					this.level = AlertLevel.Warning;
					break;

				case AlertDescription.AccessDenied:
				case AlertDescription.BadCertificate:
				case AlertDescription.BadRecordMAC:
				case AlertDescription.CertificateExpired:
				case AlertDescription.CertificateRevoked:
				case AlertDescription.CertificateUnknown:
				case AlertDescription.DecodeError:
				case AlertDescription.DecompressionFailiure:
				case AlertDescription.DecryptError:
				case AlertDescription.DecryptionFailed:
				case AlertDescription.ExportRestriction:
				case AlertDescription.HandshakeFailiure:
				case AlertDescription.IlegalParameter:
				case AlertDescription.InsuficientSecurity:
				case AlertDescription.InternalError:
				case AlertDescription.ProtocolVersion:
				case AlertDescription.RecordOverflow:
				case AlertDescription.UnexpectedMessage:
				case AlertDescription.UnknownCA:
				case AlertDescription.UnsupportedCertificate:
				default:
					this.level = AlertLevel.Fatal;
					break;
			}
		}
		
		#endregion

		#region Static Methods

		public static string GetAlertMessage(AlertDescription description)
		{
			#if (DEBUG)
			switch (description)
			{
				case AlertDescription.AccessDenied:
					return "An inappropriate message was received.";

				case AlertDescription.BadCertificate:
					return "TLSCiphertext decrypted in an invalid way.";

				case AlertDescription.BadRecordMAC:
					return "Record with an incorrect MAC.";

				case AlertDescription.CertificateExpired:
					return "Certificate has expired or is not currently valid";

				case AlertDescription.CertificateRevoked:
					return "Certificate was revoked by its signer.";
					
				case AlertDescription.CertificateUnknown:
					return "Certificate Unknown.";

				case AlertDescription.CloseNotify:
					return "Connection closed";

				case AlertDescription.DecodeError:
					return "A message could not be decoded because some field was out of the specified range or the length of the message was incorrect.";

				case AlertDescription.DecompressionFailiure:
					return "The decompression function received improper input (e.g. data that would expand to excessive length).";

				case AlertDescription.DecryptError:
					return "TLSCiphertext decrypted in an invalid way: either it wasn`t an even multiple of the block length or its padding values, when checked, weren`t correct.";

				case AlertDescription.DecryptionFailed:
					return "Handshake cryptographic operation failed, including being unable to correctly verify a signature, decrypt a key exchange, or validate finished message.";

				case AlertDescription.ExportRestriction:
					return "Negotiation not in compliance with export restrictions was detected.";

				case AlertDescription.HandshakeFailiure:
					return "Unable to negotiate an acceptable set of security parameters given the options available.";

				case AlertDescription.IlegalParameter:
					return "A field in the handshake was out of range or inconsistent with other fields.";
					
				case AlertDescription.InsuficientSecurity:
					return "Negotiation has failed specifically because the server requires ciphers more secure than those supported by the client.";
					
				case AlertDescription.InternalError:
					return "Internal error unrelated to the peer or the correctness of the protocol makes it impossible to continue.";

				case AlertDescription.NoRenegotiation:
					return "Invalid renegotiation.";

				case AlertDescription.ProtocolVersion:
					return "Unsupported protocol version.";

				case AlertDescription.RecordOverflow:
					return "Invalid length on TLSCiphertext record or TLSCompressed record.";

				case AlertDescription.UnexpectedMessage:
					return "Invalid message received.";

				case AlertDescription.UnknownCA:
					return "CA can't be identified as a trusted CA.";

				case AlertDescription.UnsupportedCertificate:
					return "Certificate was of an unsupported type.";

				case AlertDescription.UserCancelled:
					return "Handshake cancelled by user.";

				default:
					return "";
			}
			#else
			return "The authentication or decryption has failed.";
			#endif
		}

		#endregion
	}
}
