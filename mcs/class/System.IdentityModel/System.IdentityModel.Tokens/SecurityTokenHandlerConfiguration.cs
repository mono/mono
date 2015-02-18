//
// SecurityTokenHandlerConfiguration.cs
//
// Author:
//   Noesis Labs (Ryan.Melena@noesislabs.com)
//
// Copyright (C) 2014 Noesis Labs, LLC  https://noesislabs.com
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
