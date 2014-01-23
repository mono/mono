//
// WebReferenceTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

#if !MOBILE

using NUnit.Framework;

using System;
using System.CodeDom;
using System.IO;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class WebReferenceTest
	{
		[Test]
		public void ProtocolName ()
		{
			WebReference r = new WebReference (
				new DiscoveryClientDocumentCollection (),
				new CodeNamespace (),
				null, null, null); // null ProtocolName
			r = new WebReference (
				new DiscoveryClientDocumentCollection (),
				new CodeNamespace (),
				null, null);
			Assert.AreEqual (String.Empty, r.ProtocolName, "#1");
			// it is not rejected here, while only "SOAP" and
			// "SOAP12" are said as valid...
			r.ProtocolName = "invalid";
		}
	}
}

#endif
