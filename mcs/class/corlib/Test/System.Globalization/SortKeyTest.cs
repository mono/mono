//
// SortKeyTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Globalization
{

	[TestFixture]
	public class SortKeyTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CompareNull ()
		{
			// bug #376171
			SortKey.Compare (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CompareNull2 ()
		{
			// bug #376171
			SortKey.Compare (CultureInfo.InvariantCulture.CompareInfo.GetSortKey ("A"), null);
		}
	}

}
