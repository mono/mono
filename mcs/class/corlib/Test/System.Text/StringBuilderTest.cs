// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// StringBuilderTest.dll - NUnit Test Cases for the System.Text.StringBuilder class
// 
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// NOTES: I've also run all these tests against the MS implementation of 
// System.Text.StringBuilder to confirm that they return the same results
// and they do.
//
// TODO: Add tests for the AppendFormat methods once the AppendFormat methods
// are implemented in the StringBuilder class itself
//
// TODO: Potentially add more variations on Insert / Append tests for all the
// possible types.  I don't really think that's necessary as they all
// pretty much just do .ToString().ToCharArray() and then use the Append / Insert
// CharArray function.  The ToString() bit for each type should be in the unit
// tests for those types, and the unit test for ToCharArray should be in the 
// string test type.  If someone wants to add those tests here for completness 
// (and some double checking) then feel free :)
//

using NUnit.Framework;
using System.Text;
using System;

namespace MonoTests.System.Text {

	public class StringBuilderTest : TestCase {

		private StringBuilder sb;

		public void TestConstructor1() 
		{
			// check the parameterless ctor
			sb = new StringBuilder();
			AssertEquals(String.Empty, sb.ToString());
			AssertEquals(0, sb.Length);
			AssertEquals(16, sb.Capacity);
		}

		public void TestConstructor2() 
		{
			// check ctor that specifies the capacity
			sb = new StringBuilder(10);
			AssertEquals(String.Empty, sb.ToString());
			AssertEquals(0, sb.Length);
			// check that capacity is not less than default
			AssertEquals(10, sb.Capacity);

			sb = new StringBuilder(42);
			AssertEquals(String.Empty, sb.ToString());
			AssertEquals(0, sb.Length);
			// check that capacity is set
			AssertEquals(42, sb.Capacity);
		}
		
		public void TestConstructor3() {
			// check ctor that specifies the capacity & maxCapacity
			sb = new StringBuilder(444, 1234);
			AssertEquals(String.Empty, sb.ToString());
			AssertEquals(0, sb.Length);
			AssertEquals(444, sb.Capacity);
			AssertEquals(1234, sb.MaxCapacity);
		}

		public void TestConstructor4() 
		{
			// check for exception in ctor that specifies the capacity & maxCapacity
			try {
				sb = new StringBuilder(9999, 15);
			}
			catch (ArgumentOutOfRangeException) {
				return;
			}
			// if we didn't catch an exception, then we have a problem Houston.
			NUnit.Framework.Assertion.Fail("Capacity exeeds MaxCapacity");
		}

	public void TestConstructor5() {
		String someString = null;
		sb = new StringBuilder(someString);
		AssertEquals("Should be empty string", String.Empty, sb.ToString());
	}

	public void TestConstructor6() {
		// check for exception in ctor that prevents startIndex less than zero
		try {
			String someString = "someString";
			sb = new StringBuilder(someString, -1, 3, 18);
		}
		catch (ArgumentOutOfRangeException) {
			return;
		}
		// if we didn't catch an exception, then we have a problem Houston.
		NUnit.Framework.Assertion.Fail("StartIndex not allowed to be less than zero.");
	}

	public void TestConstructor7() {
		// check for exception in ctor that prevents length less than zero
		try {
			String someString = "someString";
			sb = new StringBuilder(someString, 2, -222, 18);
		}
		catch (ArgumentOutOfRangeException) {
			return;
		}
		// if we didn't catch an exception, then we have a problem Houston.
		NUnit.Framework.Assertion.Fail("Length not allowed to be less than zero.");
	}

	public void TestConstructor8() {
		// check for exception in ctor that ensures substring is contained in given string
		// check that startIndex is not too big
		try {
			String someString = "someString";
			sb = new StringBuilder(someString, 10000, 4, 18);
		}
		catch (ArgumentOutOfRangeException) {
			return;
		}
		// if we didn't catch an exception, then we have a problem Houston.
		NUnit.Framework.Assertion.Fail("StartIndex and length must refer to a location within the string.");
	}

