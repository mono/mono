// Per Arneng <pt99par@student.bth.se>
using System;
using System.Security.Cryptography;

namespace System.Web.Mail {

    
    internal class ToUUEncodingTransform : ICryptoTransform {
	
	public int InputBlockSize { get { return 45; } }
	public int OutputBlockSize { get { return 61; } }

	public bool CanTransformMultipleBlocks { get { return true; } }
	public bool CanReuseTransform { get { return true; } }
    
	
	public int TransformBlock( byte[] inputBuffer,
				   int inputOffset,
				   int inputCount,
				   byte[] outputBuffer,
				   int outputOffset
				   ) {
	    
	    outputBuffer[ 0 ] = (byte)'M';
	    
	    for( int i=0;i<15;i++ ) {
		
		TransformTriplet( inputBuffer , inputOffset + i * 3 , 3,
				  outputBuffer , outputOffset + i * 4 + 1);
	    
	    }
	    
	    
	    return 61;
	}
	
	public byte[] TransformFinalBlock(byte[] inputBuffer,
					  int inputOffset,
					  int inputCount
					  ) {
	    
	    	    
	    int tripletBlocks = inputCount / 3 + 1;
	    
	    byte[] buffer = new byte[ tripletBlocks * 3 ];
	    Buffer.BlockCopy( inputBuffer,inputOffset, buffer,0,inputCount);
	    
	    byte[] outputBuffer = new byte[ tripletBlocks * 4 + 1 ];
	    outputBuffer[ 0 ] = (byte)(inputCount+0x20);
	    
	    for( int i =0 ; i < tripletBlocks ; i++ ) {
		TransformTriplet( inputBuffer , inputOffset + i * 3 , 3,
				  outputBuffer , i * 4 + 1);
	    }
	    
	    

	    return outputBuffer;
	    	    
	}

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
