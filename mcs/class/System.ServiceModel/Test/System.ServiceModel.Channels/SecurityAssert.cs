//
// SecurityAssert.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	public static class SecurityAssert
	{
		public static void AssertLocalClientSecuritySettings (
			bool cacheCookies,
			int renewalThresholdPercentage,
			bool detectReplays,
			LocalClientSecuritySettings lc, string label)
		{
			Assert.IsNotNull (lc, label + " IsNotNull");
			Assert.AreEqual (cacheCookies, lc.CacheCookies, label + ".CacheCookies");
			Assert.AreEqual (renewalThresholdPercentage, lc.CookieRenewalThresholdPercentage, label + ".CookieRenewalThresholdPercentage");
			Assert.AreEqual (detectReplays, lc.DetectReplays, label + ".DetectReplays");
		}

		public static void AssertSecurityTokenParameters (
			SecurityTokenInclusionMode protectionTokenInclusionMode,
			SecurityTokenReferenceStyle protectionTokenReferenceStyle,
			bool protectionTokenRequireDerivedKeys,
			SecurityTokenParameters tp, string label)
		{
			Assert.IsNotNull (tp, label + " IsNotNull");
			Assert.AreEqual (protectionTokenInclusionMode,
				tp.InclusionMode, label + ".InclusionMode");
			Assert.AreEqual (protectionTokenReferenceStyle,
				tp.ReferenceStyle, label + ".ReferenceStyle");
			Assert.AreEqual (protectionTokenRequireDerivedKeys,
				tp.RequireDerivedKeys, label + ".RequireDerivedKeys");
		}

		public static void AssertSupportingTokenParameters (
			int endorsing, int signed, int signedEncrypted, int signedEndorsing,
			SupportingTokenParameters tp, string label)
		{
			Assert.IsNotNull (tp, label + " IsNotNull");
			Assert.AreEqual (endorsing, tp.Endorsing.Count, label + ".Endoring.Count");
			Assert.AreEqual (signed, tp.Signed.Count, label + ".Signed.Count");
			Assert.AreEqual (signedEncrypted, tp.SignedEncrypted.Count, label + ".SignedEncrypted.Count");
			Assert.AreEqual (signedEndorsing, tp.SignedEndorsing.Count, label + ".SignedEndorsing.Count");
		}

		public static void AssertSecurityBindingElement (
			SecurityAlgorithmSuite algorithm,
			bool includeTimestamp,
			SecurityKeyEntropyMode keyEntropyMode,
			MessageSecurityVersion messageSecurityVersion,
			SecurityHeaderLayout securityHeaderLayout,
			// EndpointSupportingTokenParameters
			int endorsing, int signed, int signedEncrypted, int signedEndorsing,
			// LocalClientSettings
			bool cacheCookies,
			int renewalThresholdPercentage,
			bool detectReplays,
			SecurityBindingElement be, string label)
		{
			Assert.AreEqual (algorithm, be.DefaultAlgorithmSuite, label + ".DefaultAlgorithmSuite");
			Assert.AreEqual (includeTimestamp, be.IncludeTimestamp, label + ".KeyEntropyMode");
			Assert.AreEqual (keyEntropyMode,
				be.KeyEntropyMode, label + "#3");

			Assert.AreEqual (messageSecurityVersion,
				be.MessageSecurityVersion, label + ".MessageSecurityVersion");
			Assert.AreEqual (securityHeaderLayout,
				be.SecurityHeaderLayout, label + ".SecurityHeaderLayout");

			// FIXME: they should be extracted step by step...

			// EndpointSupportingTokenParameters
			SupportingTokenParameters tp = be.EndpointSupportingTokenParameters;
			AssertSupportingTokenParameters (
				endorsing, signed, signedEncrypted, signedEndorsing,
				tp, label + ".Endpoint");

			// OptionalEndpointSupportingTokenParameters
			tp = be.OptionalEndpointSupportingTokenParameters;
			Assert.IsNotNull (tp, label + "#3-0");
			Assert.AreEqual (0, tp.Endorsing.Count, label + "#3-1");
			Assert.AreEqual (0, tp.Signed.Count, label + "#3-2");
			Assert.AreEqual (0, tp.SignedEncrypted.Count, label + "#3-3");
			Assert.AreEqual (0, tp.SignedEndorsing.Count, label + "#3-4");

			// OperationSupportingTokenParameters
			IDictionary<string,SupportingTokenParameters> oper = be.OperationSupportingTokenParameters;
			Assert.IsNotNull (oper, label + "#4-1");
			Assert.AreEqual (0, oper.Count, label + "#4-2");

			// OptionalOperationSupportingTokenParameters
			oper = be.OptionalOperationSupportingTokenParameters;
			Assert.IsNotNull (oper, label + "#5-1");
			Assert.AreEqual (0, oper.Count, label + "#5-2");

			// LocalClientSettings
			LocalClientSecuritySettings lc =
				be.LocalClientSettings;
			AssertLocalClientSecuritySettings (
				cacheCookies,
				renewalThresholdPercentage,
				detectReplays,
				lc, "");
			// FIXME: IdentityVerifier
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.MaxClockSkew, label + "#7-5");
			Assert.AreEqual (TimeSpan.MaxValue, lc.MaxCookieCachingTime, label + "#7-6");
			Assert.AreEqual (true, lc.ReconnectTransportOnFailure, label + "#7-7");
			Assert.AreEqual (900000, lc.ReplayCacheSize, label + "#7-8");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.ReplayWindow, label + "#7-9");
			Assert.AreEqual (TimeSpan.FromHours (10), lc.SessionKeyRenewalInterval, label + "#7-10");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.SessionKeyRolloverInterval, label + "#7-11");
			Assert.AreEqual (TimeSpan.FromMinutes (5), lc.TimestampValidityDuration, label + "#7-12");

			// FIXME: LocalServiceSettings
		}

		public static void AssertSymmetricSecurityBindingElement (
			SecurityAlgorithmSuite algorithm,
			bool includeTimestamp,
			SecurityKeyEntropyMode keyEntropyMode,
			MessageProtectionOrder messageProtectionOrder,
			MessageSecurityVersion messageSecurityVersion,
			bool requireSignatureConfirmation,
			SecurityHeaderLayout securityHeaderLayout,
			// EndpointSupportingTokenParameters
			int endorsing, int signed, int signedEncrypted, int signedEndorsing,
			// ProtectionTokenParameters
			bool hasProtectionTokenParameters,
			SecurityTokenInclusionMode protectionTokenInclusionMode,
			SecurityTokenReferenceStyle protectionTokenReferenceStyle,
			bool protectionTokenRequireDerivedKeys,
			// LocalClientSettings
			bool cacheCookies,
			int renewalThresholdPercentage,
			bool detectReplays,
			SymmetricSecurityBindingElement be, string label)
		{
			AssertSecurityBindingElement (
				algorithm,
				includeTimestamp,
				keyEntropyMode,
				messageSecurityVersion,
				securityHeaderLayout,
				// EndpointSupportingTokenParameters
				endorsing, signed, signedEncrypted, signedEndorsing,
				// LocalClientSettings
				cacheCookies,
				renewalThresholdPercentage,
				detectReplays,
				be, label);

			Assert.AreEqual (messageProtectionOrder, be.MessageProtectionOrder, label + ".MessageProtectionOrder");
			Assert.AreEqual (requireSignatureConfirmation, be.RequireSignatureConfirmation, label + ".RequireSignatureConfirmation");

			if (!hasProtectionTokenParameters)
				Assert.IsNull (be.ProtectionTokenParameters, label + ".ProtectionTokenParameters (null)");
			else
				AssertSecurityTokenParameters (
					protectionTokenInclusionMode,
					protectionTokenReferenceStyle,
					protectionTokenRequireDerivedKeys,
					be.ProtectionTokenParameters, label + ".ProtectionTokenParameters");
		}

		public static void AssertAsymmetricSecurityBindingElement (
			SecurityAlgorithmSuite algorithm,
			bool includeTimestamp,
			SecurityKeyEntropyMode keyEntropyMode,
			MessageProtectionOrder messageProtectionOrder,
			MessageSecurityVersion messageSecurityVersion,
			bool requireSignatureConfirmation,
			SecurityHeaderLayout securityHeaderLayout,
			// EndpointSupportingTokenParameters
			int endorsing, int signed, int signedEncrypted, int signedEndorsing,
			// InitiatorTokenParameters
			bool hasInitiatorTokenParameters,
			SecurityTokenInclusionMode initiatorTokenInclusionMode,
			SecurityTokenReferenceStyle initiatorTokenReferenceStyle,
			bool initiatorTokenRequireDerivedKeys,
			// RecipientTokenParameters
			bool hasRecipientTokenParameters,
			SecurityTokenInclusionMode recipientTokenInclusionMode,
			SecurityTokenReferenceStyle recipientTokenReferenceStyle,
			bool recipientTokenRequireDerivedKeys,
			// LocalClientSettings
			bool cacheCookies,
			int renewalThresholdPercentage,
			bool detectReplays,
			AsymmetricSecurityBindingElement be, string label)
		{
			AssertSecurityBindingElement (
				algorithm,
				includeTimestamp,
				keyEntropyMode,
				messageSecurityVersion,
				securityHeaderLayout,
				// EndpointSupportingTokenParameters
				endorsing, signed, signedEncrypted, signedEndorsing,
				// LocalClientSettings
				cacheCookies,
				renewalThresholdPercentage,
				detectReplays,
				be, label);

			Assert.AreEqual (messageProtectionOrder, be.MessageProtectionOrder, label + ".MessageProtectionOrder");
			Assert.AreEqual (requireSignatureConfirmation, be.RequireSignatureConfirmation, label + ".RequireSignatureConfirmation");

			if (!hasInitiatorTokenParameters)
				Assert.IsNull (be.InitiatorTokenParameters, label + ".InitiatorTokenParameters (null)");
			else
				AssertSecurityTokenParameters (
					initiatorTokenInclusionMode,
					initiatorTokenReferenceStyle,
					initiatorTokenRequireDerivedKeys,
					be.InitiatorTokenParameters, label + ".InitiatorTokenParameters");
			if (!hasRecipientTokenParameters)
				Assert.IsNull (be.RecipientTokenParameters, label + ".RecipientTokenParameters (null)");
			else
				AssertSecurityTokenParameters (
					recipientTokenInclusionMode,
					recipientTokenReferenceStyle,
					recipientTokenRequireDerivedKeys,
					be.RecipientTokenParameters, label + ".RecipientTokenParameters");
		}
	}
}
#endif