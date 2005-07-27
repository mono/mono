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
// Novell.Directory.Ldap.Asn1.Asn1OctetString.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class encapsulates the OCTET STRING type.</summary>
	public class Asn1OctetString:Asn1Object
	{
		
		private sbyte[] content;
		
		/// <summary> ASN.1 OCTET STRING tag definition.</summary>
		public const int TAG = 0x04;
		
		/// <summary> ID is added for Optimization.
		/// Id needs only be one Value for every instance,
		/// thus we create it only once.
		/// </summary>
		protected internal static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);
		/* Constructors for Asn1OctetString
		*/
		
		/// <summary> Call this constructor to construct an Asn1OctetString
		/// object from a byte array.
		/// 
		/// </summary>
		/// <param name="content">A byte array representing the string that
		/// will be contained in the this Asn1OctetString object
		/// </param>
		[CLSCompliantAttribute(false)]
		public Asn1OctetString(sbyte[] content):base(ID)
		{
			this.content = content;
			return ;
		}
		
		
		/// <summary> Call this constructor to construct an Asn1OctetString
		/// object from a String object.
		/// 
		/// </summary>
		/// <param name="content">A string value that will be contained
		/// in the this Asn1OctetString object
		/// </param>
		public Asn1OctetString(System.String content):base(ID)
		{
			try
			{
/*                System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
				byte[] bytes = utf8.GetBytes (content);
				sbyte[] sbytes = new sbyte[bytes.Length+1]; //signed bytes
				sbytes[0] = 0; //set sign byte to zero.
				for(int i=1; i<sbytes.Length; i++)
					sbytes[i] = (sbyte) bytes[i-1]; //cast byte-->sbyte
*/
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(content);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);

				this.content = sbytes;
//				this.content = content.getBytes("UTF8");
			}
			catch (System.IO.IOException uee)
			{
				throw new System.SystemException(uee.ToString());
			}
			return ;
		}
		
		
		/// <summary> Constructs an Asn1OctetString object by decoding data from an
		/// input stream.
		/// 
		/// </summary>
		/// <param name="dec">The decoder object to use when decoding the
		/// input stream.  Sometimes a developer might want to pass
		/// in his/her own decoder object
		/// 
		/// </param>
		/// <param name="in">A byte stream that contains the encoded ASN.1
		/// 
		/// </param>
		[CLSCompliantAttribute(false)]
		public Asn1OctetString(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(ID)
		{
			content = (len > 0)?(sbyte[]) dec.decodeOctetString(in_Renamed, len):new sbyte[0];
			return ;
		}
		
		
		/* Asn1Object implementation
		*/
		
		/// <summary> Call this method to encode the current instance into the
		/// specified output stream using the specified encoder object.
		/// 
		/// </summary>
		/// <param name="enc">Encoder object to use when encoding self.
		/// 
		/// </param>
		/// <param name="out">The output stream onto which the encoded byte
		/// stream is written.
		/// </param>
		public override void  encode(Asn1Encoder enc, System.IO.Stream out_Renamed)
		{
			enc.encode(this, out_Renamed);
			return ;
		}
		
		
		/*Asn1OctetString specific methods
		*/
		
		/// <summary> Returns the content of this Asn1OctetString as a byte array.</summary>
		[CLSCompliantAttribute(false)]
		public sbyte[] byteValue()
		{
			return content;
		}
		
		
		/// <summary> Returns the content of this Asn1OctetString as a String.</summary>
		public System.String stringValue()
		{
			System.String s = null;
			try
			{
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				char[] dchar = encoder.GetChars(SupportClass.ToByteArray(content));
				s = new String(dchar);
//				sbyte *sb=content;
//				s = new  String(sb,0,content.Length, new System.Text.UTF8Encoding());
			}
			catch (System.IO.IOException uee)
			{
				throw new System.SystemException(uee.ToString());
			}
			
			return s;
		}
		
		
		/// <summary> Return a String representation of this Asn1Object.</summary>
		public override System.String ToString()
		{
			return base.ToString() + "OCTET STRING: " + stringValue();
		}
	}
}
