//
// MessageSecurityVersion.cs
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
using System.Collections.ObjectModel;
#if !MOBILE && !XAMMAC_4_5
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#endif
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel
{
	public abstract class MessageSecurityVersion
	{
#if !MOBILE && !XAMMAC_4_5
		// Types
		class MessageSecurityTokenVersion : SecurityTokenVersion
		{
			static string [] specs10_profile_source, specs11_source, specs11_profile_source;
			static readonly MessageSecurityTokenVersion wss10basic, wss11, wss11basic;


			static MessageSecurityTokenVersion ()
			{
				specs10_profile_source = new string [] {
					Constants.WssNamespace,
					Constants.WstNamespace,
					Constants.WsscNamespace,
					Constants.WSBasicSecurityProfileCore1,
					};
				specs11_source = new string [] {
					Constants.Wss11Namespace,
					Constants.WstNamespace,
					Constants.WsscNamespace,
					};
				specs11_profile_source = new string [] {
					Constants.Wss11Namespace,
					Constants.WstNamespace,
					Constants.WsscNamespace,
					Constants.WSBasicSecurityProfileCore1,
					};

				wss10basic = new MessageSecurityTokenVersion (false, true);
				wss11basic = new MessageSecurityTokenVersion (true, true);
				wss11 = new MessageSecurityTokenVersion (true, false);
			}

			public static MessageSecurityTokenVersion GetVersion (bool isWss11, bool basicProfile)
			{
				if (isWss11)
					return basicProfile ? wss11basic : wss11;
				else
					return wss10basic;
			}

			ReadOnlyCollection<string> specs;

			MessageSecurityTokenVersion (bool wss11, bool basicProfile)
			{
				string [] src;
				if (wss11)
					src = basicProfile ? specs11_profile_source : specs11_source;
				else
					src = basicProfile ? specs10_profile_source : null;
				specs = new ReadOnlyCollection<string> (src);
			}

			public override ReadOnlyCollection<string> GetSecuritySpecifications ()
			{
				return specs;
			}
		}

		class MessageSecurityVersionImpl : MessageSecurityVersion
		{
			bool wss11, basic_profile, use2007;

			public MessageSecurityVersionImpl (bool wss11, bool basicProfile, bool use2007)
			{
				this.wss11 = wss11;
				this.basic_profile = basicProfile;
				this.use2007 = use2007;
				if (use2007) {
					SecureConversationVersion = SecureConversationVersion.Default;
					TrustVersion = TrustVersion.Default;
				} else {
					SecureConversationVersion = SecureConversationVersion.WSSecureConversationFeb2005;
					TrustVersion = TrustVersion.WSTrustFeb2005;
				}
				this.SecurityVersion = wss11 ? SecurityVersion.WSSecurity11 : SecurityVersion.WSSecurity10;
			}

			public override BasicSecurityProfileVersion BasicSecurityProfileVersion {
				get { return basic_profile ? BasicSecurityProfileVersion.BasicSecurityProfile10 : null; }
			}

			public override SecurityTokenVersion SecurityTokenVersion {
				get { return MessageSecurityTokenVersion.GetVersion (wss11, basic_profile); }
			}

			public override SecurityPolicyVersion SecurityPolicyVersion {
				get { return use2007 ? SecurityPolicyVersion.WSSecurityPolicy12 : SecurityPolicyVersion.WSSecurityPolicy11; }
			}
		}
#endif

		// Static members

		static MessageSecurityVersion wss10_basic, wss11, wss11_basic, wss10_2007_basic, wss11_2007_basic, wss11_2007;

		static MessageSecurityVersion ()
		{
#if !MOBILE && !XAMMAC_4_5
			wss10_basic = new MessageSecurityVersionImpl (false, true, false);
			wss11 = new MessageSecurityVersionImpl (true, false, false);
			wss11_basic = new MessageSecurityVersionImpl (true, true, false);
			wss10_2007_basic = new MessageSecurityVersionImpl (false, true, true);
			wss11_2007_basic = new MessageSecurityVersionImpl (true, true, true);
			wss11_2007 = new MessageSecurityVersionImpl (true, false, true);
#else
			throw new NotImplementedException ();
#endif
		}

		public static MessageSecurityVersion Default {
			get { return wss11; }
		}

		// guys, have you ever seen such silly member names??

		public static MessageSecurityVersion WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10 {
			get { return wss10_basic; }
		}

		public static MessageSecurityVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11 {
			get { return wss11; }
		}

		public static MessageSecurityVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10 {
			get { return wss11_basic; }
		}

		public static MessageSecurityVersion WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10 {
			get { return wss10_2007_basic; }
		}

		public static MessageSecurityVersion WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10 {
			get { return wss11_2007_basic; }
		}

		public static MessageSecurityVersion WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12 {
			get { return wss11_2007; }
		}

		// Instance members

		MessageSecurityVersion ()
		{
		}

		public abstract BasicSecurityProfileVersion BasicSecurityProfileVersion { get; }

#if !MOBILE && !XAMMAC_4_5
		public abstract SecurityTokenVersion SecurityTokenVersion { get; }
#endif

		public SecurityVersion SecurityVersion { get; internal set; }

		public SecureConversationVersion SecureConversationVersion { get; internal set; }

		public abstract SecurityPolicyVersion SecurityPolicyVersion { get; }

		public TrustVersion TrustVersion { get; internal set; }

	}
}
