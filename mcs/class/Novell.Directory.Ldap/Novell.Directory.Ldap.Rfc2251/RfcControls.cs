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
// Novell.Directory.Ldap.Rfc2251.RfcControls.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents Ldap Contreols.
	/// 
	/// <pre>
	/// Controls ::= SEQUENCE OF Control
	/// </pre>
	/// </summary>
	public class RfcControls:Asn1SequenceOf
	{
		
		/// <summary> Controls context specific tag</summary>
		public const int CONTROLS = 0;
		
		//*************************************************************************
		// Constructors for Controls
		//*************************************************************************
		
		/// <summary> Constructs a Controls object. This constructor is used in combination
		/// with the add() method to construct a set of Controls to send to the
		/// server.
		/// </summary>
		public RfcControls():base(5)
		{
		}
		
		/// <summary> Constructs a Controls object by decoding it from an InputStream.</summary>
		[CLSCompliantAttribute(false)]
		public RfcControls(Asn1Decoder dec, System.IO.Stream in_Renamed, int len):base(dec, in_Renamed, len)
		{
			
			// Convert each SEQUENCE element to a Control
			for (int i = 0; i < size(); i++)
			{
				RfcControl tempControl = new RfcControl((Asn1Sequence) get_Renamed(i));
				set_Renamed(i, tempControl);
			}
		}
		
		//*************************************************************************
		// Mutators
		//*************************************************************************
		
		/// <summary> Override add() of Asn1SequenceOf to only accept a Control type.</summary>
		public void  add(RfcControl control)
		{
			base.add(control);
		}
		
		/// <summary> Override set() of Asn1SequenceOf to only accept a Control type.</summary>
		public void  set_Renamed(int index, RfcControl control)
		{
			base.set_Renamed(index, control);
		}
		
		//*************************************************************************
		// Accessors
		//*************************************************************************
		
		/// <summary> Override getIdentifier to return a context specific id.</summary>
		public override Asn1Identifier getIdentifier()
		{
			return new Asn1Identifier(Asn1Identifier.CONTEXT, true, CONTROLS);
		}
	}
}
