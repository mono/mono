//
// SecurityAlgorithmSuite.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Tokens;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	public abstract class SecurityAlgorithmSuite
	{
		#region Internal Class

		class BasicSecurityAlgorithmSuite : SecurityAlgorithmSuiteImplBase
		{
			public BasicSecurityAlgorithmSuite (int size, bool sha, bool rsa)
				: base (size, sha, rsa, false)
			{
			}

			public override int DefaultSignatureKeyDerivationLength {
				get { return Size > 192 ? 192 : Size; }
			}

			public override bool IsAsymmetricKeyLengthSupported (int length)
			{
				switch (length) {
				case 128:
				case 192:
					return Size >= length;
				}
				return false;
			}

			public override bool IsSymmetricKeyLengthSupported (int length)
			{
				switch (length) {
				case 128:
				case 192:
				case 256:
					return Size >= length;
				}
				return false;
			}

			public override bool IsSymmetricKeyWrapAlgorithmSupported (string algorithm)
			{
				switch (Size) {
				case 256:
					if (algorithm == EncryptedXml.XmlEncAES256KeyWrapUrl)
						return true;
					goto case 192;
				case 192:
					if (algorithm == EncryptedXml.XmlEncAES192KeyWrapUrl)
						return true;
					goto case 128;
				case 128:
					return algorithm == EncryptedXml.XmlEncAES128KeyWrapUrl;
				}
				return false;
			}
		}

		class TripleDESSecurityAlgorithmSuite : SecurityAlgorithmSuiteImplBase
		{
			public TripleDESSecurityAlgorithmSuite (bool sha, bool rsa)
				: base (192, sha, rsa, true)
			{
			}

			public override int DefaultSignatureKeyDerivationLength {
				get { return 192; }
			}

			public override bool IsAsymmetricKeyLengthSupported (int length)
			{
				return length == 192;
			}

			public override bool IsSymmetricKeyLengthSupported (int length)
			{
				return length == 192;
			}

			public override bool IsSymmetricKeyWrapAlgorithmSupported (
				string algorithm)
			{
				return algorithm == EncryptedXml.XmlEncTripleDESKeyWrapUrl;
			}
		}

		abstract class SecurityAlgorithmSuiteImplBase : SecurityAlgorithmSuite
		{
			int size;
			bool rsa15, sha256, tdes;

			public SecurityAlgorithmSuiteImplBase (
				int size, bool sha256, bool rsa15, bool tripleDes)
			{
				this.size = size;
				this.sha256 = sha256;
				this.rsa15 = rsa15;
				this.tdes = tripleDes;
			}

			public int Size {
				get { return size; }
			}

			public bool Rsa15 {
				get { return rsa15; }
			}

			public bool Sha256 {
				get { return sha256; }
			}

			public override string DefaultAsymmetricKeyWrapAlgorithm {
				get { return rsa15 ? EncryptedXml.XmlEncRSA15Url : EncryptedXml.XmlEncRSAOAEPUrl; }
			}

			public override string DefaultAsymmetricSignatureAlgorithm {
				get { return sha256 ? SecurityAlgorithms.RsaSha256Signature : SignedXml.XmlDsigRSASHA1Url; }
			}

			public override string DefaultCanonicalizationAlgorithm {
				get { return SignedXml.XmlDsigExcC14NTransformUrl; }
			}


			public override string DefaultDigestAlgorithm {
				get { return sha256 ? EncryptedXml.XmlEncSHA256Url : SignedXml.XmlDsigSHA1Url; }
			}

			public override string DefaultEncryptionAlgorithm {
				get {
					if (tdes)
						return EncryptedXml.XmlEncTripleDESUrl;
					switch (size) {
					case 128:
						return EncryptedXml.XmlEncAES128Url;
					case 192:
						return EncryptedXml.XmlEncAES192Url;
					case 256:
						return EncryptedXml.XmlEncAES256Url;
					}
					throw new Exception ("Should not happen.");
				}
			}

			public override int DefaultEncryptionKeyDerivationLength {
				get { return size; }
			}

			public override int DefaultSymmetricKeyLength {
				get { return size; }
			}

			public override string DefaultSymmetricKeyWrapAlgorithm {
				get {
					if (tdes)
						return EncryptedXml.XmlEncTripleDESKeyWrapUrl;
					switch (size) {
					case 128:
						return EncryptedXml.XmlEncAES128KeyWrapUrl;
					case 192:
						return EncryptedXml.XmlEncAES192KeyWrapUrl;
					case 256:
						return EncryptedXml.XmlEncAES256KeyWrapUrl;
					}
					throw new Exception ("Should not happen.");
				}
			}

			public override string DefaultSymmetricSignatureAlgorithm {
				get { return sha256 ? SecurityAlgorithms.HmacSha256Signature : SignedXml.XmlDsigHMACSHA1Url; }
			}

			[MonoTODO]
			public override bool IsAsymmetricSignatureAlgorithmSupported (
				string algorithm)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool IsCanonicalizationAlgorithmSupported (
				string algorithm)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool IsDigestAlgorithmSupported (string algorithm)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool IsEncryptionAlgorithmSupported (
				string algorithm)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool IsEncryptionKeyDerivationAlgorithmSupported (
				string algorithm)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool IsSignatureKeyDerivationAlgorithmSupported (
				string algorithm)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool IsSymmetricSignatureAlgorithmSupported (
				string algorithm)
			{
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region Static members

		static SecurityAlgorithmSuite b128, b128r, b128s, b128sr;
		static SecurityAlgorithmSuite b192, b192r, b192s, b192sr;
		static SecurityAlgorithmSuite b256, b256r, b256s, b256sr;
		static SecurityAlgorithmSuite tdes, tdes_r, tdes_s, tdes_sr;

		static SecurityAlgorithmSuite ()
		{
			b128 = new BasicSecurityAlgorithmSuite (128, false, false);
			b128r = new BasicSecurityAlgorithmSuite (128, false, true);
			b128s = new BasicSecurityAlgorithmSuite (128, true, false);
			b128sr = new BasicSecurityAlgorithmSuite (128, true, true);
			b192 = new BasicSecurityAlgorithmSuite (192, false, false);
			b192r = new BasicSecurityAlgorithmSuite (192, false, true);
			b192s = new BasicSecurityAlgorithmSuite (192, true, false);
			b192sr = new BasicSecurityAlgorithmSuite (192, true, true);
			b256 = new BasicSecurityAlgorithmSuite (256, false, false);
			b256r = new BasicSecurityAlgorithmSuite (256, false, true);
			b256s = new BasicSecurityAlgorithmSuite (256, true, false);
			b256sr = new BasicSecurityAlgorithmSuite (256, true, true);
			tdes = new TripleDESSecurityAlgorithmSuite (false, false);
			tdes_r = new TripleDESSecurityAlgorithmSuite (false, true);
			tdes_s = new TripleDESSecurityAlgorithmSuite (true, false);
			tdes_sr = new TripleDESSecurityAlgorithmSuite (true, true);
		}

		public static SecurityAlgorithmSuite Default {
			get { return Basic256; }
		}

		public static SecurityAlgorithmSuite Basic128 {
			get { return b128; }
		}

		public static SecurityAlgorithmSuite Basic128Rsa15 {
			get { return b128r; }
		}

		public static SecurityAlgorithmSuite Basic128Sha256 {
			get { return b128s; }
		}

		public static SecurityAlgorithmSuite Basic128Sha256Rsa15 {
			get { return b128sr; }
		}

		public static SecurityAlgorithmSuite Basic192 {
			get { return b192; }
		}

		public static SecurityAlgorithmSuite Basic192Rsa15 {
			get { return b192r; }
		}

		public static SecurityAlgorithmSuite Basic192Sha256 {
			get { return b192s; }
		}

		public static SecurityAlgorithmSuite Basic192Sha256Rsa15 {
			get { return b192sr; }
		}

		public static SecurityAlgorithmSuite Basic256 {
			get { return b256; }
		}

		public static SecurityAlgorithmSuite Basic256Rsa15 {
			get { return b256r; }
		}

		public static SecurityAlgorithmSuite Basic256Sha256 {
			get { return b256s; }
		}

		public static SecurityAlgorithmSuite Basic256Sha256Rsa15 {
			get { return b256sr; }
		}

		public static SecurityAlgorithmSuite TripleDes {
			get { return tdes; }
		}

		public static SecurityAlgorithmSuite TripleDesRsa15 {
			get { return tdes_r; }
		}

		public static SecurityAlgorithmSuite TripleDesSha256 {
			get { return tdes_s; }
		}

		public static SecurityAlgorithmSuite TripleDesSha256Rsa15 {
			get { return tdes_sr; }
		}

		#endregion

		#region Instance members

		protected SecurityAlgorithmSuite ()
		{
		}

		public abstract string DefaultAsymmetricKeyWrapAlgorithm { get; }

		public abstract string DefaultAsymmetricSignatureAlgorithm { get; }

		public abstract string DefaultCanonicalizationAlgorithm { get; }

		public abstract string DefaultDigestAlgorithm { get; }

		public abstract string DefaultEncryptionAlgorithm { get; }

		public abstract int DefaultEncryptionKeyDerivationLength { get; }

		public abstract int DefaultSignatureKeyDerivationLength { get; }

		public abstract int DefaultSymmetricKeyLength { get; }

		public abstract string DefaultSymmetricKeyWrapAlgorithm { get; }

		public abstract string DefaultSymmetricSignatureAlgorithm { get; }

			public virtual bool IsAsymmetricKeyWrapAlgorithmSupported (
				string algorithm)
			{
				return algorithm == DefaultAsymmetricKeyWrapAlgorithm;
			}

		public abstract bool IsAsymmetricKeyLengthSupported (int length);

		public virtual bool IsAsymmetricSignatureAlgorithmSupported (
			string algorithm)
		{
				return algorithm == DefaultAsymmetricSignatureAlgorithm;
		}

		[MonoTODO]
		public virtual bool IsCanonicalizationAlgorithmSupported (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsDigestAlgorithmSupported (string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsEncryptionAlgorithmSupported (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsEncryptionKeyDerivationAlgorithmSupported (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSignatureKeyDerivationAlgorithmSupported (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		public abstract bool IsSymmetricKeyLengthSupported (int length);

		[MonoTODO]
		public virtual bool IsSymmetricKeyWrapAlgorithmSupported (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSymmetricSignatureAlgorithmSupported (
			string algorithm)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
