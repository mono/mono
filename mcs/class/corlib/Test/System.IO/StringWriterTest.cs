//
// System.IO.StringWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Add some testing for exceptions
//

using NUnit.Framework;
using System.IO;
using System;
using System.Text;

namespace MonoTests.System.IO {

public class StringWriterTest : TestCase {
	public void TestConstructors() {
                StringBuilder sb = new StringBuilder("Test String");

                StringWriter writer = new StringWriter( sb );
                AssertEquals( sb, writer.GetStringBuilder() );
        }

        public void TestWrite() {
                StringWriter writer = new StringWriter();

                AssertEquals( String.Empty, writer.ToString() );
                
                writer.Write( 'A' );
                AssertEquals( "A", writer.ToString() );

                writer.Write( " foo" );
                AssertEquals( "A foo", writer.ToString() );

                
                char[] testBuffer = "Test String".ToCharArray();

                writer.Write( testBuffer, 0, 4 );
                AssertEquals( "A fooTest", writer.ToString() );

                writer.Write( testBuffer, 5, 6 );
                AssertEquals( "A fooTestString", writer.ToString() );
        }
}

}
