//
// System.Runtime.Serialization.ObjectIDGeneratorTests.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
	public class ObjectIDGeneratorTests : TestCase
	{
		ObjectIDGenerator generator;

		string obj1 = "obj1";
		int obj2 = 42;		
		long id;

		protected override void SetUp ()
		{
			generator = new ObjectIDGenerator ();
		}

		//
		// Tests adding an ID for a new object
		//
		public void TestGetId1 ()
		{
			bool testBool1;
			id = generator.GetId (obj1, out testBool1);

			AssertEquals ("A1", 1L, id); // should start at 1
			AssertEquals ("A2", true, testBool1);	// firstTime should be true
		}

		//
		// Tests getting the ID for an existing object
		//
		public void TestGetId2 ()
		{
			bool testBool1;
			bool testBool2;
			id = generator.GetId (obj1, out testBool1);
			long testId1 = generator.GetId (obj1, out testBool2);

			AssertEquals ("B1", testId1, id); // same object, same ID
			AssertEquals ("B2", false, testBool2); // no longer firstTime
		}

		//
		// Tests getting the ID for an existing object
		//
		public void TestHasId1 ()
		{
			bool testBool1;
			bool testBool3;
			id = generator.GetId (obj1, out testBool1);
			long testId2 = generator.HasId (obj1, out testBool3);

			AssertEquals ("C1", false, testBool3); // this has been inserted before
			AssertEquals ("C2", id, testId2); // we should get the same ID
		}

		//
		// Tests getting the ID for a non-existent object
		//
		public void TestHasId2 ()
		{
			bool testBool4;
			long testId3 = generator.HasId (obj2, out testBool4);

			AssertEquals ("D1", 0L, testId3);
			AssertEquals ("D2", true, testBool4);
		}
	}
}
