//
// ServiceDescriptionTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.
//

using NUnit.Framework;

using System;
using System.IO;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionTest
	{
		[Test]
		public void SimpleWrite ()
		{
			ServiceDescription sd = new ServiceDescription ();
			Assert.IsNull (sd.Name);
			sd.Write (TextWriter.Null);
		}
	}
}
