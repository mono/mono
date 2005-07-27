/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Asn1.Asn1Encoder.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This interface defines the methods for encoding each of the ASN.1 types.
	/// 
	/// Encoders which implement this interface may be used to encode any of the
	/// Asn1Object data types.
	/// 
	/// This package also provides the BEREncoder class that can be used to 
	/// BER encode ASN.1 classes.  However an application might chose to use 
	/// its own encoder class.
	/// 
	/// This interface thus allows an application to use this package to
	/// encode ASN.1 objects using other encoding rules if needed.  
	/// 
	/// Note that Ldap packets are required to be BER encoded. Since this package
	/// includes a BER encoder no application provided encoder is needed for 
	/// building Ldap packets.
	/// </summary>
	public interface Asn1Encoder : System.Runtime.Serialization.ISerializable
		{
			
			/* Encoders for ASN.1 simple types */
			
			/// <summary> Encode an Asn1Boolean directly into the provided output stream.
			/// 
			/// </summary>
			/// <param name="b">The Asn1Boolean object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded    
			/// </param>
			void  encode(Asn1Boolean b, System.IO.Stream out_Renamed);
			
			/// <summary> Encode an Asn1Numeric directly to a stream.
			/// 
			/// Use a two's complement representation in the fewest number of octets
			/// possible.
			/// 
			/// Can be used to encode both INTEGER and ENUMERATED values.
			/// 
			/// </summary>
			/// <param name="n">The Asn1Numeric object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded
			/// </param>
			void  encode(Asn1Numeric n, System.IO.Stream out_Renamed);
			
			/* Asn1 TYPE NOT YET SUPPORTED
			* Encode an Asn1Real directly to a stream.
			* public void encode(Asn1Real r, OutputStream out)
			* throws IOException;
			*/
			
			/// <summary> Encode an Asn1Null directly to a stream.
			/// 
			/// </summary>
			/// <param name="n">The Asn1Null object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded    
			/// </param>
			void  encode(Asn1Null n, System.IO.Stream out_Renamed);
			
			/* Asn1 TYPE NOT YET SUPPORTED
			* Encode an Asn1BitString directly to a stream.
			* public void encode(Asn1BitString bs, OutputStream out)
			* throws IOException;
			*/
			
			/// <summary> Encode an Asn1OctetString directly to a stream.
			/// 
			/// </summary>
			/// <param name="os">The Asn1OctetString object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded     
			/// </param>
			void  encode(Asn1OctetString os, System.IO.Stream out_Renamed);
			
			/* Asn1 TYPE NOT YET SUPPORTED
			* Encode an Asn1ObjectIdentifier directly to a stream.
			* public void encode(Asn1ObjectIdentifier oi, OutputStream out)
			* throws IOException;
			*/
			
			/* Asn1 TYPE NOT YET SUPPORTED
			* Encode an Asn1CharacterString directly to a stream.
			* public void encode(Asn1CharacterString cs, OutputStream out)
			* throws IOException;
			*/
			
			/* Encoder for ASN.1 structured types
			*/
			
			/// <summary> Encode an Asn1Structured directly to a stream.
			/// 
			/// </summary>
			/// <param name="c">The Asn1Structured object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded  
			/// </param>
			void  encode(Asn1Structured c, System.IO.Stream out_Renamed);
			
			/// <summary> Encode an Asn1Tagged directly to a stream.
			/// 
			/// </summary>
			/// <param name="t">The Asn1Tagged object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded      
			/// </param>
			void  encode(Asn1Tagged t, System.IO.Stream out_Renamed);
			
			/* Encoders for ASN.1 useful types
			*/
			
			/* Encoder for ASN.1 Identifier
			*/
			
			/// <summary> Encode an Asn1Identifier directly to a stream.
			/// 
			/// </summary>
			/// <param name="id">The Asn1Identifier object to encode
			/// 
			/// </param>
			/// <param name="out">The output stream onto which the ASN.1 object is 
			/// to be encoded      
			/// </param>
			void  encode(Asn1Identifier id, System.IO.Stream out_Renamed);
		}
}
