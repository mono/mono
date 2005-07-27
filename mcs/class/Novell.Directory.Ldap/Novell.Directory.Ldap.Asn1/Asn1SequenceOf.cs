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
// Novell.Directory.Ldap.Asn1.Asn1SequenceOf.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> The Asn1SequenceOf class is used to hold an ordered collection
	/// of components with identical type.  This class inherits
	/// from the Asn1Structured class which already provides
	/// functionality to hold multiple Asn1 components.
	/// </summary>
	public class Asn1SequenceOf:Asn1Structured
	{
		
		/// <summary> ASN.1 SEQUENCE OF tag definition.</summary>
		public const int TAG = 0x10;
		
		/// <summary> ID is added for Optimization.
		/// 
		/// ID needs only be one Value for every instance,
		/// thus we create it only once.
		/// </summary>
		public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, true, TAG);
		
		/* Constructors for Asn1SequenceOf
		*/
		
		/// <summary> Constructs an Asn1SequenceOf object with no actual
		/// Asn1Objects in it. Assumes a default size of 5 elements.
		/// </summary>
		public Asn1SequenceOf():base(ID)
		{
			return ;
		}
		
		
		/// <summary> Constructs an Asn1SequenceOf object with the specified
		/// number of placeholders for Asn1Objects. However there
		/// are no actual Asn1Objects in this SequenceOf object.
		/// 
		/// </summary>
		/// <param name="size">Specifies the initial size of the collection.
		/// </param>
		public Asn1SequenceOf(int size):base(ID, size)
		{
			return ;
		}
		
		
		/// <summary> A copy constructor which creates an Asn1SequenceOf from an
		/// instance of Asn1Sequence.
		/// 
		/// Since SEQUENCE and SEQUENCE_OF have the same identifier, the decoder
		/// will always return a SEQUENCE object when it detects that identifier.
		/// In order to take advantage of the Asn1SequenceOf type, we need to be
		/// able to construct this object when knowingly receiving an
		/// Asn1Sequence.
		/// </summary>
		public Asn1SequenceOf(Asn1Sequence sequence):base(ID, sequence.toArray(), sequence.size())
		{
			return ;
		}
		
		
		/// <summary> Constructs an Asn1SequenceOf object by decoding data from an
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
		public Asn1SequenceOf(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(ID)
		{
			decodeStructured(dec, in_Renamed, len);
			return ;
		}
		
		
		/* Asn1SequenceOf specific methods
		*/
		
		/// <summary> Returns a String representation of this Asn1SequenceOf object</summary>
		[CLSCompliantAttribute(false)]
		public override System.String ToString()
		{
			return base.toString("SEQUENCE OF: { ");
		}
	}
}
