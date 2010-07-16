//
// TlsClientSession.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.IO;
using System.IdentityModel.Selectors;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Mono.Security.Protocol.Tls;
using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Client;

namespace System.ServiceModel.Security.Tokens
{
	internal abstract class TlsSession
	{
		protected abstract Context Context { get; }

		protected abstract RecordProtocol Protocol { get; }

		public byte [] MasterSecret {
			get { return Context.MasterSecret; }
		}

		public byte [] CreateHash (byte [] key, byte [] seedSrc, string label)
		{
			byte [] labelBytes = Encoding.UTF8.GetBytes (label);
			byte [] seed = new byte [seedSrc.Length + labelBytes.Length];
			Array.Copy (seedSrc, seed, seedSrc.Length);
			Array.Copy (labelBytes, 0, seed, seedSrc.Length, labelBytes.Length);
			return Context.Current.Cipher.Expand ("SHA1", key, seed, 256 / 8);
		}

		public byte [] CreateHashAlt (byte [] key, byte [] seed, string label)
		{
			return Context.Current.Cipher.PRF (key, label, seed, 256 / 8);
		}

		protected void WriteHandshake (MemoryStream ms)
		{
			Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers (SecurityProtocolType.Tls);
			ms.WriteByte (0x16); // Handshake
			ms.WriteByte (3); // version-major
			ms.WriteByte (1); // version-minor
		}

		protected void WriteChangeCipherSpec (MemoryStream ms)
		{
			ms.WriteByte (0x14); // Handshake
			ms.WriteByte (3); // version-major
			ms.WriteByte (1); // version-minor
			ms.WriteByte (0); // size-upper
			ms.WriteByte (1); // size-lower
			ms.WriteByte (1); // ChangeCipherSpec content (1 byte)
		}

		protected void ReadHandshake (MemoryStream ms)
		{
			if (ms.ReadByte () != 0x16)
				throw new Exception ("INTERNAL ERROR: handshake is expected");
			Context.ChangeProtocol ((short) (ms.ReadByte () * 0x100 + ms.ReadByte ()));
		}

		protected void ReadChangeCipherSpec (MemoryStream ms)
		{
			if (ms.ReadByte () != 0x14)
				throw new Exception ("INTERNAL ERROR: ChangeCipherSpec is expected");
			Context.ChangeProtocol ((short) (ms.ReadByte () * 0x100 + ms.ReadByte ()));
			if (ms.ReadByte () * 0x100 + ms.ReadByte () != 1)
				throw new Exception ("INTERNAL ERROR: unexpected ChangeCipherSpec length");
			ms.ReadByte (); // ChangeCipherSpec content (1 byte) ... anything is OK?
		}

		protected byte [] ReadNextOperation (MemoryStream ms, HandshakeType expected)
		{
			if (ms.ReadByte () != (int) expected)
				throw new Exception ("INTERNAL ERROR: unexpected server response");
			int size = ms.ReadByte () * 0x10000 + ms.ReadByte () * 0x100 + ms.ReadByte ();
			// FIXME: use correct valid input range
			if (size > 0x100000)
				throw new Exception ("rejected massive input size.");
			byte [] bytes = new byte [size];
			ms.Read (bytes, 0, size);
			return bytes;
		}

		protected void WriteOperations (MemoryStream ms, params HandshakeMessage [] msgs)
		{
			List<byte []> rawbufs = new List<byte []> ();
			int total = 0;
			for (int i = 0; i < msgs.Length; i++) {
				HandshakeMessage msg = msgs [i];
				msg.Process ();
				rawbufs.Add (msg.EncodeMessage ());
				total += rawbufs [i].Length;
				msg.Update ();
			}
			// FIXME: split packets when the size exceeded 0x10000 (or so)
			ms.WriteByte ((byte) (total / 0x100));
			ms.WriteByte ((byte) (total % 0x100));
			foreach (byte [] bytes in rawbufs)
				ms.Write (bytes, 0, bytes.Length);
		}

		protected void VerifyEndOfTransmit (MemoryStream ms)
		{
			if (ms.Position == ms.Length)
				return;

			/*
			byte [] bytes = new byte [ms.Length - ms.Position];
			ms.Read (bytes, 0, bytes.Length);
			foreach (byte b in bytes)
				Console.Write ("{0:X02} ", b);
			Console.WriteLine (" - total {0} bytes remained.", bytes.Length);
			*/

			throw new Exception ("INTERNAL ERROR: unexpected server response");
		}
	}

	internal class TlsClientSession : TlsSession
	{
		SslClientStream ssl;
		MemoryStream stream;
		bool mutual;

