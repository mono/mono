//
// System.IO.StringWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Add some testing for exceptions
//
// TODO: Add some testing for the CanXXX properties, exceptions,
// various different constructors.
//

using NUnit.Framework;
using System.IO;
using System;
using System.Text;

namespace MonoTests.System.IO
{

public class MemoryStreamTest : TestCase {
	
        private MemoryStream testStream;
        private byte[] testStreamData;
        
	public MemoryStreamTest( string name ): base(name) { }

	public static ITest Suite {
		get {
			return new TestSuite(typeof(MemoryStreamTest));
		}
	}

        protected override void SetUp() {
                testStreamData = new byte[100];

                for( int i = 0; i < 100; i++ ) {
                        testStreamData[i] = (byte)(100 - i);
                }

                testStream = new MemoryStream( testStreamData );
        }

	public void TestConstructors() {
                MemoryStream ms = new MemoryStream();

                AssertEquals( 0, ms.Length );
                AssertEquals( 0, ms.Capacity );
                AssertEquals( true, ms.CanWrite );
                
                ms = new MemoryStream( 10 );

                // FIXME: Should ms.Length be 0 if there is no data?
                // the spec is a little unclear.  If this is changed then
                // the code will probably need to change

                AssertEquals( 10, ms.Length );
                AssertEquals( 10, ms.Capacity );
        }

        // 
        // Verify that the first count bytes in testBytes are the same as
        // the count bytes from index start in testStreamData
        //
        private void VerifyTestData( byte[] testBytes, int start, int count) {
                if( testBytes == null ) {
                        throw new ArgumentNullException();
                } else if( ( start < 0 || count < 0 ) || start + count > testStreamData.Length || start > testStreamData.Length ) {
                        throw new ArgumentOutOfRangeException();
                }

                for( int test = 0; test < count; test++ ) {
                        if( testBytes[ test ] != testStreamData[ start + test ] ) {
                                string failStr = String.Format( "testByte {0} (testStream {1}) was <{2}>, expecting <{3}>", test, start+test, 
                                        testBytes[ test ], testStreamData[ start+test] );
                                Fail( failStr );
                        }
                }
        }

        public void TestRead() {
                byte[] readBytes = new byte[20];

                /* Test simple read */
                testStream.Read( readBytes, 0, 10 );
                VerifyTestData( readBytes, 0, 10 );

                /* Seek back to beginning */

                testStream.Seek( 0, SeekOrigin.Begin );
 
                /* Read again, bit more this time */
                testStream.Read( readBytes, 0, 20 );
                VerifyTestData( readBytes, 0, 20 );

                /* Seek to 20 bytes from End */
                testStream.Seek( -20, SeekOrigin.End );
                testStream.Read( readBytes, 0, 20);
                VerifyTestData( readBytes, 80, 20);

                int readByte = testStream.ReadByte();
                AssertEquals( -1, readByte );
                                      
        }

        public void TestWriteBytes() {
                byte[] readBytes = new byte[100];
                MemoryStream ms = new MemoryStream( 100 );

                for( int i = 0; i < 100; i++ ) {
                        ms.WriteByte( testStreamData[i] );
                }

                ms.Seek( 0, SeekOrigin.Begin); 
                
                testStream.Read( readBytes, 0, 100 );

                VerifyTestData( readBytes, 0, 100 );
        }               

        public void TestWriteBlock() {
                byte[] readBytes = new byte[100];
                MemoryStream ms = new MemoryStream( 100 );

                ms.Write( testStreamData, 0, 100 );

                ms.Seek( 0, SeekOrigin.Begin); 
                
                testStream.Read( readBytes, 0, 100 );

                VerifyTestData( readBytes, 0, 100 );

                byte[] arrayBytes = testStream.ToArray();

                VerifyTestData( arrayBytes, 0, 100 );

        }
}

}
