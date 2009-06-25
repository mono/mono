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
	public class ObjectIDGeneratorTests
	{
		ObjectIDGenerator generator;

		string obj1 = "obj1";
		int obj2 = 42;		
		long id;

		[SetUp]
		protected void SetUp ()
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

			Assert.AreEqual (1L, id); // should start at 1, "A1");
			Assert.AreEqual (true, testBool1);	// firstTime should be true, "A2");
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

			Assert.AreEqual (testId1, id); // same object, same ID, "B1");
			Assert.AreEqual (false, testBool2); // no longer firstTime, "B2");
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

			Assert.AreEqual (false, testBool3); // this has been inserted before, "C1");
			Assert.AreEqual (id, testId2); // we should get the same ID, "C2");
		}

		//
		// Tests getting the ID for a non-existent object
		//
		public void TestHasId2 ()
		{
			bool testBool4;
			long testId3 = generator.HasId (obj2, out testBool4);

			Assert.AreEqual (0L, testId3, "D1");
			Assert.AreEqual (true, testBool4, "D2");
		}
	}
}
