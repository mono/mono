//
// ServiceModelSecurityTokenRequirement.cs
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
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Security.Tokens
{
	public abstract class ServiceModelSecurityTokenRequirement :
		SecurityTokenRequirement
	{
		protected ServiceModelSecurityTokenRequirement ()
		{
		}

		protected const string Namespace = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement";

		public static string AuditLogLocationProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/AuditLogLocation"; }
		}

		public static string ChannelParametersCollectionProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ChannelParametersCollection"; }
		}

		public static string DuplexClientLocalAddressProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/DuplexClientLocalAddress"; }
		}

		public static string EndpointFilterTableProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/EndpointFilterTable"; }
		}

		public static string ExtendedProtectionPolicy {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ExtendedProtectionPolicy"; }
		}

		public static string HttpAuthenticationSchemeProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/HttpAuthenticationScheme"; }
		}

		public static string IsInitiatorProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IsInitiator"; }
		}

		public static string IsOutOfBandTokenProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IsOutOfBandToken"; }
		}

		public static string IssuedSecurityTokenParametersProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuedSecurityTokenParameters"; }
		}

		public static string IssuerAddressProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerAddress"; }
		}

		public static string IssuerBindingContextProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerBindingContext"; }
		}

		public static string IssuerBindingProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/IssuerBinding"; }
		}

		public static string ListenUriProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/ListenUri"; }
		}

		public static string MessageAuthenticationAuditLevelProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageAuthenticationAuditLevel"; }
		}

		public static string MessageDirectionProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageDirection"; }
		}

		public static string MessageSecurityVersionProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/MessageSecurityVersion"; }
		}

		public static string PrivacyNoticeUriProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/PrivacyNoticeUri"; }
		}

		public static string PrivacyNoticeVersionProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/PrivacyNoticeVersion"; }
		}

		public static string SecureConversationSecurityBindingElementProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecureConversationSecurityBindingElement"; }
		}

		public static string SecurityAlgorithmSuiteProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecurityAlgorithmSuite"; }
		}

		public static string SecurityBindingElementProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SecurityBindingElement"; }
		}

		public static string SupportingTokenAttachmentModeProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SupportingTokenAttachmentMode"; }
		}

		public static string SupportSecurityContextCancellationProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SupportSecurityContextCancellation"; }
		}

		public static string SuppressAuditFailureProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/SuppressAuditFailure"; }
		}

		public static string TargetAddressProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TargetAddress"; }
		}

		public static string TransportSchemeProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/TransportScheme"; }
		}

		public static string ViaProperty {
			get { return "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement/Via"; }
		}

		public bool IsInitiator {
			get {
				bool ret;
				if (TryGetProperty<bool> (IsInitiatorProperty, out ret))
					return ret;
				return false;
			}
		}

		public EndpointAddress IssuerAddress {
			get {
				EndpointAddress ret;
				TryGetProperty<EndpointAddress> (IssuerAddressProperty, out ret);
				return ret;
			}
			set { Properties [IssuerAddressProperty] = value; }
		}

		public Binding IssuerBinding {
			get {
				Binding ret;
				TryGetProperty<Binding> (IssuerBindingProperty, out ret);
				return ret;
			}
			set { Properties [IssuerBindingProperty] = value; }
		}

		public SecurityTokenVersion MessageSecurityVersion {
			get {
				SecurityTokenVersion ret;
				TryGetProperty<SecurityTokenVersion> (MessageSecurityVersionProperty, out ret);
				return ret;
			}
			set { Properties [MessageSecurityVersionProperty] = value; }
		}

		public SecurityBindingElement SecureConversationSecurityBindingElement {
			get {
				SecurityBindingElement ret;
				TryGetProperty<SecurityBindingElement> (SecureConversationSecurityBindingElementProperty, out ret);
				return ret;
			}
			set { Properties [SecureConversationSecurityBindingElementProperty] = value; }
		}

		public SecurityAlgorithmSuite SecurityAlgorithmSuite {
			get {
				SecurityAlgorithmSuite ret;
				TryGetProperty<SecurityAlgorithmSuite> (SecurityAlgorithmSuiteProperty, out ret);
				return ret;
			}
			set { Properties [SecurityAlgorithmSuiteProperty] = value; }
		}

		public SecurityBindingElement SecurityBindingElement {
			get {
				SecurityBindingElement ret;
				TryGetProperty<SecurityBindingElement> (SecurityBindingElementProperty, out ret);
				return ret;
			}
			set { Properties [SecurityBindingElementProperty] = value; }
		}

		public string TransportScheme {
			get {
				string ret;
				TryGetProperty<string> (TransportSchemeProperty, out ret);
				return ret;
			}
			set { Properties [TransportSchemeProperty] = value; }
		}

		internal string Dump ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (GetType ()).Append (":");
			foreach (KeyValuePair<string, object> p in Properties)
				sb.Append ("\n------------\n")
				  .Append ("URI: ")
				  .Append (p.Key)
				  .Append ("\nValue: ")
				  .Append (p.Value);
			return sb.ToString ();
		}
	}
}
