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
// Novell.Directory.Ldap.Asn1.Asn1Object.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> This is the base class for all other Asn1 types.</summary>
	[Serializable]
	public abstract class Asn1Object : System.Runtime.Serialization.ISerializable
	{
		
		private Asn1Identifier id;
		
		public Asn1Object(Asn1Identifier id)
		{
			this.id = id;
			return ;
		}
		
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}
		
		/// <summary> Abstract method that must be implemented by each child
		/// class to encode itself ( an Asn1Object) directly intto 
		/// a output stream.
		/// 
		/// </summary>
		/// <param name="out">The output stream onto which the encoded 
		/// Asn1Object will be placed.
		/// </param>
		abstract public void  encode(Asn1Encoder enc, System.IO.Stream out_Renamed);
		
		/// <summary> Returns the identifier for this Asn1Object as an Asn1Identifier. 
		/// This Asn1Identifier object will include the CLASS, FORM and TAG
		/// for this Asn1Object.
		/// </summary>
		public virtual Asn1Identifier getIdentifier()
		{
			return id;
		}
		
		/// <summary> Sets the identifier for this Asn1Object. This is helpful when 
		/// creating implicit Asn1Tagged types.
		/// 
		/// </summary>
		/// <param name="id">An Asn1Identifier object representing the CLASS, 
		/// FORM and TAG)
		/// </param>
		public virtual void  setIdentifier(Asn1Identifier id)
		{
			this.id = id;
			return ;
		}
		
		/// <summary> This method returns a byte array representing the encoded
		/// Asn1Object.  It in turn calls the encode method that is 
		/// defined in Asn1Object but will usually be implemented
		/// in the child Asn1 classses.
		/// </summary>
		[CLSCompliantAttribute(false)]
		public sbyte[] getEncoding(Asn1Encoder enc)
		{
			System.IO.MemoryStream out_Renamed = new System.IO.MemoryStream();
			try
			{
				encode(enc, out_Renamed);
			}
			catch (System.IO.IOException e)
			{
				// Should never happen - the current Asn1Object does not have
				// a encode method. 
				throw new System.SystemException("IOException while encoding to byte array: " + e.ToString());
			}
			return SupportClass.ToSByteArray(out_Renamed.ToArray());
		}
		
		/// <summary> Return a String representation of this Asn1Object.</summary>
		[CLSCompliantAttribute(false)]
		public override System.String ToString()
		{
			System.String[] classTypes = new System.String[]{"[UNIVERSAL ", "[APPLICATION ", "[CONTEXT ", "[PRIVATE "};
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			Asn1Identifier id = getIdentifier(); // could be overridden.
			
			sb.Append(classTypes[id.Asn1Class]).Append(id.Tag).Append("] ");
			
			return sb.ToString();
		}
	}
}
