//
// EndpointBehaviorElementTest.cs
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
using System.Configuration;
using System.ServiceModel.Description;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Security.Principal;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class EndpointBehaviorElementTest
	{
		EndpointBehaviorElement OpenConfig () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/endpointBehaviors")).GetSectionGroup ("system.serviceModel");
			return config.Behaviors.EndpointBehaviors [0];
		}

		[Test]
		public void CallbackDebugElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
			CallbackDebugElement callbackDebug = (CallbackDebugElement) behavior [typeof (CallbackDebugElement)];

			if (callbackDebug == null)
				Assert.Fail ("CallbackDebugElement is not exist in collection.");

			Assert.AreEqual (typeof (CallbackDebugBehavior), callbackDebug.BehaviorType, "RoleProviderName");
			Assert.AreEqual ("callbackDebug", callbackDebug.ConfigurationElementName, "RoleProviderName");
			Assert.AreEqual (true, callbackDebug.IncludeExceptionDetailInFaults, "IncludeExceptionDetailInFaults");
		}

		[Test]
		public void CallbackDebugElement_defaults () {
			CallbackDebugElement element = new CallbackDebugElement ();

			Assert.AreEqual (typeof (CallbackDebugBehavior), element.BehaviorType, "element");
			Assert.AreEqual ("callbackDebug", element.ConfigurationElementName, "ConfigurationElementName");
			Assert.AreEqual (false, element.IncludeExceptionDetailInFaults, "IncludeExceptionDetailInFaults");
		}

		[Test]
		public void CallbackTimeoutsElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
			CallbackTimeoutsElement element = (CallbackTimeoutsElement) behavior [typeof (CallbackTimeoutsElement)];

			if (element == null)
				Assert.Fail ("CallbackTimeoutsElement is not exist in collection.");

			Assert.AreEqual ("System.ServiceModel.Description.CallbackTimeoutsBehavior", element.BehaviorType.FullName, "BehaviorType");
			Assert.AreEqual ("callbackTimeouts", element.ConfigurationElementName, "ConfigurationElementName");
			Assert.AreEqual (new TimeSpan (0, 2, 30), element.TransactionTimeout, "TransactionTimeout");
		}

		[Test]
		public void CallbackTimeoutsElement_defaults () {
			CallbackTimeoutsElement element = new CallbackTimeoutsElement ();

			Assert.AreEqual ("System.ServiceModel.Description.CallbackTimeoutsBehavior", element.BehaviorType.FullName, "BehaviorType");
			Assert.AreEqual ("callbackTimeouts", element.ConfigurationElementName, "ConfigurationElementName");
			Assert.AreEqual (new TimeSpan (0, 0, 0), element.TransactionTimeout, "TransactionTimeout");
		}

		[Test]
		public void ClientViaElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
			ClientViaElement element = (ClientViaElement) behavior [typeof (ClientViaElement)];

			if (element == null)
				Assert.Fail ("ClientViaElement is not exist in collection.");

			Assert.AreEqual (typeof (ClientViaBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("clientVia", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual ("http://via.uri", element.ViaUri.OriginalString, "ViaUri");
		}

		[Test]
		public void ClientViaElement_defaults () {
			ClientViaElement element = new ClientViaElement ();

			Assert.AreEqual (typeof (ClientViaBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("clientVia", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (null, element.ViaUri, "ViaUri");
		}

		[Test]
		public void DataContractSerializerElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
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
		public void SynchronousReceiveElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
			SynchronousReceiveElement element = (SynchronousReceiveElement) behavior [typeof (SynchronousReceiveElement)];

			if (element == null)
				Assert.Fail ("SynchronousReceiveElement is not exist in collection.");

			Assert.AreEqual (typeof (SynchronousReceiveBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("synchronousReceive", element.ConfigurationElementName, "ConfigurationElementName");
		}

		[Test]
		public void SynchronousReceiveElement_defaults () {
			SynchronousReceiveElement element = new SynchronousReceiveElement ();

			Assert.AreEqual (typeof (SynchronousReceiveBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("synchronousReceive", element.ConfigurationElementName, "ConfigurationElementName");
		}

		[Test]
		public void TransactedBatchingElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
			TransactedBatchingElement element = (TransactedBatchingElement) behavior [typeof (TransactedBatchingElement)];

			if (element == null)
				Assert.Fail ("TransactedBatchingElement is not exist in collection.");

			Assert.AreEqual (typeof (TransactedBatchingBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("transactedBatching", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (16, element.MaxBatchSize, "MaxBatchSize");
		}

		[Test]
		public void TransactedBatchingElement_defaults () {
			TransactedBatchingElement element = new TransactedBatchingElement ();

			Assert.AreEqual (typeof (TransactedBatchingBehavior), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("transactedBatching", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (0, element.MaxBatchSize, "MaxBatchSize");
		}

		[Test]
		public void ClientCredentialsElement () {
			EndpointBehaviorElement behavior = OpenConfig ();
			ClientCredentialsElement element = (ClientCredentialsElement) behavior [typeof (ClientCredentialsElement)];

			if (element == null)
				Assert.Fail ("ClientCredentialsElement is not exist in collection.");

			Assert.AreEqual (typeof (ClientCredentials), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("clientCredentials", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (false, element.SupportInteractive, "SupportInteractive");
			Assert.AreEqual ("ClientCredentialType", element.Type, "Type");

			Assert.AreEqual ("findValue", element.ClientCertificate.FindValue, "ClientCertificate.FindValue");
			Assert.AreEqual (StoreLocation.LocalMachine, element.ClientCertificate.StoreLocation, "ClientCertificate.StoreLocation");
			Assert.AreEqual (StoreName.Root, element.ClientCertificate.StoreName, "ClientCertificate.StoreName");
			Assert.AreEqual (X509FindType.FindByExtension, element.ClientCertificate.X509FindType, "ClientCertificate.X509FindType");

			Assert.AreEqual ("findValue", element.ServiceCertificate.DefaultCertificate.FindValue, "ServiceCertificate.DefaultCertificate.FindValue");
			Assert.AreEqual (StoreLocation.LocalMachine, element.ServiceCertificate.DefaultCertificate.StoreLocation, "ServiceCertificate.DefaultCertificate.StoreLocation");
			Assert.AreEqual (StoreName.Root, element.ServiceCertificate.DefaultCertificate.StoreName, "ServiceCertificate.DefaultCertificate.StoreName");
			Assert.AreEqual (X509FindType.FindByExtension, element.ServiceCertificate.DefaultCertificate.X509FindType, "ServiceCertificate.DefaultCertificate.X509FindType");

			Assert.AreEqual ("CustomCertificateValidatorType", element.ServiceCertificate.Authentication.CustomCertificateValidatorType, "ServiceCertificate.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.None, element.ServiceCertificate.Authentication.CertificateValidationMode, "ServiceCertificate.Authentication.CertificateValidationMode");
			Assert.AreEqual (X509RevocationMode.Offline, element.ServiceCertificate.Authentication.RevocationMode, "ServiceCertificate.Authentication.RevocationMode");
			Assert.AreEqual (StoreLocation.LocalMachine, element.ServiceCertificate.Authentication.TrustedStoreLocation, "ServiceCertificate.Authentication.TrustedStoreLocation");

			Assert.AreEqual (false, element.Windows.AllowNtlm, "Windows.AllowNtlm");
			Assert.AreEqual (TokenImpersonationLevel.None, element.Windows.AllowedImpersonationLevel, "Windows.AllowedImpersonationLevel");

			Assert.AreEqual (false, element.IssuedToken.CacheIssuedTokens, "IssuedToken.CacheIssuedTokens");
			Assert.AreEqual (SecurityKeyEntropyMode.ClientEntropy, element.IssuedToken.DefaultKeyEntropyMode, "IssuedToken.DefaultKeyEntropyMode");
			Assert.AreEqual (30, element.IssuedToken.IssuedTokenRenewalThresholdPercentage, "IssuedToken.IssuedTokenRenewalThresholdPercentage");

			Assert.AreEqual (TokenImpersonationLevel.None, element.HttpDigest.ImpersonationLevel, "HttpDigest.ImpersonationLevel");
		}

		[Test]
		public void ClientCredentialsElement_defaults () {
			ClientCredentialsElement element = new ClientCredentialsElement ();

			Assert.AreEqual (typeof (ClientCredentials), element.BehaviorType, "BehaviorType");
			Assert.AreEqual ("clientCredentials", element.ConfigurationElementName, "ConfigurationElementName");

			Assert.AreEqual (true, element.SupportInteractive, "SupportInteractive");
			Assert.AreEqual (String.Empty, element.Type, "Type");

			Assert.AreEqual (String.Empty, element.ClientCertificate.FindValue, "ClientCertificate.FindValue");
			Assert.AreEqual (StoreLocation.CurrentUser, element.ClientCertificate.StoreLocation, "ClientCertificate.StoreLocation");
			Assert.AreEqual (StoreName.My, element.ClientCertificate.StoreName, "ClientCertificate.StoreName");
			Assert.AreEqual (X509FindType.FindBySubjectDistinguishedName, element.ClientCertificate.X509FindType, "ClientCertificate.X509FindType");

			Assert.AreEqual (String.Empty, element.ServiceCertificate.DefaultCertificate.FindValue, "ServiceCertificate.DefaultCertificate.FindValue");
			Assert.AreEqual (StoreLocation.CurrentUser, element.ServiceCertificate.DefaultCertificate.StoreLocation, "ServiceCertificate.DefaultCertificate.StoreLocation");
			Assert.AreEqual (StoreName.My, element.ServiceCertificate.DefaultCertificate.StoreName, "ServiceCertificate.DefaultCertificate.StoreName");
			Assert.AreEqual (X509FindType.FindBySubjectDistinguishedName, element.ServiceCertificate.DefaultCertificate.X509FindType, "ServiceCertificate.DefaultCertificate.X509FindType");

			Assert.AreEqual (String.Empty, element.ServiceCertificate.Authentication.CustomCertificateValidatorType, "ServiceCertificate.Authentication.CustomCertificateValidatorType");
			Assert.AreEqual (X509CertificateValidationMode.ChainTrust, element.ServiceCertificate.Authentication.CertificateValidationMode, "ServiceCertificate.Authentication.CertificateValidationMode");
			Assert.AreEqual (X509RevocationMode.Online, element.ServiceCertificate.Authentication.RevocationMode, "ServiceCertificate.Authentication.RevocationMode");
			Assert.AreEqual (StoreLocation.CurrentUser, element.ServiceCertificate.Authentication.TrustedStoreLocation, "ServiceCertificate.Authentication.TrustedStoreLocation");

			Assert.AreEqual (true, element.Windows.AllowNtlm, "Windows.AllowNtlm");
			Assert.AreEqual (TokenImpersonationLevel.Identification, element.Windows.AllowedImpersonationLevel, "Windows.AllowedImpersonationLevel");

			Assert.AreEqual (true, element.IssuedToken.CacheIssuedTokens, "IssuedToken.CacheIssuedTokens");
			Assert.AreEqual (SecurityKeyEntropyMode.CombinedEntropy, element.IssuedToken.DefaultKeyEntropyMode, "IssuedToken.DefaultKeyEntropyMode");
			Assert.AreEqual (60, element.IssuedToken.IssuedTokenRenewalThresholdPercentage, "IssuedToken.IssuedTokenRenewalThresholdPercentage");

			Assert.AreEqual (TokenImpersonationLevel.Identification, element.HttpDigest.ImpersonationLevel, "HttpDigest.ImpersonationLevel");
		}
	}
}
#endif
