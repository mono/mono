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
// Novell.Directory.Ldap.Asn1.Asn1Integer.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class encapsulates the ASN.1 INTEGER type.</summary>
	public class Asn1Integer:Asn1Numeric
	{
		
		/// <summary> ASN.1 INTEGER tag definition.</summary>
		public const int TAG = 0x02;
		
		/// <summary> ID is added for Optimization.</summary>
		/// <summary> ID needs only be one Value for every instance,
		/// thus we create it only once.
		/// </summary>
		public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);
		/* Constructors for Asn1Integer
		*/
		
		/// <summary> Call this constructor to construct an Asn1Integer
		/// object from an integer value.
		/// 
		/// </summary>
		/// <param name="content">The integer value to be contained in the
		/// this Asn1Integer object
		/// </param>
		public Asn1Integer(int content):base(ID, content)
		{
			return ;
		}
		
		/// <summary> Call this constructor to construct an Asn1Integer
		/// object from a long value.
		/// 
		/// </summary>
		/// <param name="content">The long value to be contained in the
		/// this Asn1Integer object
		/// </param>
		public Asn1Integer(long content):base(ID, content)
		{
			return ;
		}
		
		/// <summary> Constructs an Asn1Integer object by decoding data from an
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
		public Asn1Integer(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(ID, (System.Int64) dec.decodeNumeric(in_Renamed, len))
		{
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
		
		/* Asn1Integer specific methods
		*/
		
		/// <summary> Returns a String representation of this Asn1Integer object.</summary>
		public override System.String ToString()
		{
			return base.ToString() + "INTEGER: " + longValue();
		}
	}
}
