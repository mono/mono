//
// WebReferenceOptionsTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

#if !MOBILE && !XAMMAC_4_5

using NUnit.Framework;

using System;
using System.IO;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class WebReferenceOptionsTest
	{
		string xml1 = "<webReferenceOptions xmlns='http://microsoft.com/webReference/' />";
		string xml2 = @"
<webReferenceOptions xmlns='http://microsoft.com/webReference/'>
  <codeGenerationOptions>properties newAsync</codeGenerationOptions>
  <style>client</style>
  <verbose>false</verbose>
</webReferenceOptions>
		";
		string xml3 = @"
<webReferenceOptions xmlns='http://microsoft.com/webReference/'>
  <gyabo/>
  <hoge/>
</webReferenceOptions>";

		[Test]
		[Category ("NotDotNet")] // why on earth does it allow invalid xml?
		public void Schema ()
		{
			Validate (xml1);
			Validate (xml2);
			try {
				Validate (xml3);
				Assert.Fail ("xml3 is invalid.");
			} catch (XmlSchemaValidationException) {
			}
		}

		void Validate (string xml)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ValidationType = ValidationType.Schema;
			s.Schemas.Add (WebReferenceOptions.Schema);
			XmlReader r = XmlReader.Create (new StringReader (xml), s);
			while (!r.EOF)
				r.Read ();
		}

		[Test]
		public void Read ()
		{
			Read (xml1);
			Read (xml2);
			try {
				Read (xml3);
				Assert.Fail ("xml3 is invalid.");
			} catch (InvalidOperationException) {
			}
		}

		void Read (string xml)
		{
			XmlReader r = XmlReader.Create (new StringReader (xml));
			WebReferenceOptions.Read (r, null);
		}
	}
}

#endif
