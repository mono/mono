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

public class StringBuilderTest : TestCase {

        public StringBuilderTest( string name ) : base(name) { }

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
                
                AssertEquals( "1234F!!oo..", sb.ToString() );
                
                sb.Insert( 5, 1.5 ); /* Test insert of a decimal (and end of string) */
                
                AssertEquals( "1234F1.5!!oo..", sb.ToString() );

                sb.Insert( 4, 'A' ); /* Test char insert in middle of string */

                AssertEquals( "1234AF1.5!!oo..", sb.ToString() );

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