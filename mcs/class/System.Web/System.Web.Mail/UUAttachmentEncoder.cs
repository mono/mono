//
// System.Web.Mail.UUAttachmentEncoder.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//
using System;
using System.IO;
using System.Text;

namespace System.Web.Mail {

    // a class that handles UU encoding for attachments
    internal class UUAttachmentEncoder : IAttachmentEncoder {
	
	protected byte[] beginTag;
	protected byte[] endTag;
	protected byte[] endl;
	
	public UUAttachmentEncoder( int mode , string fileName ) {
	    string endlstr = "\r\n";
	    
	    beginTag = 
		Encoding.ASCII.GetBytes( "begin " + mode + " " + fileName + endlstr); 
	    
	    endTag = 
		Encoding.ASCII.GetBytes( "`" + endlstr + "end" + endlstr ); 
	    
	    endl = Encoding.ASCII.GetBytes( endlstr );
	}
	
	// uu encodes a stream in to another stream
	public void EncodeStream(  Stream ins , Stream outs ) {
	    
	    // write the start tag
	    outs.Write( beginTag , 0 , beginTag.Length );	   
	    
	    // create the uu transfom and the buffers
	    ToUUEncodingTransform tr = new ToUUEncodingTransform();
	    byte[] input = new byte[ tr.InputBlockSize ];
	    byte[] output = new byte[ tr.OutputBlockSize ];
	    
	    while( true ) {
			
		// read from the stream until no more data is available
		int check = ins.Read( input , 0 , input.Length );
		if( check < 1 ) break;
		
		// if the read length is not InputBlockSize
		// write a the final block
		if( check == tr.InputBlockSize ) {
		    tr.TransformBlock( input , 0 , check , output , 0 );
		    outs.Write( output , 0 , output.Length );
		    outs.Write( endl , 0 , endl.Length );
		} else {
		    byte[] finalBlock = tr.TransformFinalBlock( input , 0 , check );
		    outs.Write( finalBlock , 0 , finalBlock.Length );
		    outs.Write( endl , 0 , endl.Length );
		    break;
		}
				
	    }
	    
	    // write the end tag.
	    outs.Write( endTag , 0 , endTag.Length );
        }
	
	
	
	
	
	
    }
    
}
