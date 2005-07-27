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
// Novell.Directory.Ldap.Asn1.Asn1Identifier.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This class is used to encapsulate an ASN.1 Identifier.
	/// 
	/// An Asn1Identifier is composed of three parts:
	/// <li> a class type,</li>
	/// <li> a form, and</li>
	/// <li> a tag.</li>
	/// 
	/// The class type is defined as:
	/// <pre>
	/// bit 8 7 TAG CLASS
	/// ------- -----------
	/// 0 0 UNIVERSAL
	/// 0 1 APPLICATION
	/// 1 0 CONTEXT
	/// 1 1 PRIVATE
	/// </pre>
	///  The form is defined as:
	/// <pre>
	/// bit 6 FORM
	/// ----- --------
	/// 0 PRIMITIVE
	/// 1 CONSTRUCTED
	/// </pre>
	/// 
	///  Note: CONSTRUCTED types are made up of other CONSTRUCTED or PRIMITIVE
	/// types.
	/// 
	///  The tag is defined as:
	/// <pre>
	/// bit 5 4 3 2 1 TAG
	/// ------------- ---------------------------------------------
	/// 0 0 0 0 0
	/// . . . . .
	/// 1 1 1 1 0 (0-30) single octet tag
	/// 
	/// 1 1 1 1 1 (> 30) multiple octet tag, more octets follow
	/// </pre>
	/// </summary>
	public class Asn1Identifier:System.Object, System.ICloneable
	{
		/// <summary> Returns the CLASS of this Asn1Identifier as an int value.
		/// 
		/// </summary>
		/// <seealso cref="UNIVERSAL">
		/// </seealso>
		/// <seealso cref="APPLICATION">
		/// </seealso>
		/// <seealso cref="CONTEXT">
		/// </seealso>
		/// <seealso cref="PRIVATE">
		/// </seealso>
		virtual public int Asn1Class
		{
			get
			{
				return tagClass;
			}
			
		}
		/// <summary> Return a boolean indicating if the constructed bit is set.
		/// 
		/// </summary>
		/// <returns> true if constructed and false if primitive.
		/// </returns>
		virtual public bool Constructed
		{
			get
			{
				return constructed;
			}
			
		}
		/// <summary> Returns the TAG of this Asn1Identifier.</summary>
		virtual public int Tag
		{
			get
			{
				return tag;
			}
			
		}
		/// <summary> Returns the encoded length of this Asn1Identifier.</summary>
		virtual public int EncodedLength
		{
			get
			{
				return encodedLength;
			}
			
		}
		/// <summary> Returns a boolean value indicating whether or not this Asn1Identifier
		/// has a TAG CLASS of UNIVERSAL.
		/// 
		/// </summary>
		/// <seealso cref="UNIVERSAL">
		/// </seealso>
		[CLSCompliantAttribute(false)]
		virtual public bool Universal
		{
			get
			{
				return tagClass == UNIVERSAL;
			}
			
		}
		/// <summary> Returns a boolean value indicating whether or not this Asn1Identifier
		/// has a TAG CLASS of APPLICATION.
		/// 
		/// </summary>
		/// <seealso cref="APPLICATION">
		/// </seealso>
		[CLSCompliantAttribute(false)]
		virtual public bool Application
		{
			get
			{
				return tagClass == APPLICATION;
			}
			
		}
		/// <summary> Returns a boolean value indicating whether or not this Asn1Identifier
		/// has a TAG CLASS of CONTEXT-SPECIFIC.
		/// 
		/// </summary>
		/// <seealso cref="CONTEXT">
		/// </seealso>
		[CLSCompliantAttribute(false)]
		virtual public bool Context
		{
			get
			{
				return tagClass == CONTEXT;
			}
			
		}
		/// <summary> Returns a boolean value indicating whether or not this Asn1Identifier
		/// has a TAG CLASS of PRIVATE.
		/// 
		/// </summary>
		/// <seealso cref="PRIVATE"></seealso>
		[CLSCompliantAttribute(false)]
		virtual public bool Private
		{
			get
			{
				return tagClass == PRIVATE;
			}
			
		}
		
		/// <summary> Universal tag class.
		/// 
		///  UNIVERSAL = 0 
		/// </summary>
		public const int UNIVERSAL = 0;
		
		/// <summary> Application-wide tag class.
		/// 
		///  APPLICATION = 1 
		/// </summary>
		public const int APPLICATION = 1;
		
		/// <summary> Context-specific tag class.
		/// 
		///  CONTEXT = 2 
		/// </summary>
		public const int CONTEXT = 2;
		
		/// <summary> Private-use tag class.
		/// 
		///  PRIVATE = 3 
		/// </summary>
		public const int PRIVATE = 3;
		
		
		/* Private variables
		*/
		
		private int tagClass;
		private bool constructed;
		private int tag;
		private int encodedLength;
		
		/* Constructors for Asn1Identifier
		*/
		
		/// <summary> Constructs an Asn1Identifier using the classtype, form and tag.
		/// 
		/// </summary>
		/// <param name="tagClass">As defined above.
		/// 
		/// </param>
		/// <param name="constructed">Set to true if constructed and false if primitive.
		/// 
		/// </param>
		/// <param name="tag">The tag of this identifier
		/// </param>
		public Asn1Identifier(int tagClass, bool constructed, int tag)
		{
			this.tagClass = tagClass;
			this.constructed = constructed;
			this.tag = tag;
		}
		
		/// <summary> Decode an Asn1Identifier directly from an InputStream and
		/// save the encoded length of the Asn1Identifier.
		/// 
		/// </summary>
		/// <param name="in">The input stream to decode from.
		/// </param>
		public Asn1Identifier(System.IO.Stream in_Renamed)
		{
			int r = in_Renamed.ReadByte();
			encodedLength++;
			if (r < 0)
				throw new System.IO.EndOfStreamException("BERDecoder: decode: EOF in Identifier");
			tagClass = r >> 6;
			constructed = (r & 0x20) != 0;
			tag = r & 0x1F; // if tag < 30 then its a single octet identifier.
			if (tag == 0x1F)
			// if true, its a multiple octet identifier.
				tag = decodeTagNumber(in_Renamed);
			return ;
		}
		
		public Asn1Identifier()
		{
			return ;
		}
		
		/// <summary> Decode an Asn1Identifier directly from an InputStream and
		/// save the encoded length of the Asn1Identifier, but reuse the object.
		/// 
		/// </summary>
		/// <param name="in">The input stream to decode from.
		/// </param>
		public void  reset(System.IO.Stream in_Renamed)
		{
			encodedLength = 0;
			int r = in_Renamed.ReadByte();
			encodedLength++;
			if (r < 0)
				throw new System.IO.EndOfStreamException("BERDecoder: decode: EOF in Identifier");
			tagClass = r >> 6;
			constructed = (r & 0x20) != 0;
			tag = r & 0x1F; // if tag < 30 then its a single octet identifier.
			if (tag == 0x1F)
			// if true, its a multiple octet identifier.
				tag = decodeTagNumber(in_Renamed);
		}
		
		/// <summary> In the case that we have a tag number that is greater than 30, we need
		/// to decode a multiple octet tag number.
		/// </summary>
		private int decodeTagNumber(System.IO.Stream in_Renamed)
		{
			int n = 0;
			while (true)
			{
				int r = in_Renamed.ReadByte();
				encodedLength++;
				if (r < 0)
					throw new System.IO.EndOfStreamException("BERDecoder: decode: EOF in tag number");
				n = (n << 7) + (r & 0x7F);
				if ((r & 0x80) == 0)
					break;
			}
			return n;
		}
		
		/* Convenience methods
		*/
		
		/// <summary> Creates a duplicate, not a true clone, of this object and returns
		/// a reference to the duplicate.
		/// 
		/// </summary>
		public System.Object Clone()
		{
			try
			{
				return base.MemberwiseClone();
			}
			catch (System.Exception ce)
			{
				throw new System.SystemException("Internal error, cannot create clone");
			}
		}
	}
}
