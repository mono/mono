//
// WebReferenceOptionsTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

#if NET_2_0

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
		[Test]
		[Category ("NotDotNet")] // why on earth does it allow invalid xml?
		public void Schema ()
		{
			string xml1 = "<webReferenceOptions xmlns='http://microsoft.com/webReferences/' />";
			string xml2 = @"
<webReferenceOptions xmlns='http://microsoft.com/webReferences/'>
  <codeGenerationOptions>properties newAsync</codeGenerationOptions>
  <style>client</style>
  <verbose>false</verbose>
</webReferenceOptions>
			";
			string xml3 = @"
<webReferenceOptions xmlns='http://microsoft.com/webReferences/'>
  <gyabo/>
  <hoge/>
</webReferenceOptions>";

WebReferenceOptions.Schema.Write (Console.Out);

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
	}
}

#endif
