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
// Novell.Directory.Ldap.Rfc2251.RfcAttributeDescriptionList.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary>
	/// The AttributeDescriptionList is used to list attributes to be returned in
	/// a search request.
	/// 
	/// <pre>
	/// AttributeDescriptionList ::= SEQUENCE OF
	/// AttributeDescription
	/// </pre>
	/// 
	/// </summary>
	/// <seealso cref="RfcAttributeDescription">
	/// </seealso>
	/// <seealso cref="Asn1SequenceOf">
	/// </seealso>
	/// <seealso cref="RfcSearchRequest">
	/// </seealso>
	public class RfcAttributeDescriptionList:Asn1SequenceOf
	{
		/// <summary> </summary>
		public RfcAttributeDescriptionList(int size):base(size)
		{
			return ;
		}
		
		/// <summary> Convenience constructor. This constructor will construct an
		/// AttributeDescriptionList using the supplied array of Strings.
		/// </summary>
		public RfcAttributeDescriptionList(System.String[] attrs):base(attrs == null?0:attrs.Length)
		{
			
			if (attrs != null)
			{
				for (int i = 0; i < attrs.Length; i++)
				{
					add(new RfcAttributeDescription(attrs[i]));
				}
			}
			return ;
		}
		
		/*
		* Override add() to only accept types of AttributeDescription
		*
		* @exception Asn1InvalidTypeException
		*/
	}
}
