//
// XmlBinaryFormat.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
//

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
using System;

namespace System.Xml
{
	internal class XmlBinaryFormat
	{
		public const byte EndElement = 0x01;
		public const byte Comment = 0x02;
		public const byte Array = 0x03;
		public const byte AttrString = 0x04;
		public const byte AttrStringPrefix = 0x05;
		public const byte AttrIndex = 0x06;
		public const byte AttrIndexPrefix = 0x07;
		public const byte DefaultNSString = 0x08;
		public const byte PrefixNSString = 0x09;
		public const byte DefaultNSIndex = 0x0A;
		public const byte PrefixNSIndex = 0x0B;
		public const byte PrefixNAttrIndexStart = 0x0C;
		public const byte PrefixNAttrIndexEnd = 0x0C + 26 - 1;
		public const byte PrefixNAttrStringStart = 0x26;
		public const byte PrefixNAttrStringEnd = 0x26 + 26 - 1;
		public const byte ElemString = 0x40;
		public const byte ElemStringPrefix = 0x41;
		public const byte ElemIndex = 0x42;
		public const byte ElemIndexPrefix = 0x43;
		public const byte PrefixNElemIndexStart = 0x44;
		public const byte PrefixNElemIndexEnd = 0x44 + 26 - 1;
		public const byte PrefixNElemStringStart = 0x5E;
		public const byte PrefixNElemStringEnd = 0x5E + 26 - 1;

		public const byte Zero = 0x80;
		public const byte One = 0x82;
		public const byte BoolFalse = 0x84;
		public const byte BoolTrue = 0x86;
		public const byte Int8 = 0x88;
		public const byte Int16 = 0x8A;
		public const byte Int32 = 0x8C;
		public const byte Int64 = 0x8E;
		public const byte Single = 0x90;
		public const byte Double = 0x92;
		public const byte Decimal = 0x94;
		public const byte DateTime = 0x96;
		public const byte Chars8 = 0x98;
		public const byte Chars16 = 0x9A;
		public const byte Chars32 = 0x9C;
		public const byte Bytes8 = 0x9E;
		public const byte Bytes16 = 0xA0;
		public const byte Bytes32 = 0xA2;

		public const byte EmptyText = 0xA8;
		public const byte TextIndex = 0xAA;
		public const byte UniqueId = 0xAC;
		public const byte TimeSpan = 0xAE;
		public const byte Guid = 0xB0;
		public const byte UInt64 = 0xB2;
		public const byte Bool = 0xB4; // e.g. for typed array
		public const byte Utf16_8 = 0xB6;
		public const byte Utf16_16 = 0xB8;
		public const byte Utf16_32 = 0xBA;
		public const byte QNameIndex = 0xBC;
	}

	/* Binary Format (incomplete):

		Literal strings are represented as UTF-8 string, with a length
		prefixed to the string itself.

		Key indices are based on the rules below:
		- dictionary strings which can be found in IXmlDictionary are 
		  doubled its Key. e.g. if the string.Key is 4, then the
		  output is 8.
		- dictionary strings which cannot be found in IXmlDictionary
		  are stored in the XmlBinaryWriterSession, and its output
		  number is doubled + 1 e.g. if the string is the first
		  non-dictionary entry, then the output is 1, and 7 for the
		  fourth one.
		- When the index goes beyond 128, then it becomes 2 bytes,
		  where the first byte becomes 0x80 + idx % 0x80 and
		  the second byte becomes idx / 0x80.

		Below are operations. Prefixes are always raw strings.
		$string is length-prefixed string. @index is index as
		described above. [value] is length-prefixed raw array.

		// 2009-03-25: now that the binary format is open under OSP
		// [MC-NBFX], I have added some notes beyond current
		// implementation status (marked as TODO).

		01			: EndElement
		02 $value		: Comment
		03			: array
		04 $name		: local attribute by string
		05 $prefix $name	: global attribute by string
		06 @name		: local attribute by index
		07 $prefix @name	: global attribute by index
		08 $name		: default namespace by string
		09 $prefix $name	: prefixed namespace by string
		0A @name		: default namespace by index
		0B $prefix @name	: prefixed namespace by index
		0C @name		: global attribute by index,
		... 0x25		: in current element's namespace
		26 ... 0x3F		: attributes with prefix
		40 $name		: element w/o namespace by string
		41 $prefix $name	: element with namespace by string
		42 @name		: element w/o namespace by index
		43 $prefix @name	: element with namespace by index
		44 @name		: global element by index,
		... 0x5D		: in current element's namespace
		5E ... 0x77		: elements with prefix
		98 $value		: text/cdata/chars
		99 $value		: text/cdata/chars + EndElement

		FIXME: Below are not implemented:
		(Uri is simply 98, QName is 98 '{' ns '}' 98 name)

		Combined EndElement for below are supported:
		80 : 0 (integer)
		82 : 1 (integer)
		84 : false (bool)
		86 : true (bool)
		88 : 1-byte integer
		8A : 2-bytes integer
		8C : 4-bytes integer
		8E : 8-bytes integer
		90 : single
		92 : double
		94 : decimal
		96 : DateTime
		98 : chars8
		9A : chars16
		9C : chars32
		9E : bytes8 (base64)
		A0 : bytes16 (base64)
		A2 : bytes32 (base64)
		A4 : TODO: start of list
		A6 : TODO: end of list
		A8 : empty text
		AA : text index
		AC : UniqueId (IsGuid = true)
		AE : TimeSpan
		B0 : UUID
		B2 : UInt64
		B4 : bool text
		B6 : utf16_8
		B8 : utf16_16
		BA : utf16_32
		BC : QName index

		Error: PIs, doctype
		Ignored: XMLdecl
	*/
}
