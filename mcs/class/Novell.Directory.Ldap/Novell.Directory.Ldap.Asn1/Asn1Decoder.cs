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
// Novell.Directory.Ldap.Asn1.Asn1Decoder.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This interface defines the methods for decoding each of the ASN.1 types.
	/// 
	/// Decoders which implement this interface may be used to decode any of the
	/// Asn1Object data types.
	/// 
	/// This package also provides the BERDecoder class that can be used to 
	/// BER decode ASN.1 classes.  However an application might chose to use 
	/// its own decoder class.
	/// 
	/// This interface thus allows an application to use this package to
	/// decode ASN.1 objects using other decoding rules if needed.  
	/// 
	/// Note that Ldap packets are required to be BER encoded. Since this package
	/// includes a BER decoder no application provided decoder is needed for 
	/// building Ldap packets.
	/// </summary>
	[CLSCompliantAttribute(false)]
	public interface Asn1Decoder : System.Runtime.Serialization.ISerializable
		{
			
			/// <summary> Decode an encoded value into an Asn1Object from a byte array.
			/// 
			/// </summary>
			/// <param name="value">A byte array that points to the encoded Asn1 data
			/// </param>
			Asn1Object decode(sbyte[] value_Renamed);
			
			
			/// <summary> Decode an encoded value into an Asn1Object from an InputStream.
			/// 
			/// </summary>
			/// <param name="in">An input stream containig the encoded ASN.1 data.
			/// </param>
			Asn1Object decode(System.IO.Stream in_Renamed);
			
			
			/// <summary> Decode an encoded value into an Asn1Object from an InputStream.
			/// 
			/// </summary>
			/// <param name="length">The decoded components encoded length. This value is
			/// handy when decoding structured types. It allows you to accumulate 
			/// the number of bytes decoded, so you know when the structured 
			/// type has decoded all of its components.
			/// 
			/// </param>
			/// <param name="in">An input stream containig the encoded ASN.1 data.
			/// </param>
			Asn1Object decode(System.IO.Stream in_Renamed, int[] length);
			
			/* Decoders for ASN.1 simple types
			*/
			
			/// <summary> Decode a BOOLEAN directly from a stream. Call this method when you
			/// know that the next ASN.1 encoded element is a BOOLEAN
			/// 
			/// </summary>
			/// <param name="in">An input stream containig the encoded ASN.1 data.
			/// 
			/// </param>
			/// <param name="len">Length in bytes
			/// </param>
			System.Object decodeBoolean(System.IO.Stream in_Renamed, int len);
			
			/// <summary> Decode a Numeric value directly from a stream.  Call this method when you
			/// know that the next ASN.1 encoded element is a Numeric
			/// 
			/// Can be used to decodes INTEGER and ENUMERATED types.
			/// 
			/// </summary>
			/// <param name="in">An input stream containig the encoded ASN.1 data.
			/// 
			/// </param>
			/// <param name="len">Length in bytes    
			/// </param>
			System.Object decodeNumeric(System.IO.Stream in_Renamed, int len);
			
			
			
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode a REAL directly from a stream.
			* public Object decodeReal(InputStream in, int len)
			* throws IOException;
			*/
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode a BIT_STRING directly from a stream.
			* public Object decodeBitString(InputStream in, int len)
			* throws IOException;
			*/
			
			
			
			/// <summary> Decode an OCTET_STRING directly from a stream. Call this method when you
			/// know that the next ASN.1 encoded element is a OCTET_STRING.
			/// 
			/// </summary>
			/// <param name="in">An input stream containig the encoded ASN.1 data.
			/// 
			/// </param>
			/// <param name="len">Length in bytes    
			/// </param>
			System.Object decodeOctetString(System.IO.Stream in_Renamed, int len);
			
			
			
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode an OBJECT_IDENTIFIER directly from a stream.
			* public Object decodeObjectIdentifier(InputStream in, int len)
			* throws IOException;
			*/
			
			
			
			/// <summary> Decode a CharacterString directly from a stream.
			/// 
			/// Decodes any of the specialized character strings.
			/// 
			/// </summary>
			/// <param name="in">An input stream containig the encoded ASN.1 data.
			/// 
			/// </param>
			/// <param name="len">Length in bytes    
			/// </param>
			System.Object decodeCharacterString(System.IO.Stream in_Renamed, int len);
			
			/* No Decoders for ASN.1 structured types. A structured type's value is a
			* collection of other types.
			*/
			
			
			/* Decoders for ASN.1 useful types
			*/
			
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode a GENERALIZED_TIME directly from a stream.
			* public Object decodeGeneralizedTime(InputStream in, int len)
			* throws IOException;
			*/
			
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode a UNIVERSAL_TIME directly from a stream.
			* public Object decodeUniversalTime(InputStream in, int len)
			* throws IOException;
			*/
			
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode an EXTERNAL directly from a stream.
			* public Object decodeExternal(InputStream in, int len)
			* throws IOException;
			*/
			
			
			/* Asn1 TYPE NOT YET SUPPORTED  
			* Decode an OBJECT_DESCRIPTOR directly from a stream.
			* public Object decodeObjectDescriptor(InputStream in, int len)
			* throws IOException;
			*/
		}
}
