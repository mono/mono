//
// ReceivedCollectionTest.cs - NUnit Test Cases for ReceivedCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Timestamp;
using System;
using System.Xml;

// note: due to compiler confusion between classes and namespace (like Timestamp)
// I renamed the test namespace from "MonoTests.Microsoft.Web.Services.Timestamp"
// to "MonoTests.MS.Web.Services.Timestamp".
namespace MonoTests.MS.Web.Services.Timestamp {

	[TestFixture]
	public class ReceivedCollectionTest : Assertion {

		private ReceivedCollection coll;

		[SetUp]
		void SetUp ()
		{
			coll = new ReceivedCollection ();
		}

		[Test]
		public void Empty () 
		{
			AssertEquals ("Empty: Count = 0", 0, coll.Count);
			AssertNotNull ("Enumerator", coll.GetEnumerator ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void EmptyAccess () 
		{
			Received r = coll[0];
		}

		[Test]
		public void Add () 
		{
			Received r = new Received (new Uri ("http://www.go-mono.com/"));
			coll.Add (r);
			AssertEquals ("Add: Count = 1", 1, coll.Count);
			Assert ("Contains 1", coll.Contains (r));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNull () 
		{
			Received r = null;
			coll.Add (r);
		}

		[Test]
		public void CopyTo () 
		{
			Received r = new Received (new Uri ("http://www.go-mono.com/"));
			coll.Add (r);

			object[] container = new object [3];
			coll.CopyTo (container, 0);
			AssertNotNull ("CopyTo[0]", container[0]);
			AssertNull ("CopyTo[1]", container[1]);
			AssertNull ("CopyTo[2]", container[2]);

			coll.Remove (r);
			AssertEquals ("Remove: Count = 0", 0, coll.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyToException () 
		{
			coll.CopyTo (null, 0);
		}
	}
}