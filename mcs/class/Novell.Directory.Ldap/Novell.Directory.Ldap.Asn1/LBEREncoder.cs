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
// Novell.Directory.Ldap.Asn1.LBEREncoder.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class provides LBER encoding routines for ASN.1 Types. LBER is a
	/// subset of BER as described in the following taken from 5.1 of RFC 2251:
	/// 
	/// 5.1. Mapping Onto BER-based Transport Services
	/// 
	/// The protocol elements of Ldap are encoded for exchange using the
	/// Basic Encoding Rules (BER) [11] of ASN.1 [3]. However, due to the
	/// high overhead involved in using certain elements of the BER, the
	/// following additional restrictions are placed on BER-encodings of Ldap
	/// protocol elements:
	/// 
	/// <li>(1) Only the definite form of length encoding will be used.</li>
	/// 
	/// <li>(2) OCTET STRING values will be encoded in the primitive form only.</li>
	/// 
	/// <li>(3) If the value of a BOOLEAN type is true, the encoding MUST have
	/// its contents octets set to hex "FF".</li>
	/// 
	/// <li>(4) If a value of a type is its default value, it MUST be absent.
	/// Only some BOOLEAN and INTEGER types have default values in this
	/// protocol definition.
	/// 
	/// These restrictions do not apply to ASN.1 types encapsulated inside of
	/// OCTET STRING values, such as attribute values, unless otherwise
	/// noted.</li>
	/// 
	/// [3] ITU-T Rec. X.680, "Abstract Syntax Notation One (ASN.1) -
	/// Specification of Basic Notation", 1994.
	/// 
	/// [11] ITU-T Rec. X.690, "Specification of ASN.1 encoding rules: Basic,
	/// Canonical, and Distinguished Encoding Rules", 1994.
	/// 
	/// </summary>
	public class LBEREncoder : Asn1Encoder
	{
		
		/* Encoders for ASN.1 simple type Contents
		*/
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}
		/// <summary> BER Encode an Asn1Boolean directly into the specified output stream.</summary>
		public virtual void  encode(Asn1Boolean b, System.IO.Stream out_Renamed)
		{
			/* Encode the id */
			encode(b.getIdentifier(), out_Renamed);
			
			/* Encode the length */
			out_Renamed.WriteByte((System.Byte) 0x01);
			
			/* Encode the boolean content*/
			out_Renamed.WriteByte((byte) (b.booleanValue()?(sbyte) SupportClass.Identity(0xff):(sbyte) 0x00));
			
			return ;
		}
		
		/// <summary> Encode an Asn1Numeric directly into the specified outputstream.
		/// 
		/// Use a two's complement representation in the fewest number of octets
		/// possible.
		/// 
		/// Can be used to encode INTEGER and ENUMERATED values.
		/// </summary>
		public void  encode(Asn1Numeric n, System.IO.Stream out_Renamed)
		{
			sbyte[] octets = new sbyte[8];
			sbyte len;
			long value_Renamed = n.longValue();
			long endValue = (value_Renamed < 0)?- 1:0;
			long endSign = endValue & 0x80;
			
			for (len = 0; len == 0 || value_Renamed != endValue || (octets[len - 1] & 0x80) != endSign; len++)
			{
				octets[len] = (sbyte) (value_Renamed & 0xFF);
				value_Renamed >>= 8;
			}
			
			encode(n.getIdentifier(), out_Renamed);
			out_Renamed.WriteByte((byte) len); // Length
			for (int i = len - 1; i >= 0; i--)
			// Content
				out_Renamed.WriteByte((byte) octets[i]);
			return ;
		}
		
		/* Asn1 TYPE NOT YET SUPPORTED
		* Encode an Asn1Real directly to a stream.
		public void encode(Asn1Real r, OutputStream out)
		throws IOException
		{
		throw new IOException("LBEREncoder: Encode to a stream not implemented");
		}
		*/
		
		/// <summary> Encode an Asn1Null directly into the specified outputstream.</summary>
		public void  encode(Asn1Null n, System.IO.Stream out_Renamed)
		{
			encode(n.getIdentifier(), out_Renamed);
			out_Renamed.WriteByte((System.Byte) 0x00); // Length (with no Content)
			return ;
		}
		
		/* Asn1 TYPE NOT YET SUPPORTED
		* Encode an Asn1BitString directly to a stream.
		public void encode(Asn1BitString bs, OutputStream out)
		throws IOException
		{
		throw new IOException("LBEREncoder: Encode to a stream not implemented");
		}
		*/
		
		/// <summary> Encode an Asn1OctetString directly into the specified outputstream.</summary>
		public void  encode(Asn1OctetString os, System.IO.Stream out_Renamed)
		{
			encode(os.getIdentifier(), out_Renamed);
			encodeLength(os.byteValue().Length, out_Renamed);
			sbyte[] temp_sbyteArray;
			temp_sbyteArray = os.byteValue();
			out_Renamed.Write(SupportClass.ToByteArray(temp_sbyteArray), 0, temp_sbyteArray.Length);;;
			return ;
		}
		
		/* Asn1 TYPE NOT YET SUPPORTED
		* Encode an Asn1ObjectIdentifier directly to a stream.
		* public void encode(Asn1ObjectIdentifier oi, OutputStream out)
		* throws IOException
		* {
		* throw new IOException("LBEREncoder: Encode to a stream not implemented");
		* }
		*/
		
		/* Asn1 TYPE NOT YET SUPPORTED
		* Encode an Asn1CharacterString directly to a stream.
		* public void encode(Asn1CharacterString cs, OutputStream out)
		* throws IOException
		* {
		* throw new IOException("LBEREncoder: Encode to a stream not implemented");
		* }
		*/
		
		/* Encoders for ASN.1 structured types
		*/
		
		/// <summary> Encode an Asn1Structured into the specified outputstream.  This method
		/// can be used to encode SET, SET_OF, SEQUENCE, SEQUENCE_OF
		/// </summary>
		public void  encode(Asn1Structured c, System.IO.Stream out_Renamed)
		{
			encode(c.getIdentifier(), out_Renamed);
			
			Asn1Object[] value_Renamed = c.toArray();
			
			System.IO.MemoryStream output = new System.IO.MemoryStream();
			
			/* Cycle through each element encoding each element */
			for (int i = 0; i < value_Renamed.Length; i++)
			{
				(value_Renamed[i]).encode(this, output);
			}
			
			/* Encode the length */
			encodeLength((int)output.Length, out_Renamed);
			
			/* Add each encoded element into the output stream */
			sbyte[] temp_sbyteArray;
			temp_sbyteArray = SupportClass.ToSByteArray(output.ToArray());
			out_Renamed.Write(SupportClass.ToByteArray(temp_sbyteArray), 0, temp_sbyteArray.Length);;;
			return ;
		}
		
		/// <summary> Encode an Asn1Tagged directly into the specified outputstream.</summary>
		public void  encode(Asn1Tagged t, System.IO.Stream out_Renamed)
		{
			if (t.Explicit)
			{
				encode(t.getIdentifier(), out_Renamed);
				
				/* determine the encoded length of the base type. */
				System.IO.MemoryStream encodedContent = new System.IO.MemoryStream();
				t.taggedValue().encode(this, encodedContent);
				
				encodeLength((int)encodedContent.Length, out_Renamed);
				sbyte[] temp_sbyteArray;
				temp_sbyteArray = SupportClass.ToSByteArray(encodedContent.ToArray());
				out_Renamed.Write(SupportClass.ToByteArray(temp_sbyteArray), 0, temp_sbyteArray.Length);;;;
			}
			else
			{
				t.taggedValue().encode(this, out_Renamed);
			}
			return ;
		}
		
		/* Encoders for ASN.1 useful types
		*/
		/* Encoder for ASN.1 Identifier
		*/
		
		/// <summary> Encode an Asn1Identifier directly into the specified outputstream.</summary>
		public void  encode(Asn1Identifier id, System.IO.Stream out_Renamed)
		{
			int c = id.Asn1Class;
			int t = id.Tag;
			sbyte ccf = (sbyte) ((c << 6) | (id.Constructed?0x20:0));
			
			if (t < 30)
			{
				/* single octet */
				out_Renamed.WriteByte((System.Byte) (ccf | t));
			}
			else
			{
				/* multiple octet */
				out_Renamed.WriteByte((System.Byte) (ccf | 0x1F));
				encodeTagInteger(t, out_Renamed);
			}
			return ;
		}
		
		/* Private helper methods
		*/
		
		/*
		*  Encodes the specified length into the the outputstream
		*/
		private void  encodeLength(int length, System.IO.Stream out_Renamed)
		{
			if (length < 0x80)
			{
				out_Renamed.WriteByte((System.Byte) length);
			}
			else
			{
				sbyte[] octets = new sbyte[4]; // 4 bytes sufficient for 32 bit int.
				sbyte n;
				for (n = 0; length != 0; n++)
				{
					octets[n] = (sbyte) (length & 0xFF);
					length >>= 8;
				}
				
				out_Renamed.WriteByte((System.Byte) (0x80 | n));
				
				for (int i = n - 1; i >= 0; i--)
					out_Renamed.WriteByte((byte) octets[i]);
			}
			return ;
		}
		
		/// <summary> Encodes the provided tag into the outputstream.</summary>
		private void  encodeTagInteger(int value_Renamed, System.IO.Stream out_Renamed)
		{
			sbyte[] octets = new sbyte[5];
			int n;
			for (n = 0; value_Renamed != 0; n++)
			{
				octets[n] = (sbyte) (value_Renamed & 0x7F);
				value_Renamed = value_Renamed >> 7;
			}
			for (int i = n - 1; i > 0; i--)
			{
				out_Renamed.WriteByte((System.Byte) (octets[i] | 0x80));
			}
			out_Renamed.WriteByte((byte) octets[0]);
			return ;
		}
	}
}
