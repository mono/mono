//
// ServiceBehaviorElementTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Configuration;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class ServiceBehaviorElementTest
	{
		ServiceBehaviorElement OpenConfig () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/serviceBehaviors")).GetSectionGroup ("system.serviceModel");
			return config.Behaviors.ServiceBehaviors [0];
		}

		[Test]
		public void ServiceAuthorizationElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceAuthorizationElement serviceAuthorization = (ServiceAuthorizationElement) behavior [typeof (ServiceAuthorizationElement)];

			if (serviceAuthorization == null)
				Assert.Fail ("ServiceAuthorizationElement is not exist in collection.");

			Assert.AreEqual (typeof (ServiceAuthorizationBehavior), serviceAuthorization.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceAuthorization", serviceAuthorization.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual ("RoleProvider", serviceAuthorization.RoleProviderName, "RoleProviderName");
			Assert.AreEqual (PrincipalPermissionMode.UseAspNetRoles, serviceAuthorization.PrincipalPermissionMode, "PrincipalPermissionMode");
			Assert.AreEqual (true, serviceAuthorization.ImpersonateCallerForAllOperations, "ImpersonateCallerForAllOperations");
			Assert.AreEqual ("SerAuthManagType", serviceAuthorization.ServiceAuthorizationManagerType, "ServiceAuthorizationManagerType");

			Assert.AreEqual (2, serviceAuthorization.AuthorizationPolicies.Count, "AuthorizationPolicies.Count");
			Assert.AreEqual ("PolicyType1", serviceAuthorization.AuthorizationPolicies [0].PolicyType, "AuthorizationPolicies[0].PolicyType");
			Assert.AreEqual ("PolicyType2", serviceAuthorization.AuthorizationPolicies [1].PolicyType, "AuthorizationPolicies[1].PolicyType");
		}

		[Test]
		public void ServiceAuthorizationElement_default () {
			ServiceAuthorizationElement serviceAuthorization = new ServiceAuthorizationElement ();

			Assert.AreEqual (typeof (ServiceAuthorizationBehavior), serviceAuthorization.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceAuthorization", serviceAuthorization.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (String.Empty, serviceAuthorization.RoleProviderName, "RoleProviderName");
			Assert.AreEqual (PrincipalPermissionMode.UseWindowsGroups, serviceAuthorization.PrincipalPermissionMode, "PrincipalPermissionMode");
			Assert.AreEqual (false, serviceAuthorization.ImpersonateCallerForAllOperations, "ImpersonateCallerForAllOperations");
			Assert.AreEqual (String.Empty, serviceAuthorization.ServiceAuthorizationManagerType, "ServiceAuthorizationManagerType");

			Assert.AreEqual (0, serviceAuthorization.AuthorizationPolicies.Count, "AuthorizationPolicies.Count");
		}

		[Test]
		public void DataContractSerializerElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			DataContractSerializerElement element = (DataContractSerializerElement) behavior [typeof (DataContractSerializerElement)];

			if (element == null)
				Assert.Fail ("DataContractSerializerElement is not exist in collection.");

			Assert.AreEqual ("System.ServiceModel.Dispatcher.DataContractSerializerServiceBehavior", element.BehaviorType.FullName, "BehaviorType");
			Assert.AreEqual ("dataContractSerializer", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.IgnoreExtensionDataObject, "IgnoreExtensionDataObject");
			Assert.AreEqual (32768, element.MaxItemsInObjectGraph, "MaxItemsInObjectGraph");
		}

		[Test]
		public void DataContractSerializerElement_defaults () {
			DataContractSerializerElement element = new DataContractSerializerElement ();

			Assert.AreEqual ("System.ServiceModel.Dispatcher.DataContractSerializerServiceBehavior", element.BehaviorType.FullName, "BehaviorType");
			Assert.AreEqual ("dataContractSerializer", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (false, element.IgnoreExtensionDataObject, "IgnoreExtensionDataObject");
			Assert.AreEqual (65536, element.MaxItemsInObjectGraph, "MaxItemsInObjectGraph");
		}

		[Test]
		public void ServiceDebugElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceDebugElement element = (ServiceDebugElement) behavior [typeof (ServiceDebugElement)];

			if (element == null)
				Assert.Fail ("ServiceDebugElement is not exist in collection.");

			Assert.AreEqual (typeof (ServiceDebugBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceDebug", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (false, element.HttpHelpPageEnabled, "HttpHelpPageEnabled");
			Assert.AreEqual ("http://help.page.url", element.HttpHelpPageUrl.OriginalString, "HttpHelpPageUrl");
			Assert.AreEqual (false, element.HttpsHelpPageEnabled, "HttpsHelpPageEnabled");
			Assert.AreEqual ("https://help.page.url", element.HttpsHelpPageUrl.OriginalString, "HttpsHelpPageUrl");
			Assert.AreEqual (true, element.IncludeExceptionDetailInFaults, "IncludeExceptionDetailInFaults");
		}

		[Test]
		public void ServiceDebugElement_defaults () {
			ServiceDebugElement element = new ServiceDebugElement ();

			Assert.AreEqual (typeof (ServiceDebugBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceDebug", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.HttpHelpPageEnabled, "HttpHelpPageEnabled");
			Assert.AreEqual (null, element.HttpHelpPageUrl, "HttpHelpPageUrl");
			Assert.AreEqual (true, element.HttpsHelpPageEnabled, "HttpsHelpPageEnabled");
			Assert.AreEqual (null, element.HttpsHelpPageUrl, "HttpsHelpPageUrl");
			Assert.AreEqual (false, element.IncludeExceptionDetailInFaults, "IncludeExceptionDetailInFaults");
		}

		[Test]
		public void ServiceMetadataPublishingElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceMetadataPublishingElement element = (ServiceMetadataPublishingElement) behavior [typeof (ServiceMetadataPublishingElement)];

			if (element == null)
				Assert.Fail ("ServiceMetadataPublishingElement is not exist in collection.");

			Assert.AreEqual (typeof (ServiceMetadataBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceMetadata", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.HttpGetEnabled, "HttpGetEnabled");
			Assert.AreEqual ("http://get.url", element.HttpGetUrl.OriginalString, "HttpGetUrl");
			Assert.AreEqual (true, element.HttpsGetEnabled, "HttpsGetEnabled");
			Assert.AreEqual ("https://get.url", element.HttpsGetUrl.OriginalString, "HttpsHelpPageUrl");
			Assert.AreEqual ("http://external.metadata.location", element.ExternalMetadataLocation.OriginalString, "ExternalMetadataLocation");
			Assert.AreEqual (PolicyVersion.Policy12, element.PolicyVersion, "PolicyVersion");
		}

		[Test]
		public void ServiceMetadataPublishingElement_defaults () {
			ServiceMetadataPublishingElement element = new ServiceMetadataPublishingElement ();

			Assert.AreEqual (typeof (ServiceMetadataBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceMetadata", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (false, element.HttpGetEnabled, "HttpGetEnabled");
			Assert.AreEqual (null, element.HttpGetUrl, "HttpGetUrl");
			Assert.AreEqual (false, element.HttpsGetEnabled, "HttpsGetEnabled");
			Assert.AreEqual (null, element.HttpsGetUrl, "HttpsGetUrl");
			Assert.AreEqual (null, element.ExternalMetadataLocation, "ExternalMetadataLocation");
			Assert.AreEqual (PolicyVersion.Default, element.PolicyVersion, "PolicyVersion");
		}

		[Test]
		public void ServiceSecurityAuditElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceSecurityAuditElement element = (ServiceSecurityAuditElement) behavior [typeof (ServiceSecurityAuditElement)];

			if (element == null)
				Assert.Fail ("ServiceSecurityAuditElement is not exist in collection.");

			Assert.AreEqual (typeof (ServiceSecurityAuditBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceSecurityAudit", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (AuditLogLocation.Application, element.AuditLogLocation, "AuditLogLocation");
			Assert.AreEqual (false, element.SuppressAuditFailure, "SuppressAuditFailure");
			Assert.AreEqual (AuditLevel.Success, element.ServiceAuthorizationAuditLevel, "ServiceAuthorizationAuditLevel");
			Assert.AreEqual (AuditLevel.Success, element.MessageAuthenticationAuditLevel, "MessageAuthenticationAuditLevel");
		}

		[Test]
		public void ServiceSecurityAuditElement_defaults () {
			ServiceSecurityAuditElement element = new ServiceSecurityAuditElement ();

			Assert.AreEqual (typeof (ServiceSecurityAuditBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceSecurityAudit", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (AuditLogLocation.Default, element.AuditLogLocation, "AuditLogLocation");
			Assert.AreEqual (true, element.SuppressAuditFailure, "SuppressAuditFailure");
			Assert.AreEqual (AuditLevel.None, element.ServiceAuthorizationAuditLevel, "ServiceAuthorizationAuditLevel");
			Assert.AreEqual (AuditLevel.None, element.MessageAuthenticationAuditLevel, "MessageAuthenticationAuditLevel");
		}

		[Test]
		public void ServiceThrottlingElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceThrottlingElement element = (ServiceThrottlingElement) behavior [typeof (ServiceThrottlingElement)];

			if (element == null)
				Assert.Fail ("ServiceThrottlingElement is not exist in collection.");

			Assert.AreEqual (typeof (ServiceThrottlingBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceThrottling", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (32, element.MaxConcurrentCalls, "MaxConcurrentCalls");
			Assert.AreEqual (20, element.MaxConcurrentSessions, "MaxConcurrentSessions");
			Assert.AreEqual (14, element.MaxConcurrentInstances, "MaxConcurrentInstances");
		}

		[Test]
		public void ServiceThrottlingElement_defaults () {
			ServiceThrottlingElement element = new ServiceThrottlingElement ();

			Assert.AreEqual (typeof (ServiceThrottlingBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceThrottling", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (16, element.MaxConcurrentCalls, "MaxConcurrentCalls");
			Assert.AreEqual (10, element.MaxConcurrentSessions, "MaxConcurrentSessions");
			Assert.AreEqual (26, element.MaxConcurrentInstances, "MaxConcurrentInstances");
		}

		[Test]
		public void ServiceTimeoutsElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceTimeoutsElement element = (ServiceTimeoutsElement) behavior [typeof (ServiceTimeoutsElement)];

			if (element == null)
				Assert.Fail ("ServiceTimeoutsElement is not exist in collection.");

			Assert.AreEqual ("System.ServiceModel.Description.ServiceTimeoutsBehavior", element.BehaviorType.FullName, "BehaviorType");
			Assert.AreEqual ("serviceTimeouts", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (new TimeSpan (0, 3, 0), element.TransactionTimeout, "TransactionTimeout");
		}

		[Test]
		public void ServiceTimeoutsElement_defaults () {
			ServiceTimeoutsElement element = new ServiceTimeoutsElement ();

			Assert.AreEqual ("System.ServiceModel.Description.ServiceTimeoutsBehavior", element.BehaviorType.FullName, "BehaviorType");
			Assert.AreEqual ("serviceTimeouts", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (new TimeSpan (0, 0, 0), element.TransactionTimeout, "TransactionTimeout");
		}

		[Test]
		public void ServiceCredentialsElement () {
			ServiceBehaviorElement behavior = OpenConfig ();
			ServiceCredentialsElement element = (ServiceCredentialsElement) behavior [typeof (ServiceCredentialsElement)];

			if (element == null)
				Assert.Fail ("ServiceCredentialsElement is not exist in collection.");

			Assert.AreEqual (typeof (ServiceCredentials), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceCredentials", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual ("ServiceCredentialsType", element.Type, "Type");

			Assert.AreEqual ("FindValue", element.ClientCertificate.Certificate.FindValue, "ClientCertificate.Certificate.FindValue");
			Assert.AreEqual (StoreLocation.CurrentUser, element.ClientCertificate.Certificate.StoreLocation, "ClientCertificate.Certificate.StoreLocation");
			Assert.AreEqual (StoreName.Root, element.ClientCertificate.Certificate.StoreName, "ClientCertificate.Certificate.StoreName");
			Assert.AreEqual (X509FindType.FindByIssuerName, element.ClientCertificate.Certificate.X509FindType, "ClientCertificate.Certificate.X509FindType");

			Assert.AreEqual ("CustomCertificateValidationType", element.ClientCertificate.Authentication.CustomCertificateValidatorType, "ClientCertificate.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.PeerOrChainTrust, element.ClientCertificate.Authentication.CertificateValidationMode, "ClientCertificate.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Offline, element.ClientCertificate.Authentication.RevocationMode, "ClientCertificate.Authentication.RevocationMode");
			Assert.AreEqual (StoreLocation.CurrentUser, element.ClientCertificate.Authentication.TrustedStoreLocation, "ClientCertificate.Authentication.TrustedStoreLocation");
			Assert.AreEqual (false, element.ClientCertificate.Authentication.IncludeWindowsGroups, "ClientCertificate.Authentication.IncludeWindowsGroups");
			Assert.AreEqual (true, element.ClientCertificate.Authentication.MapClientCertificateToWindowsAccount, "ClientCertificate.Authentication.MapClientCertificateToWindowsAccount");

			Assert.AreEqual ("FindValue", element.ServiceCertificate.FindValue, "ServiceCertificate.FindValue");
			Assert.AreEqual (StoreLocation.CurrentUser, element.ServiceCertificate.StoreLocation, "ServiceCertificate.StoreLocation");
			Assert.AreEqual (StoreName.Root, element.ServiceCertificate.StoreName, "ServiceCertificate.StoreName");
			Assert.AreEqual (X509FindType.FindByIssuerName, element.ServiceCertificate.X509FindType, "ServiceCertificate.X509FindType");

			Assert.AreEqual (UserNamePasswordValidationMode.MembershipProvider, element.UserNameAuthentication.UserNamePasswordValidationMode, "UserNameAuthentication.UserNamePasswordValidationMode");
			Assert.AreEqual (false, element.UserNameAuthentication.IncludeWindowsGroups, "UserNameAuthentication.IncludeWindowsGroups");
			Assert.AreEqual ("MembershipProviderName", element.UserNameAuthentication.MembershipProviderName, "UserNameAuthentication.MembershipProviderName");
			Assert.AreEqual ("CustomUserNamePasswordValidatorType", element.UserNameAuthentication.CustomUserNamePasswordValidatorType, "UserNameAuthentication.customUserNamePasswordValidatorType");
			Assert.AreEqual (true, element.UserNameAuthentication.CacheLogonTokens, "UserNameAuthentication.CacheLogonTokens");
			Assert.AreEqual (252, element.UserNameAuthentication.MaxCachedLogonTokens, "UserNameAuthentication.MaxCachedLogonTokens");
			Assert.AreEqual (new TimeSpan (0, 30, 0), element.UserNameAuthentication.CachedLogonTokenLifetime, "UserNameAuthentication.CachedLogonTokenLifetime");

			Assert.AreEqual ("FindValue", element.Peer.Certificate.FindValue, "Peer.Certificate.FindValue");
			Assert.AreEqual (StoreLocation.LocalMachine, element.Peer.Certificate.StoreLocation, "Peer.Certificate.StoreLocation");
			Assert.AreEqual (StoreName.Root, element.Peer.Certificate.StoreName, "Peer.Certificate.StoreName");
			Assert.AreEqual (X509FindType.FindByIssuerName, element.Peer.Certificate.X509FindType, "Peer.Certificate.X509FindType");

			Assert.AreEqual ("CustomCertificateValidatorType", element.Peer.PeerAuthentication.CustomCertificateValidatorType, "Peer.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.Custom, element.Peer.PeerAuthentication.CertificateValidationMode, "Peer.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Offline, element.Peer.PeerAuthentication.RevocationMode, "Peer.Authentication.RevocationMode");
			Assert.AreEqual (StoreLocation.LocalMachine, element.Peer.PeerAuthentication.TrustedStoreLocation, "Peer.Authentication.TrustedStoreLocation");

			Assert.AreEqual ("CustomCertificateValidatorType", element.Peer.MessageSenderAuthentication.CustomCertificateValidatorType, "Peer.MessageSenderAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.None, element.Peer.MessageSenderAuthentication.CertificateValidationMode, "Peer.MessageSenderAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Offline, element.Peer.MessageSenderAuthentication.RevocationMode, "Peer.MessageSenderAuthentication.RevocationMode");
			Assert.AreEqual (StoreLocation.LocalMachine, element.Peer.MessageSenderAuthentication.TrustedStoreLocation, "Peer.MessageSenderAuthentication.TrustedStoreLocation");

			Assert.AreEqual ("CustomCertificateValidatorType", element.IssuedTokenAuthentication.CustomCertificateValidatorType, "IssuedTokenAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.PeerOrChainTrust, element.IssuedTokenAuthentication.CertificateValidationMode, "IssuedTokenAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Offline, element.IssuedTokenAuthentication.RevocationMode, "IssuedTokenAuthentication.RevocationMode");
			Assert.AreEqual (StoreLocation.CurrentUser, element.IssuedTokenAuthentication.TrustedStoreLocation, "IssuedTokenAuthentication.TrustedStoreLocation");
			Assert.AreEqual ("SalmSerializerType", element.IssuedTokenAuthentication.SamlSerializerType, "IssuedTokenAuthentication.SamlSerializerType");
			Assert.AreEqual (true, element.IssuedTokenAuthentication.AllowUntrustedRsaIssuers, "IssuedTokenAuthentication.AllowUntrustedRsaIssuers");

			Assert.AreEqual ("FindValue", element.IssuedTokenAuthentication.KnownCertificates [0].FindValue, "IssuedTokenAuthentication.KnownCertificates[0].FindValue");
			Assert.AreEqual (StoreLocation.CurrentUser, element.IssuedTokenAuthentication.KnownCertificates [0].StoreLocation, "IssuedTokenAuthentication.KnownCertificates[0].StoreLocation");
			Assert.AreEqual (StoreName.Root, element.IssuedTokenAuthentication.KnownCertificates [0].StoreName, "IssuedTokenAuthentication.KnownCertificates[0].StoreName");
			Assert.AreEqual (X509FindType.FindByIssuerName, element.IssuedTokenAuthentication.KnownCertificates [0].X509FindType, "IssuedTokenAuthentication.KnownCertificates[0].X509FindType");

			Assert.AreEqual ("SecurityStateEncoderType", element.SecureConversationAuthentication.SecurityStateEncoderType, "SecureConversationAuthentication.SecurityStateEncoderType");
		}

		[Test]
		public void ServiceCredentialsElement_defaults () {
			ServiceCredentialsElement element = new ServiceCredentialsElement ();

			Assert.AreEqual (typeof (ServiceCredentials), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("serviceCredentials", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (String.Empty, element.Type, "Type");

			Assert.AreEqual (String.Empty, element.ClientCertificate.Certificate.FindValue, "ClientCertificate.Certificate.FindValue");
			Assert.AreEqual (StoreLocation.LocalMachine, element.ClientCertificate.Certificate.StoreLocation, "ClientCertificate.Certificate.StoreLocation");
			Assert.AreEqual (StoreName.My, element.ClientCertificate.Certificate.StoreName, "ClientCertificate.Certificate.StoreName");
			Assert.AreEqual (X509FindType.FindBySubjectDistinguishedName, element.ClientCertificate.Certificate.X509FindType, "ClientCertificate.Certificate.X509FindType");

			Assert.AreEqual (String.Empty, element.ClientCertificate.Authentication.CustomCertificateValidatorType, "ClientCertificate.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.ChainTrust, element.ClientCertificate.Authentication.CertificateValidationMode, "ClientCertificate.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Online, element.ClientCertificate.Authentication.RevocationMode, "ClientCertificate.Authentication.RevocationMode");
			Assert.AreEqual (StoreLocation.LocalMachine, element.ClientCertificate.Authentication.TrustedStoreLocation, "ClientCertificate.Authentication.TrustedStoreLocation");
			Assert.AreEqual (true, element.ClientCertificate.Authentication.IncludeWindowsGroups, "ClientCertificate.Authentication.IncludeWindowsGroups");
			Assert.AreEqual (false, element.ClientCertificate.Authentication.MapClientCertificateToWindowsAccount, "ClientCertificate.Authentication.MapClientCertificateToWindowsAccount");

			Assert.AreEqual (String.Empty, element.ServiceCertificate.FindValue, "ServiceCertificate.FindValue");
			Assert.AreEqual (StoreLocation.LocalMachine, element.ServiceCertificate.StoreLocation, "ServiceCertificate.StoreLocation");
			Assert.AreEqual (StoreName.My, element.ServiceCertificate.StoreName, "ServiceCertificate.StoreName");
			Assert.AreEqual (X509FindType.FindBySubjectDistinguishedName, element.ServiceCertificate.X509FindType, "ServiceCertificate.X509FindType");

			Assert.AreEqual (UserNamePasswordValidationMode.Windows, element.UserNameAuthentication.UserNamePasswordValidationMode, "UserNameAuthentication.UserNamePasswordValidationMode");
			Assert.AreEqual (true, element.UserNameAuthentication.IncludeWindowsGroups, "UserNameAuthentication.IncludeWindowsGroups");
			Assert.AreEqual (String.Empty, element.UserNameAuthentication.MembershipProviderName, "UserNameAuthentication.MembershipProviderName");
			Assert.AreEqual (String.Empty, element.UserNameAuthentication.CustomUserNamePasswordValidatorType, "UserNameAuthentication.customUserNamePasswordValidatorType");
			Assert.AreEqual (false, element.UserNameAuthentication.CacheLogonTokens, "UserNameAuthentication.CacheLogonTokens");
			Assert.AreEqual (128, element.UserNameAuthentication.MaxCachedLogonTokens, "UserNameAuthentication.MaxCachedLogonTokens");
			Assert.AreEqual (new TimeSpan (0, 15, 0), element.UserNameAuthentication.CachedLogonTokenLifetime, "UserNameAuthentication.CachedLogonTokenLifetime");

			Assert.AreEqual (String.Empty, element.Peer.Certificate.FindValue, "Peer.Certificate.FindValue");
			Assert.AreEqual (StoreLocation.CurrentUser, element.Peer.Certificate.StoreLocation, "Peer.Certificate.StoreLocation");
			Assert.AreEqual (StoreName.My, element.Peer.Certificate.StoreName, "Peer.Certificate.StoreName");
			Assert.AreEqual (X509FindType.FindBySubjectDistinguishedName, element.Peer.Certificate.X509FindType, "Peer.Certificate.X509FindType");

			Assert.AreEqual (String.Empty, element.Peer.PeerAuthentication.CustomCertificateValidatorType, "Peer.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.PeerOrChainTrust, element.Peer.PeerAuthentication.CertificateValidationMode, "Peer.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Online, element.Peer.PeerAuthentication.RevocationMode, "Peer.Authentication.RevocationMode");
			Assert.AreEqual (StoreLocation.CurrentUser, element.Peer.PeerAuthentication.TrustedStoreLocation, "Peer.Authentication.TrustedStoreLocation");

			Assert.AreEqual (String.Empty, element.Peer.MessageSenderAuthentication.CustomCertificateValidatorType, "Peer.MessageSenderAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.PeerOrChainTrust, element.Peer.MessageSenderAuthentication.CertificateValidationMode, "Peer.MessageSenderAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Online, element.Peer.MessageSenderAuthentication.RevocationMode, "Peer.MessageSenderAuthentication.RevocationMode");
			Assert.AreEqual (StoreLocation.CurrentUser, element.Peer.MessageSenderAuthentication.TrustedStoreLocation, "Peer.MessageSenderAuthentication.TrustedStoreLocation");

			Assert.AreEqual (String.Empty, element.IssuedTokenAuthentication.CustomCertificateValidatorType, "IssuedTokenAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.ChainTrust, element.IssuedTokenAuthentication.CertificateValidationMode, "IssuedTokenAuthentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509RevocationMode.Online, element.IssuedTokenAuthentication.RevocationMode, "IssuedTokenAuthentication.RevocationMode");
			Assert.AreEqual (StoreLocation.LocalMachine, element.IssuedTokenAuthentication.TrustedStoreLocation, "IssuedTokenAuthentication.TrustedStoreLocation");
			Assert.AreEqual (String.Empty, element.IssuedTokenAuthentication.SamlSerializerType, "IssuedTokenAuthentication.SamlSerializerType");
			Assert.AreEqual (false, element.IssuedTokenAuthentication.AllowUntrustedRsaIssuers, "IssuedTokenAuthentication.AllowUntrustedRsaIssuers");

			Assert.AreEqual (0, element.IssuedTokenAuthentication.KnownCertificates.Count, "IssuedTokenAuthentication.KnownCertificates.Count");

			Assert.AreEqual (String.Empty, element.SecureConversationAuthentication.SecurityStateEncoderType, "SecureConversationAuthentication.SecurityStateEncoderType");
		}
	}
}
#endif
