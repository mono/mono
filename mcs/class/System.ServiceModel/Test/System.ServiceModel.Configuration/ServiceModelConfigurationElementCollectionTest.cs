//
// ServiceModelConfigurationElementCollectionTest.cs
//
// Author:
//	Eyal Alaluf <eyala@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Configuration;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class ServiceModelConfigurationElementCollectionTest
	{
		ClaimTypeElementCollection collection;

		private ClaimTypeElement CreateClaimType (string claim, bool isOptional)
		{
			ClaimTypeElement elem = new ClaimTypeElement ();
			elem.ClaimType = claim;
			elem.IsOptional = isOptional;
			return elem;
		}

		[TearDown]
		public void TearDownCollection ()
		{
			collection = null;
		}

		[SetUp]
		public void SetupCollection ()
		{
			collection = new ClaimTypeElementCollection ();
			collection.Add (CreateClaimType ("test1", false));
			collection.Add (CreateClaimType ("test2", false));
		}

		[Test]
		public void Indexer ()
		{
			Assert.AreEqual ("test1", collection ["test1"].ClaimType, "#01");
			collection ["test3"] = CreateClaimType ("test3", false);
			Assert.AreEqual ("test3", collection ["test3"].ClaimType, "#02");
			collection ["test2"] = CreateClaimType ("test2", true);
			Assert.AreEqual (true, collection ["test2"].IsOptional, "#03");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IndexerSetWithIncorrectKey ()
		{
			collection ["test10"] = CreateClaimType ("test", false);
		}

		[Test]
		public void IndexOf ()
		{
			Assert.AreEqual (1, collection.IndexOf (CreateClaimType ("test2", false)), "#01");
			Assert.AreEqual (-1, collection.IndexOf (CreateClaimType ("test10", false)), "#02");
		}

		[Test]
		public void Remove ()
		{
			ClaimTypeElement elem = collection ["test2"];
			collection.Remove (elem);
			Assert.AreEqual (-1, collection.IndexOf (elem), "#01");
			collection.Add (elem);
			Assert.AreEqual (1, collection.IndexOf (elem), "#02");
			collection.RemoveAt (1);
			Assert.AreEqual (-1, collection.IndexOf (elem), "#03");
			collection.Add (elem);
			Assert.AreEqual (1, collection.IndexOf (elem), "#04");
			collection.RemoveAt ("test2");
			Assert.AreEqual (-1, collection.IndexOf (elem), "#05");
		}

		[Test]
		public void Clear ()
		{
			ClaimTypeElement elem = collection ["test2"];
			collection.Clear ();
			Assert.AreEqual (-1, collection.IndexOf (elem), "#01");
			Assert.AreEqual (0, collection.Count, "#02");
		}

		[Test]
		public void ContainsKey ()
		{
			Assert.AreEqual (true, collection.ContainsKey ("test1"), "#01");
			Assert.AreEqual (false, collection.ContainsKey ("test10"), "#02");
		}

		[Test]
		public void CopyTo ()
		{
			ClaimTypeElement[] array = new ClaimTypeElement [4];
			collection.CopyTo (array, 2);
			Assert.AreEqual ("test1", array [2].ClaimType, "#01");
			Assert.AreEqual ("test2", array [3].ClaimType, "#02");
		}
	}
}
#endif

