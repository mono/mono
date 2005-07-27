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
// Novell.Directory.Ldap.Asn1.Asn1Choice.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Asn1
{
	
	/// <summary> The Asn1Choice object represents the choice of any Asn1Object. All
	/// Asn1Object methods are delegated to the object this Asn1Choice contains.
	/// </summary>
	/* Can a CHOICE contain anything BUT a TAGGED Type?
	*/
	public class Asn1Choice:Asn1Object
	{
		/// <summary> Sets the CHOICE value stored in this Asn1Choice.
		/// 
		/// </summary>
		/// <param name="content">The Asn1Object that this Asn1Choice will
		/// encode.  Since all Asn1 objects are derived from Asn1Object
		/// any basic type can be passed in.    
		/// </param>
		[CLSCompliantAttribute(false)]
		virtual protected internal Asn1Object ChoiceValue
		{
			set
			{
				this.content = value;
				return ;
			}
			
		}
		
		private Asn1Object content;
		
		/* Constructors for Asn1Choice
		*/
		
		/// <summary> Constructs an Asn1Choice object using an Asn1Object value.
		/// 
		/// </summary>
		/// <param name="content">The Asn1Object that this Asn1Choice will
		/// encode.  Since all Asn1 objects are derived from Asn1Object
		/// any basic type can be passed in.
		/// </param>
		public Asn1Choice(Asn1Object content):base(null)
		{
			this.content = content;
			return ;
		}
		
		/// <summary> No arg Constructor. This is used by Filter, who subsequently sets the
		/// content after parsing the RFC 2254 Search Filter String.
		/// </summary>
		protected internal Asn1Choice():base(null)
		{
			this.content = null;
			return ;
		}
		
		/* Asn1Object implementation
		*/
		
		
		/// <summary> Call this method to encode the contents of this Asn1Choice
		/// instance into the specified output stream using the 
		/// specified encoder object.
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
			content.encode(enc, out_Renamed);
			return ;
		}
		
		/* Asn1Choice specific methods
		*/
		
		/// <summary> Returns the CHOICE value stored in this Asn1Choice
		/// as an Asn1Object. 
		/// </summary>
		public Asn1Object choiceValue()
		{
			return content;
		}
		
		/// <summary> This method will return the Asn1Identifier of the 
		/// encoded Asn1Object.We  override the parent method
		/// as the identifier of an Asn1Choice depends on the 
		/// type of the object encoded by this Asn1Choice.
		/// </summary>
		public override Asn1Identifier getIdentifier()
		{
			return content.getIdentifier();
		}
		
		/// <summary> Sets the identifier of the contained Asn1Object. We
		/// override the parent method as the identifier of 
		/// an Asn1Choice depends on the type of the object 
		/// encoded by this Asn1Choice.
		/// </summary>
		public override void  setIdentifier(Asn1Identifier id)
		{
			content.setIdentifier(id);
			return ;
		}
		
		/// <summary> Return a String representation of this Asn1Object.</summary>
		public override System.String ToString()
		{
			return content.ToString();
		}
	}
}
