/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using NUnit.Framework;
namespace Lucene.Net.Util
{
	[TestFixture]
	public class StringHelperTest
	{
		[TestFixtureSetUp]
		protected virtual void  SetUp()
		{
		}
		
        [TestFixtureTearDown]
		protected virtual void  TearDown()
		{
			
		}
		
        [Test]
		public virtual void  TestStringDifference()
		{
			System.String test1 = "test";
			System.String test2 = "testing";
			
			int result = StringHelper.StringDifference(test1, test2);
			Assert.IsTrue(result == 4);
			
			test2 = "foo";
			result = StringHelper.StringDifference(test1, test2);
			Assert.IsTrue(result == 0);
			
			test2 = "test";
			result = StringHelper.StringDifference(test1, test2);
			Assert.IsTrue(result == 4);
		}
	}
}