		public TlsClientSession (string host, X509Certificate2 clientCert, X509ServiceCertificateAuthentication auth)
		{
			stream = new MemoryStream ();
			if (clientCert == null)
				ssl = new SslClientStream (stream, host, true, SecurityProtocolType.Tls);
			else {
				ssl = new SslClientStream (stream, host, true, SecurityProtocolType.Tls, new X509CertificateCollection (new X509Certificate [] {clientCert}));
				mutual = true;
				ssl.ClientCertSelection += delegate (
					X509CertificateCollection clientCertificates,
				X509Certificate serverCertificate,
				string targetHost,
				X509CertificateCollection serverRequestedCertificates) {
					return clientCertificates [0];
				};
			}
			X509CertificateValidator v = null;
			switch (auth.CertificateValidationMode) {
			case X509CertificateValidationMode.None:
				v = X509CertificateValidator.None;
				break;
			case X509CertificateValidationMode.PeerTrust:
				v = X509CertificateValidator.PeerTrust;
				break;
			case X509CertificateValidationMode.ChainTrust:
				v = X509CertificateValidator.ChainTrust;
				break;
			case X509CertificateValidationMode.PeerOrChainTrust:
				v = X509CertificateValidator.PeerOrChainTrust;
				break;
			case X509CertificateValidationMode.Custom:
				v = auth.CustomCertificateValidator;
				break;
			}
			ssl.ServerCertValidationDelegate = delegate (X509Certificate certificate, int [] certificateErrors) {
				v.Validate (new X509Certificate2 (certificate)); // will throw SecurityTokenvalidationException if invalid.
				return true;
				};
		}

		protected override Context Context {
			get { return ssl.context; }
		}

		protected override RecordProtocol Protocol {
			get { return ssl.protocol; }
		}

		public byte [] ProcessClientHello ()
		{
			Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers (Context.SecurityProtocol);
			Context.HandshakeState = HandshakeState.Started;
			Protocol.SendRecord (HandshakeType.ClientHello);
			stream.Flush ();
			return stream.ToArray ();
		}

		// ServerHello, ServerCertificate and ServerHelloDone
		public void ProcessServerHello (byte [] raw)
		{
			stream.SetLength (0);
			stream.Write (raw, 0, raw.Length);
			stream.Seek (0, SeekOrigin.Begin);

//foreach (var b in raw) Console.Write ("{0:X02} ", b); Console.WriteLine ();

#if false // FIXME: should be enabled, taking some right shape.
			ReadNextOperation (stream, HandshakeType.ServerHello);
			ReadNextOperation (stream, HandshakeType.Certificate);
			if (mutual)
				ReadNextOperation (stream, HandshakeType.CertificateRequest);
			ReadNextOperation (stream, HandshakeType.ServerHelloDone);
#else
			Protocol.ReceiveRecord (stream); // ServerHello
			Protocol.ReceiveRecord (stream); // ServerCertificate
			if (mutual)
				Protocol.ReceiveRecord (stream); // CertificateRequest
			Protocol.ReceiveRecord (stream); // ServerHelloDone
#endif
			if (stream.Position != stream.Length)
				throw new SecurityNegotiationException (String.Format ("Unexpected SSL negotiation binary: {0} bytes of excess in {1} bytes of the octets", stream.Length - stream.Position, stream.Length));
		}

		public byte [] ProcessClientKeyExchange ()
		{
			stream.SetLength (0);
			if (mutual)
				Protocol.SendRecord (HandshakeType.Certificate);
			Protocol.SendRecord (HandshakeType.ClientKeyExchange);
			Context.Negotiating.Cipher.ComputeKeys ();
			Context.Negotiating.Cipher.InitializeCipher ();
			Protocol.SendChangeCipherSpec ();
			Context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers (SecurityProtocolType.Tls);
			Protocol.SendRecord (HandshakeType.Finished);
			stream.Flush ();
			return stream.ToArray ();
		}

		public void ProcessServerFinished (byte [] raw)
		{
			stream.SetLength (0);
			stream.Write (raw, 0, raw.Length);
			stream.Seek (0, SeekOrigin.Begin);

			Protocol.ReceiveRecord (stream); // ChangeCipherSpec
			Protocol.ReceiveRecord (stream); // ServerFinished
		}

		public byte [] ProcessApplicationData (byte [] raw)
		{
			stream.SetLength (0);
			stream.Write (raw, 0, raw.Length);
			stream.Seek (0, SeekOrigin.Begin);
			return Protocol.ReceiveRecord (stream); // ApplicationData
		}
	}
}