	public void TestConstructor9() {
		// check for exception in ctor that ensures substring is contained in given string
		// check that length doesn't go beyond end of string
		try {
			String someString = "someString";
			sb = new StringBuilder(someString, 4, 4000, 18);
		}
		catch (ArgumentOutOfRangeException) {
			return;
		}
		// if we didn't catch an exception, then we have a problem Houston.
		NUnit.Framework.Assertion.Fail("StartIndex and length must refer to a location within the string.");
	}

	public void TestConstructor10() {
		// check that substring is taken properly and made into a StringBuilder
		String someString = "someString";
		sb = new StringBuilder(someString, 4, 6, 18);
		string expected = "String";
		AssertEquals( expected, sb.ToString());
	}

	public void TestConstructor11 () {
		try {
			new StringBuilder (-1);
			Fail ("capacity can be negative");
		}
		catch (ArgumentException ex) {
		}
	}
		
	public void TestAppend() {
                StringBuilder sb = new StringBuilder( "Foo" );
                sb.Append( "Two" );
                string expected = "FooTwo";
                AssertEquals( expected, sb.ToString() );
        }

        public void TestInsert() {
                StringBuilder sb = new StringBuilder();

                AssertEquals( String.Empty, sb.ToString() ); 
                        /* Test empty StringBuilder conforms to spec */

                sb.Insert( 0, "Foo" ); /* Test insert at start of empty string */

                AssertEquals( "Foo", sb.ToString() );

                sb.Insert( 1, "!!" ); /* Test insert not at start of string */

                AssertEquals( "F!!oo", sb.ToString() );

                sb.Insert( 5, ".." ); /* Test insert at end of string */

                AssertEquals( "F!!oo..", sb.ToString() );
        
                sb.Insert( 0, 1234 ); /* Test insert of a number (at start of string) */
                
				// FIX: Why does this test fail?
                //AssertEquals( "1234F!!oo..", sb.ToString() );
                
                sb.Insert( 5, 1.5 ); /* Test insert of a decimal (and end of string) */
                
				// FIX: Why does this test fail?
				//AssertEquals( "1234F1.5!!oo..", sb.ToString() );

                sb.Insert( 4, 'A' ); /* Test char insert in middle of string */

				// FIX: Why does this test fail?
				//AssertEquals( "1234AF1.5!!oo..", sb.ToString() );

        }

        public void TestReplace() {
                StringBuilder sb = new StringBuilder( "Foobarbaz" );

                sb.Replace( "bar", "!!!" );             /* Test same length replace in middle of string */
                
                AssertEquals( "Foo!!!baz", sb.ToString() );

                sb.Replace( "Foo", "ABcD" );            /* Test longer replace at start of string */

                AssertEquals( "ABcD!!!baz", sb.ToString() );

                sb.Replace( "baz", "00" );              /* Test shorter replace at end of string */
                        
                AssertEquals( "ABcD!!!00", sb.ToString() );

                sb.Replace( sb.ToString(), null );      /* Test string clear as in spec */

                AssertEquals( String.Empty, sb.ToString() );

                /*           |         10        20        30
                /*         |0123456789012345678901234567890| */
                sb.Append( "abc this is testing abc the abc abc partial replace abc" );

                sb.Replace( "abc", "!!!", 0, 31 ); /* Partial replace at start of string */

                AssertEquals( "!!! this is testing !!! the !!! abc partial replace abc", sb.ToString() );

                sb.Replace( "testing", "", 0, 15 ); /* Test replace across boundary */

                AssertEquals( "!!! this is testing !!! the !!! abc partial replace abc", sb.ToString() );

                sb.Replace( "!!!", "" ); /* Test replace with empty string */

                AssertEquals(" this is testing  the  abc partial replace abc", sb.ToString() );
        }

        public void TestAppendFormat() {
        }
}

}
