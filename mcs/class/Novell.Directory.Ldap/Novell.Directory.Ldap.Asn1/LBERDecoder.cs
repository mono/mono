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
// Novell.Directory.Ldap.Asn1.LBERDecoder.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class provides LBER decoding routines for ASN.1 Types. LBER is a
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
	public class LBERDecoder : Asn1Decoder
	{
		public LBERDecoder()
		{
			InitBlock();
		}
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}

		private void  InitBlock()
		{
			asn1ID = new Asn1Identifier();
			asn1Len = new Asn1Length();
		}
		//used to speed up decode, so it doesn't need to recreate an identifier every time
		//instead just reset is called CANNOT be static for multiple connections
		private Asn1Identifier asn1ID;
		private Asn1Length asn1Len;
		
		
		/* Generic decode routines
		*/
		
		/// <summary> Decode an LBER encoded value into an Asn1Type from a byte array.</summary>
		[CLSCompliantAttribute(false)]
		public virtual Asn1Object decode(sbyte[] value_Renamed)
		{
			Asn1Object asn1 = null;
			
			System.IO.MemoryStream in_Renamed = new System.IO.MemoryStream(SupportClass.ToByteArray(value_Renamed));
			try
			{
				asn1 = decode(in_Renamed);
			}
			catch (System.IO.IOException ioe)
			{
			}
			return asn1;
		}
		
		/// <summary> Decode an LBER encoded value into an Asn1Type from an InputStream.</summary>
		public virtual Asn1Object decode(System.IO.Stream in_Renamed)
		{
			int[] len = new int[1];
			return decode(in_Renamed, len);
		}
		
		/// <summary> Decode an LBER encoded value into an Asn1Object from an InputStream.
		/// 
		///  This method also returns the total length of this encoded
		/// Asn1Object (length of type + length of length + length of content)
		/// in the parameter len. This information is helpful when decoding
		/// structured types.
		/// </summary>
		public virtual Asn1Object decode(System.IO.Stream in_Renamed, int[] len)
		{
			asn1ID.reset(in_Renamed);
			asn1Len.reset(in_Renamed);
			
			int length = asn1Len.Length;
			len[0] = asn1ID.EncodedLength + asn1Len.EncodedLength + length;
			
			if (asn1ID.Universal)
			{
				switch (asn1ID.Tag)
				{
					
					case Asn1Sequence.TAG: 
						return new Asn1Sequence(this, in_Renamed, length);
					
					case Asn1Set.TAG: 
						return new Asn1Set(this, in_Renamed, length);
					
					case Asn1Boolean.TAG: 
						return new Asn1Boolean(this, in_Renamed, length);
					
					case Asn1Integer.TAG: 
						return new Asn1Integer(this, in_Renamed, length);
					
					case Asn1OctetString.TAG: 
						return new Asn1OctetString(this, in_Renamed, length);
					
					case Asn1Enumerated.TAG: 
						return new Asn1Enumerated(this, in_Renamed, length);
					
					case Asn1Null.TAG: 
						return new Asn1Null(); // has no content to decode.
						/* Asn1 TYPE NOT YET SUPPORTED
						case Asn1BitString.TAG:
						return new Asn1BitString(this, in, length);
						case Asn1ObjectIdentifier.TAG:
						return new Asn1ObjectIdentifier(this, in, length);
						case Asn1Real.TAG:
						return new Asn1Real(this, in, length);
						case Asn1NumericString.TAG:
						return new Asn1NumericString(this, in, length);
						case Asn1PrintableString.TAG:
						return new Asn1PrintableString(this, in, length);
						case Asn1TeletexString.TAG:
						return new Asn1TeletexString(this, in, length);
						case Asn1VideotexString.TAG:
						return new Asn1VideotexString(this, in, length);
						case Asn1IA5String.TAG:
						return new Asn1IA5String(this, in, length);
						case Asn1GraphicString.TAG:
						return new Asn1GraphicString(this, in, length);
						case Asn1VisibleString.TAG:
						return new Asn1VisibleString(this, in, length);
						case Asn1GeneralString.TAG:
						return new Asn1GeneralString(this, in, length);
						*/
					
					
					default: 
						throw new System.IO.EndOfStreamException("Unknown tag"); // !!! need a better exception
					
				}
			}
			else
			{
				// APPLICATION or CONTEXT-SPECIFIC tag
				return new Asn1Tagged(this, in_Renamed, length, (Asn1Identifier) asn1ID.Clone());
			}
		}
		
		/* Decoders for ASN.1 simple type Contents
		*/
		
		/// <summary> Decode a boolean directly from a stream.</summary>
		public System.Object decodeBoolean(System.IO.Stream in_Renamed, int len)
		{
			sbyte[] lber = new sbyte[len];
			
			int i = SupportClass.ReadInput(in_Renamed, ref lber, 0, lber.Length);
			
			if (i != len)
				throw new System.IO.EndOfStreamException("LBER: BOOLEAN: decode error: EOF");
			
			return (lber[0] == 0x00)?false:true;
		}
		
		/// <summary> Decode a Numeric type directly from a stream. Decodes INTEGER
		/// and ENUMERATED types.
		/// </summary>
		public System.Object decodeNumeric(System.IO.Stream in_Renamed, int len)
		{
			long l = 0;
			int r = in_Renamed.ReadByte();
			
			if (r < 0)
				throw new System.IO.EndOfStreamException("LBER: NUMERIC: decode error: EOF");
			
			if ((r & 0x80) != 0)
			{
				// check for negative number
				l = - 1;
			}
			
			l = (l << 8) | r;
			
			for (int i = 1; i < len; i++)
			{
				r = in_Renamed.ReadByte();
				if (r < 0)
					throw new System.IO.EndOfStreamException("LBER: NUMERIC: decode error: EOF");
				l = (l << 8) | r;
			}
			return (System.Int64) l;
		}
		
		/// <summary> Decode an OctetString directly from a stream.</summary>
		public System.Object decodeOctetString(System.IO.Stream in_Renamed, int len)
		{
			sbyte[] octets = new sbyte[len];
			int totalLen = 0;
			
			while (totalLen < len)
			{
				// Make sure we have read all the data
				int inLen = SupportClass.ReadInput(in_Renamed, ref octets, totalLen, len - totalLen);
				totalLen += inLen;
			}
			
			return octets;
		}
		
		/// <summary> Decode a CharacterString directly from a stream.</summary>
		public System.Object decodeCharacterString(System.IO.Stream in_Renamed, int len)
		{
			sbyte[] octets = new sbyte[len];
			
			for (int i = 0; i < len; i++)
			{
				int ret = in_Renamed.ReadByte(); // blocks
				if (ret == - 1)
					throw new System.IO.EndOfStreamException("LBER: CHARACTER STRING: decode error: EOF");
				octets[i] = (sbyte) ret;
			}
			System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
			char[] dchar = encoder.GetChars(SupportClass.ToByteArray(octets));
			string rval = new String(dchar);
			
			return rval;//new String( "UTF8");
		}
	}
}
