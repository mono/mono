//
// CardSelectorClientWin32.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors.Win32
{
	public class CardSelectorClientWin32 : CardSelectorClient
	{
		public override void Manage ()
		{
			ManageCardSpace ();
		}

		public override void Import (string fileName)
		{
			ImportInformationCard (fileName);
		}

		public override GenericXmlSecurityToken GetToken (
			CardSpacePolicyElement [] policyChain,
			SecurityTokenSerializer serializer)
		{
			NativeGenericXmlToken token;
			NativeInfocardCryptoHandle proof;
			NativePolicyElement [] natives =
				new NativePolicyElement [policyChain.Length];
			for (int i = 0; i < policyChain.Length; i++)
				natives [i] = new NativePolicyElement (
					policyChain [i].Target,
					policyChain [i].Issuer,
					policyChain [i].Parameters,
					policyChain [i].PolicyNoticeLink,
					policyChain [i].PolicyNoticeVersion,
					policyChain [i].IsManagedIssuer);

			int hresult = GetToken (policyChain.Length, natives, out token, out proof);
			NativeGetTokenResults ret = (NativeGetTokenResults) (hresult & 0xCFFFFFFF);
			switch (ret) {
			case NativeGetTokenResults.OK:
				return token.ToObject (proof, serializer);
			case NativeGetTokenResults.UserCancelled:
				throw new UserCancellationException ();
			case NativeGetTokenResults.InvalidPolicy:
				throw new PolicyValidationException ();
			case NativeGetTokenResults.ServiceBusy:
				throw new ServiceBusyException ();
			case NativeGetTokenResults.ServiceUnavailable:
				throw new ServiceNotStartedException ();
			case NativeGetTokenResults.IdentityVerificationFailed:
			case NativeGetTokenResults.InvalidDecryptionKey:
				throw new IdentityValidationException ();
			case NativeGetTokenResults.ErrorOnCommunication:
				throw new StsCommunicationException ();
			case NativeGetTokenResults.UntrustedRecipient:
				throw new UntrustedRecipientException ();
			case NativeGetTokenResults.UnsupportedPolicy:
				throw new UnsupportedPolicyOptionsException ();
			case NativeGetTokenResults.ErrorOnDataAccess:
			case NativeGetTokenResults.ErrorOnExport:
			case NativeGetTokenResults.ErrorOnImport:
			case NativeGetTokenResults.InvalidArgument:
			case NativeGetTokenResults.ErrorInRequest:
			case NativeGetTokenResults.ErrorInCardData:
			case NativeGetTokenResults.InvalidCertificateLogo:
			case NativeGetTokenResults.InvalidPassword:
			case NativeGetTokenResults.ProcessDied:
			case NativeGetTokenResults.Shuttingdown:
			case NativeGetTokenResults.ErrorOnTokenCreation:
			case NativeGetTokenResults.TrustExchangeFailure:
			case NativeGetTokenResults.ErrorOnStoreImport:
			case NativeGetTokenResults.UIStartFailure:
			case NativeGetTokenResults.MaxSession:
			case NativeGetTokenResults.ImportFileAccessFailure:
			case NativeGetTokenResults.MalformedRequest:
			case NativeGetTokenResults.RefreshRequired:
			case NativeGetTokenResults.MissingAppliesTo:
			case NativeGetTokenResults.UnknownReference:
			case NativeGetTokenResults.InvalidProofKey:
			case NativeGetTokenResults.ClaimsNotProvided:
			default:
				throw CardspaceError (ret);
			}
		}

		static Exception CardspaceError (NativeGetTokenResults error)
		{
			switch (error) {
			default:
				throw new CardSpaceException (String.Format ("identity selector returned an error: {0:X}", error));
			}
		}

		[DllImport ("infocardapi", CharSet = CharSet.Unicode)]
		static extern int GetToken (int cPolicyChain,
			NativePolicyElement [] pPolicyChain,
			out NativeGenericXmlToken securityToken,
			out NativeInfocardCryptoHandle phProofTokenCrypto);

		[DllImport ("infocardapi")]
		static extern void ManageCardSpace ();

		[DllImport ("infocardapi", CharSet = CharSet.Unicode)]
		static extern void ImportInformationCard (string fileName);

		enum NativeGetTokenResults : long
		{
			OK = 0,
			ErrorOnCommunication		= 0xC0050100,
			ErrorOnDataAccess		= 0xC0050101,
			ErrorOnExport			= 0xC0050102,
			IdentityVerificationFailed	= 0xC0050103,
			ErrorOnImport			= 0xC0050104,
			InvalidArgument			= 0xC0050105,
			ErrorInRequest			= 0xC0050106,
			ErrorInCardData			= 0xC0050107,
			InvalidDecryptionKey		= 0xC0050108,
			InvalidCertificateLogo		= 0xC0050109,
			InvalidPassword			= 0xC005010A,
			InvalidPolicy			= 0xC005010B,
			ProcessDied			= 0xC005010C,
			ServiceBusy			= 0xC005010D,
			ServiceUnavailable		= 0xC005010E,
			Shuttingdown			= 0xC005010F,
			ErrorOnTokenCreation		= 0xC0050110,
			TrustExchangeFailure		= 0xC0050111,
			UntrustedRecipient		= 0xC0050112,
			UserCancelled			= 0xC0050113,
			ErrorOnStoreImport		= 0xC0050114,
			UIStartFailure			= 0xC0050115,
			UnsupportedPolicy		= 0xC0050116,
			MaxSession			= 0xC0050117,
			ImportFileAccessFailure		= 0xC0050118,
			MalformedRequest		= 0xC0050119,
			RefreshRequired			= 0xC0050180,
			MissingAppliesTo		= 0xC0050181,
			InvalidProofKey			= 0xC0050182,
			UnknownReference		= 0xC0050183,
			ClaimsNotProvided		= 0xC0050184,
		}
	}
}
