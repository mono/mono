//
// SecureConversationSecurityTokenParametersTest.cs
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
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

using Parameters = System.ServiceModel.Security.Tokens.SecureConversationSecurityTokenParameters;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class SecureConversationSecurityTokenParametersTest
	{
		class MyParameters : Parameters
		{
			public MyParameters ()
			{
			}

			public MyParameters (SecurityBindingElement element)
				: base (element)
			{
			}

			public MyParameters (SecurityBindingElement element,
				bool cancel,
				ChannelProtectionRequirements requirements)
				: base (element, cancel, requirements)
			{
			}

			public bool Asym {
				get { return HasAsymmetricKey; }
			}

			public bool Client {
				get { return SupportsClientAuthentication; }
			}

			public bool Win {
				get { return SupportsClientWindowsIdentity; }
			}

			public bool Server {
				get { return SupportsServerAuthentication; }
			}

			public void InitRequirement (SecurityTokenRequirement r)
			{
				InitializeSecurityTokenRequirement (r);
			}
		}

		[Test]
		public void DefaultValues ()
		{
			MyParameters p = new MyParameters ();
			Assert.IsNull (p.BootstrapSecurityBindingElement, "#1-1");
			Assert.IsNotNull (p.BootstrapProtectionRequirements, "#1-2");
			Assert.AreEqual (true, p.RequireCancellation, "#1-3");

			Assert.AreEqual (false, p.Asym, "#1-4");
			// they cause NRE on winfx, likely a bug.
			//Assert.AreEqual (true, p.Client, "#1-5");
			//Assert.AreEqual (true, p.Win, "#1-6");
			//Assert.AreEqual (true, p.Server, "#1-7");

			p = new MyParameters (new AsymmetricSecurityBindingElement ());
			Assert.IsNotNull (p.BootstrapSecurityBindingElement, "#2-1");
			Assert.IsNotNull (p.BootstrapProtectionRequirements, "#2-2");
			Assert.AreEqual (true, p.RequireCancellation, "#2-3");

			Assert.AreEqual (false, p.Asym, "#2-4"); // regardless of binding element.
			Assert.AreEqual (false, p.Client, "#2-5");
			Assert.AreEqual (false, p.Win, "#2-6");
			Assert.AreEqual (false, p.Server, "#2-7");

			ChannelProtectionRequirements r =
				p.BootstrapProtectionRequirements;
			Assert.IsTrue (r.IncomingSignatureParts.ChannelParts.IsBodyIncluded, "#3-1");
			Assert.IsTrue (r.OutgoingSignatureParts.ChannelParts.IsBodyIncluded, "#3-2");
			Assert.IsTrue (r.IncomingEncryptionParts.ChannelParts.IsBodyIncluded, "#3-3");
			Assert.IsTrue (r.OutgoingEncryptionParts.ChannelParts.IsBodyIncluded, "#3-4");
		}

		[Test]
		public void NullArgs ()
		{
			new Parameters ((SecurityBindingElement) null);
			new Parameters (null, false, null);
		}

		[Test]
		[Ignore ("winfx bug")]
		public void InitializeSecurityTokenRequirement ()
		{
			ServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			SymmetricSecurityBindingElement sbe = 
//				new SymmetricSecurityBindingElement ();
				new WSHttpBinding ().CreateBindingElements ().Find<SymmetricSecurityBindingElement> ();
			sbe.ProtectionTokenParameters = new X509SecurityTokenParameters ();
//			MyParameters p = new MyParameters (sbe);
			// NRE occurs on winfx (likely a bug).
//			p.InitRequirement (r);
		}
	}
}
#endif

