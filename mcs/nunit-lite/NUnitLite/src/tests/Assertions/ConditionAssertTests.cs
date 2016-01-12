// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
// ***********************************************************************

using System;
using System.Collections;
using System.Threading;
using System.Globalization;
using NUnit.Framework;

namespace NUnit.Framework.Assertions
{
	[TestFixture]
	public class ConditionAssertTests : MessageChecker
	{
		[Test]
		public void IsTrue()
		{
			Assert.IsTrue(true);
		}

		[Test,ExpectedException(typeof(AssertionException))]
		public void IsTrueFails()
		{
			expectedMessage =
				"  Expected: True" + Env.NewLine +
				"  But was:  False" + Env.NewLine;
			Assert.IsTrue(false);
		}

		[Test]
		public void IsFalse()
		{
			Assert.IsFalse(false);
		}

		[Test]
		[ExpectedException(typeof(AssertionException))]
		public void IsFalseFails()
		{
			expectedMessage =
				"  Expected: False" + Env.NewLine +
				"  But was:  True" + Env.NewLine;
			Assert.IsFalse(true);
		}
	
		[Test]
		public void IsNull()
		{
			Assert.IsNull(null);
		}

		[Test]
		[ExpectedException(typeof(AssertionException))]
		public void IsNullFails()
		{
			String s1 = "S1";
			expectedMessage =
				"  Expected: null" + Env.NewLine +
				"  But was:  \"S1\"" + Env.NewLine;
			Assert.IsNull(s1);
		}
	
		[Test]
		public void IsNotNull()
		{
			String s1 = "S1";
			Assert.IsNotNull(s1);
		}

		[Test]
		[ExpectedException(typeof(AssertionException))]
		public void IsNotNullFails()
		{
			expectedMessage =
				"  Expected: not null" + Env.NewLine +
				"  But was:  null" + Env.NewLine;
			Assert.IsNotNull(null);
		}
	
#if !NUNITLITE
		[Test]
		public void IsNaN()
		{
			Assert.IsNaN(double.NaN);
		}

		[Test]
		[ExpectedException(typeof(AssertionException))]
		public void IsNaNFails()
		{
			expectedMessage =
				"  Expected: NaN" + Env.NewLine +
				"  But was:  10.0d" + Env.NewLine;
			Assert.IsNaN(10.0);
		}

		[Test]
		public void IsEmpty()
		{
			Assert.IsEmpty( "", "Failed on empty String" );
			Assert.IsEmpty( new int[0], "Failed on empty Array" );
			Assert.IsEmpty( new ArrayList(), "Failed on empty ArrayList" );
			Assert.IsEmpty( new Hashtable(), "Failed on empty Hashtable" );
			Assert.IsEmpty( (IEnumerable)new int[0], "Failed on empty IEnumerable" );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsEmptyFailsOnString()
		{
			expectedMessage =
				"  Expected: <empty>" + Env.NewLine +
				"  But was:  \"Hi!\"" + Env.NewLine;
			Assert.IsEmpty( "Hi!" );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsEmptyFailsOnNullString()
		{
			expectedMessage =
				"  Expected: <empty>" + Env.NewLine +
				"  But was:  null" + Env.NewLine;
			Assert.IsEmpty( (string)null );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsEmptyFailsOnNonEmptyArray()
		{
			expectedMessage =
				"  Expected: <empty>" + Env.NewLine +
				"  But was:  < 1, 2, 3 >" + Env.NewLine;
			Assert.IsEmpty( new int[] { 1, 2, 3 } );
		}

        [Test, ExpectedException(typeof(AssertionException))]
        public void IsEmptyFailsOnNonEmptyIEnumerable()
        {
            expectedMessage =
                "  Expected: <empty>" + Environment.NewLine +
                "  But was:  < 1, 2, 3 >" + Environment.NewLine;
            Assert.IsEmpty((IEnumerable)new int[] { 1, 2, 3 });
        }
 
		[Test]
		public void IsNotEmpty()
		{
			int[] array = new int[] { 1, 2, 3 };
			ArrayList list = new ArrayList( array );
			Hashtable hash = new Hashtable();
			hash.Add( "array", array );

			Assert.IsNotEmpty( "Hi!", "Failed on String" );
			Assert.IsNotEmpty( array, "Failed on Array" );
			Assert.IsNotEmpty( list, "Failed on ArrayList" );
			Assert.IsNotEmpty( hash, "Failed on Hashtable" );
			Assert.IsNotEmpty( (IEnumerable)array, "Failed on IEnumerable" );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsNotEmptyFailsOnEmptyString()
		{
			expectedMessage =
				"  Expected: not <empty>" + Env.NewLine +
				"  But was:  <string.Empty>" + Env.NewLine;
			Assert.IsNotEmpty( "" );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsNotEmptyFailsOnEmptyArray()
		{
			expectedMessage =
				"  Expected: not <empty>" + Env.NewLine +
				"  But was:  <empty>" + Env.NewLine;
			Assert.IsNotEmpty( new int[0] );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsNotEmptyFailsOnEmptyArrayList()
		{
			expectedMessage =
				"  Expected: not <empty>" + Env.NewLine +
				"  But was:  <empty>" + Env.NewLine;
			Assert.IsNotEmpty( new ArrayList() );
		}

		[Test, ExpectedException(typeof(AssertionException))]
		public void IsNotEmptyFailsOnEmptyHashTable()
		{
			expectedMessage =
				"  Expected: not <empty>" + Env.NewLine +
				"  But was:  <empty>" + Env.NewLine;
			Assert.IsNotEmpty( new Hashtable() );
		}

        [Test, ExpectedException(typeof(AssertionException))]
        public void IsNotEmptyFailsOnEmptyIEnumerable()
        {
            expectedMessage =
                "  Expected: not <empty>" + Environment.NewLine +
                "  But was:  <empty>" + Environment.NewLine;
            Assert.IsNotEmpty((IEnumerable)new int[0]);
        }
#endif
	}
}
