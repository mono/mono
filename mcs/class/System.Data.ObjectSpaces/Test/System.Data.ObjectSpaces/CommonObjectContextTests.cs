//
// System.Data.ObjectSpaces.CommonObjectContextTests.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.ObjectSpaces;
using System.Data.ObjectSpaces.Schema;
using NUnit.Framework;

namespace MonoTests.System.Data.ObjectSpaces
{
	[TestFixture]
	public class CommonObjectContextTests : Assertion
	{
		CommonObjectContext context;

		[SetUp]
		public void GetReady ()
		{
			ObjectSchema schema = new ObjectSchema ();
			context = new CommonObjectContext (schema);
		}

		[Test]
		public void Add1 ()
		{
			try {
				context.Add (null);
				Fail ("Expected a ContextException to be thrown.");
			} catch (ContextException) {}

			try {
				context.Add ("add1");
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}
		}

		[Test]
		public void Add2 ()
		{
			try {
				context.Add (null, ObjectState.Unknown);
				Fail ("Expected a ContextException to be thrown.");
			} catch (ContextException) {}

			try {
				context.Add ("add2", (ObjectState) (-1));
				Fail ("Expected a NullReferenceException to be thrown.");
			} catch (NullReferenceException) {}

			try {
				context.Add ("add2", ObjectState.Unknown);
				Fail ("Expected a ContextException to be thrown.");
			} catch (ContextException) {}

			context.Add ("add2", ObjectState.Inserted);
		}

		[Test]
		public void Delete ()
		{
			try {
				context.Delete (null);
				Fail ("Expected an ArgumentNullException to be thrown.");
			} catch (ArgumentNullException) {}
		}
        }
}

#endif
