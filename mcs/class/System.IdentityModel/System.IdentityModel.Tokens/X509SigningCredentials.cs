using System;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Tokens
{
	public class X509SigningCredentials : SigningCredentials
	{
		public X509Certificate2 Certificate { get; private set; }

		public X509SigningCredentials (X509Certificate2 certificate)
			: this (certificate, X509SigningCredentials.GetSecurityKeyIdentifier (certificate), SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest)
		{ }

		public X509SigningCredentials (X509Certificate2 certificate, SecurityKeyIdentifier ski)
			: this (certificate, ski, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest)
		{ }

		public X509SigningCredentials (X509Certificate2 certificate, string signatureAlgorithm, string digestAlgorithm)
			: this (certificate, X509SigningCredentials.GetSecurityKeyIdentifier (certificate), signatureAlgorithm, digestAlgorithm)
		{ }

		public X509SigningCredentials (X509Certificate2 certificate, SecurityKeyIdentifier ski, string signatureAlgorithm, string digestAlgorithm)
			: base (new X509SecurityToken (certificate).SecurityKeys[0], signatureAlgorithm, digestAlgorithm, ski)
		{
			Certificate = certificate;
		}

		private static SecurityKeyIdentifier GetSecurityKeyIdentifier (X509Certificate2 certificate) {
			return new SecurityKeyIdentifier (new X509SecurityToken (certificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause> ());
		}
	}
}