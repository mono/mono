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
// Novell.Directory.Ldap.Asn1.Asn1Length.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class provides a means to manipulate ASN.1 Length's. It will
	/// be used by Asn1Encoder's and Asn1Decoder's by composition.
	/// </summary>
	public class Asn1Length
	{
		/// <summary> Returns the length of this Asn1Length.</summary>
		virtual public int Length
		{
			get
			{
				return length;
			}
			
		}
		/// <summary> Returns the encoded length of this Asn1Length.</summary>
		virtual public int EncodedLength
		{
			get
			{
				return encodedLength;
			}
			
		}
		
		/* Private variables
		*/
		
		private int length;
		private int encodedLength;
		
		/* Constructors for Asn1Length
		*/
		
		/// <summary> Constructs an empty Asn1Length.  Values are added by calling reset</summary>
		public Asn1Length()
		{
		}
		/// <summary> Constructs an Asn1Length</summary>
		public Asn1Length(int length)
		{
			this.length = length;
		}
		
		/// <summary> Constructs an Asn1Length object by decoding data from an
		/// input stream.
		/// 
		/// </summary>
		/// <param name="in">A byte stream that contains the encoded ASN.1
		/// 
		/// </param>
		public Asn1Length(System.IO.Stream in_Renamed)
		{
			int r = in_Renamed.ReadByte();
			encodedLength++;
			if (r == 0x80)
				length = - 1;
			else if (r < 0x80)
				length = r;
			else
			{
				length = 0;
				for (r = r & 0x7F; r > 0; r--)
				{
					int part = in_Renamed.ReadByte();
					encodedLength++;
					if (part < 0)
						throw new System.IO.EndOfStreamException("BERDecoder: decode: EOF in Asn1Length");
					length = (length << 8) + part;
				}
			}
		}
		
		/// <summary> Resets an Asn1Length object by decoding data from an
		/// input stream.
		/// 
		/// Note: this was added for optimization of Asn1.LBERdecoder.decode()
		/// 
		/// </summary>
		/// <param name="in">A byte stream that contains the encoded ASN.1
		/// 
		/// </param>
		public void  reset(System.IO.Stream in_Renamed)
		{
			encodedLength = 0;
			int r = in_Renamed.ReadByte();
			encodedLength++;
			if (r == 0x80)
				length = - 1;
			else if (r < 0x80)
				length = r;
			else
			{
				length = 0;
				for (r = r & 0x7F; r > 0; r--)
				{
					int part = in_Renamed.ReadByte();
					encodedLength++;
					if (part < 0)
						throw new System.IO.EndOfStreamException("BERDecoder: decode: EOF in Asn1Length");
					length = (length << 8) + part;
				}
			}
		}
	}
}
