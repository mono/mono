//
// System.Data.ObjectSpaces.Query.ObjectExpressionTests.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Data.ObjectSpaces;
using System.Data.ObjectSpaces.Query;
using System.Data.ObjectSpaces.Schema;
using NUnit.Framework;

namespace MonoTests.System.Data.ObjectSpaces.Query
{
	[TestFixture]
	public class ObjectExpressionTests : Assertion
	{
		ObjectExpression objectExpression;
		Expression expression;
		ObjectSchema objectSchema;

		[SetUp]
		public void GetReady ()
		{
			expression = new Parameter (1);
			objectSchema = new ObjectSchema ();
			objectExpression = new ObjectExpression (expression, objectSchema);
		}

		[Test]
		public void Expression
		{
			AssertEquals ("#A01", expression, ObjectExpression.Expression);
		}

		[Test]
		public void Expression ()
		{
			AssertEquals ("#A01", objectSchema, ObjectExpression.ObjectSchema);
		}
        }
}

#endif
