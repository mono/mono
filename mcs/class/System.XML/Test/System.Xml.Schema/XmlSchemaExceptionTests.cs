// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
//
// Unit tests for XmlSchemaException
//
// Author:
//     Eberhard Beilharz <eb1@sil.org>
//
using System;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaExceptionTests
	{
		[Test]
		public void Bug599689_ToStringMatchesDotNet ()
		{
			Assert.AreEqual ("System.Xml.Schema.XmlSchemaException: Test",
				new XmlSchemaException ("Test").ToString (),
				"Novell bug #599689 (https://bugzilla.novell.com/show_bug.cgi?id=599689) not fixed.");
		}
	}
}

