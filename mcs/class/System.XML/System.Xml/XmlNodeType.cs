// XmlNodeType.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:31:46 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


namespace System.Xml {


	/// <summary>
	/// </summary>
	public enum XmlNodeType {

		/// <summary>
		/// </summary>
		None = 0,

		/// <summary>
		/// </summary>
		Element = 1,

		/// <summary>
		/// </summary>
		Attribute = 2,

		/// <summary>
		/// </summary>
		Text = 3,

		/// <summary>
		/// </summary>
		CDATA = 4,

		/// <summary>
		/// </summary>
		EntityReference = 5,

		/// <summary>
		/// </summary>
		Entity = 6,

		/// <summary>
		/// </summary>
		ProcessingInstruction = 7,

		/// <summary>
		/// </summary>
		Comment = 8,

		/// <summary>
		/// </summary>
		Document = 9,

		/// <summary>
		/// </summary>
		DocumentType = 10,

		/// <summary>
		/// </summary>
		DocumentFragment = 11,

		/// <summary>
		/// </summary>
		Notation = 12,

		/// <summary>
		/// </summary>
		Whitespace = 13,

		/// <summary>
		/// </summary>
		SignificantWhitespace = 14,

		/// <summary>
		/// </summary>
		EndElement = 15,

		/// <summary>
		/// </summary>
		EndEntity = 16,

		/// <summary>
		/// </summary>
		XmlDeclaration = 17,
	} // XmlNodeType

} // System.Xml
