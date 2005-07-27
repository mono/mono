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
// Novell.Directory.Ldap.Asn1.Asn1Boolean.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class encapsulates the ASN.1 BOOLEAN type.</summary>
	public class Asn1Boolean:Asn1Object
	{
		
		private bool content;
		
		/// <summary> ASN.1 BOOLEAN tag definition.</summary>
		public const int TAG = 0x01;
		
		/// <summary> ID is added for Optimization.
		/// 
		/// ID needs only be one Value for every instance,
		/// thus we create it only once.
		/// </summary>
		public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);
		/* Constructors for Asn1Boolean
		*/
		
		/// <summary> Call this constructor to construct an Asn1Boolean
		/// object from a boolean value.
		/// 
		/// </summary>
		/// <param name="content">The boolean value to be contained in the
		/// this Asn1Boolean object
		/// </param>
		public Asn1Boolean(bool content):base(ID)
		{
			this.content = content;
			return ;
		}
		
		/// <summary> Constructs an Asn1Boolean object by decoding data from an
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
		public Asn1Boolean(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(ID)
		{
			content = ((System.Boolean) dec.decodeBoolean(in_Renamed, len));
			return ;
		}
		
		/* Asn1Object implementation
		*/
		
		/// <summary> Encode the current instance into the
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
		
		/* Asn1Boolean specific methods
		*/
		
		/// <summary> Returns the content of this Asn1Boolean as a boolean.</summary>
		public bool booleanValue()
		{
			return content;
		}
		
		/// <summary> Returns a String representation of this Asn1Boolean object.</summary>
		public override System.String ToString()
		{
			return base.ToString() + "BOOLEAN: " + content;
		}
	}
}
