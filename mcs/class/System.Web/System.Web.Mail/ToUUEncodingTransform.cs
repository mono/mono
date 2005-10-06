//
// System.Web.Mail.ToUUEncodingTransform.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//

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
//
using System;
using System.Security.Cryptography;

namespace System.Web.Mail {

    // This class transforms blocks of plaintext to UU encoding
    internal class ToUUEncodingTransform : ICryptoTransform {
	
	public int InputBlockSize { get { return 45; } }
	public int OutputBlockSize { get { return 61; } }

	public bool CanTransformMultipleBlocks { get { return true; } }
	public bool CanReuseTransform { get { return true; } }
    
	// transforms a block of bytes to UU encoding
	public int TransformBlock( byte[] inputBuffer,
				   int inputOffset,
				   int inputCount,
				   byte[] outputBuffer,
				   int outputOffset
				   ) {
	    
	    // write the line length length+0x20
	    outputBuffer[ 0 ] = (byte)'M';
	    
	    // transform the block 3bytes at a time
	    for( int i=0;i<15;i++ ) {
		
		TransformTriplet( inputBuffer , inputOffset + i * 3 , 3,
				  outputBuffer , outputOffset + i * 4 + 1);
	    
	    }
	    
	    
	    return OutputBlockSize;
	}
	
	// make a final uu transformations
	public byte[] TransformFinalBlock(byte[] inputBuffer,
					  int inputOffset,
					  int inputCount
					  ) {
	    
	    // calculate how many 4-byte blocks there are
	    int tripletBlocks = inputCount / 3 + 1;
	    
	    // create a new buffer and copy the input data into that
	    byte[] buffer = new byte[ tripletBlocks * 3 ];
	    Buffer.BlockCopy( inputBuffer,inputOffset, buffer,0,inputCount);
	    
	    // create the outpur buffer and set the first byte
	    // to the length+0x20
	    byte[] outputBuffer = new byte[ tripletBlocks * 4 + 1 ];
	    outputBuffer[ 0 ] = (byte)(inputCount+0x20);
	    
	    // transform the block 3bytes at a time
	    for( int i =0 ; i < tripletBlocks ; i++ ) {
		TransformTriplet( inputBuffer , inputOffset + i * 3 , 3,
				  outputBuffer , i * 4 + 1);
	    }
	    
	    

	    return outputBuffer;
	    	    
	}
	
	// transforms a 3byte buffer to a 4byte uuencoded buffer
	protected int TransformTriplet( byte[] inputBuffer,
					int inputOffset,
					int inputCount,
					byte[] outputBuffer,
					int outputOffset
					) {
	    
	    byte a = inputBuffer[ inputOffset + 0 ];
	    byte b = inputBuffer[ inputOffset + 1 ];
	    byte c = inputBuffer[ inputOffset + 2 ];
	    
	    outputBuffer[ outputOffset + 0 ] = 
		(byte)(0x20 + (( a >> 2                    ) & 0x3F));
	    
	    outputBuffer[ outputOffset + 1 ] = 
		(byte)(0x20 + (((a << 4) | ((b >> 4) & 0xF)) & 0x3F));
	    
	    outputBuffer[ outputOffset + 2 ] = 
		(byte)(0x20 + (((b << 2) | ((c >> 6) & 0x3)) & 0x3F));
	    
	    outputBuffer[ outputOffset + 3 ] = 
		(byte)(0x20 + (( c                         ) & 0x3F));
	    
	    // tanslate all 0x20 to 0x60 according to specs
	    for( int i = 0; i < 4; i++ ) {
		if( outputBuffer[ outputOffset + i ] == 0x20 ) {
		    outputBuffer[ outputOffset + i ] = 0x60;
		}
	    }
	    
	    return 4;
	}

	public void Dispose() {}
    }

}
