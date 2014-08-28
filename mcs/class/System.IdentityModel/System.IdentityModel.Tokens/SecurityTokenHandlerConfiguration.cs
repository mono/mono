using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace System.IdentityModel.Tokens
{
	public class SecurityTokenHandlerConfiguration
	{
		public static readonly X509CertificateValidationMode DefaultCertificateValidationMode;
		public static readonly X509CertificateValidator DefaultCertificateValidator;
		public static readonly bool DefaultDetectReplayedTokens;
		public static readonly IssuerNameRegistry DefaultIssuerNameRegistry;
		public static readonly SecurityTokenResolver DefaultIssuerTokenResolver;
		public static readonly TimeSpan DefaultMaxClockSkew;
		public static readonly X509RevocationMode DefaultRevocationMode;
		public static readonly bool DefaultSaveBootstrapContext;
		public static readonly TimeSpan DefaultTokenReplayCacheExpirationPeriod;
		public static readonly StoreLocation DefaultTrustedStoreLocation;

		public AudienceRestriction AudienceRestriction { get; set; }
		public IdentityModelCaches Caches { get; set; }
		public X509CertificateValidationMode CertificateValidationMode { get; set; }
		public X509CertificateValidator CertificateValidator { get; set; }
		public bool DetectReplayedTokens { get; set; }
		public IssuerNameRegistry IssuerNameRegistry { get; set; }
		public SecurityTokenResolver IssuerTokenResolver { get; set; }
		public TimeSpan MaxClockSkew { get; set; }
		public X509RevocationMode RevocationMode { get; set; }
		public bool SaveBootstrapContext { get; set; }
		public SecurityTokenResolver ServiceTokenResolver { get; set; }
		public TimeSpan TokenReplayCacheExpirationPeriod { get; set; }
		public StoreLocation TrustedStoreLocation { get; set; }
	}
}