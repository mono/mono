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
        
	protected override void SetUp() {
                testStreamData = new byte[100];

                for( int i = 0; i < 100; i++ ) {
                        testStreamData[i] = (byte)(100 - i);
                }

                testStream = new MemoryStream( testStreamData );
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

	public void TestConstructors() {
                MemoryStream ms = new MemoryStream();

                AssertEquals("A1", 0L, ms.Length );
                AssertEquals("A2", 0, ms.Capacity );
                AssertEquals("A3", true, ms.CanWrite );
                
                ms = new MemoryStream( 10 );

                AssertEquals("A4", 0L, ms.Length );
                AssertEquals("A5", 10, ms.Capacity );
        }

        public void TestRead() {
                byte[] readBytes = new byte[20];

		try {
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
		catch(Exception e){
			Fail("Threw an unexpected exception:"+e.ToString());
			return;
		}
        }

        public void TestWriteBytes() {
                byte[] readBytes = new byte[100];

		try {
			MemoryStream ms = new MemoryStream( 100 );

			for( int i = 0; i < 100; i++ ) {
				ms.WriteByte( testStreamData[i] );
			}

			ms.Seek( 0, SeekOrigin.Begin); 
			
			testStream.Read( readBytes, 0, 100 );

			VerifyTestData( readBytes, 0, 100 );
		}
		catch(Exception e){
			Fail("Threw an unexpected exception:"+e.ToString());
			return;
		}
        }               

        public void TestWriteBlock() {
                byte[] readBytes = new byte[100];

		try {
			MemoryStream ms = new MemoryStream( 100 );

			ms.Write( testStreamData, 0, 100 );

			ms.Seek( 0, SeekOrigin.Begin); 
			
			testStream.Read( readBytes, 0, 100 );

			VerifyTestData( readBytes, 0, 100 );

			byte[] arrayBytes = testStream.ToArray();

			VerifyTestData( arrayBytes, 0, 100 );
		}
		catch(Exception e){
			Fail("Threw an unexpected exception:"+e.ToString());
			return;
		}
        }
}

}
