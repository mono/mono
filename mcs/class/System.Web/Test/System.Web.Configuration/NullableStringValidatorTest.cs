//
// NullableStringValidatorTest.cs 
//	- unit tests from the aspect of NullableStringValidator usage.
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class NullableStringValidatorTest  {

		[Test]
		// It test all existing (as of r61933) configuration
		// sections that use PropertyHelper.NonEmptyStringValidator.
		public void NullableStringProperties ()
		{
			new AnonymousIdentificationSection ().CookieName = null;
			new AnonymousIdentificationSection ().CookiePath = null;
			new AssemblyInfo (null);
			new BufferModeSettings (null, 0x10000, 0x1000, 10,
			TimeSpan.FromMinutes (1),
			TimeSpan.FromSeconds (30), 10);
			new BuildProvider (null, null);
			new ClientTarget (null, null);
			new CodeSubDirectory (null);
			new EventMappingSettings (null, null);
			new ExpressionBuilder (null, null);
			FormsAuthenticationConfiguration fac =
			new FormsAuthenticationConfiguration ();
			// I don't like this test though.
			fac.DefaultUrl = null;
			fac.LoginUrl = null;
			fac.Name = null;
			fac.Path = null;
			new HttpHandlerAction (null, null, null);
			new HttpModuleAction (null, null);
			MachineKeySection mks = new MachineKeySection ();
			// algorithms are limited
			// mks.Decryption = null;
			mks.DecryptionKey = null;
			mks.ValidationKey = null;
			new MembershipSection ().DefaultProvider = null;
			new NamespaceInfo (null);
			new OutputCacheProfile (null);
			new ProfileSettings (null);
			RoleManagerSection rms = new RoleManagerSection ();
			rms.CookieName = null;
			rms.CookiePath = null;
			rms.DefaultProvider = null;
			new RuleSettings (null, null, null);
			new SqlCacheDependencyDatabase (null, null);
			new TagMapInfo (null, null);
			new TagPrefixInfo (null, null, null, null, null);
			new TransformerInfo (null, null);
			new TrustLevel (null, null);
			new TrustSection ().Level = null;
			new UrlMapping (null, null);
			// WebControlsSection.ClientScriptsLocation is not settable
			new WebPartsPersonalization ().DefaultProvider = null;
		}
	}

}

#endif
