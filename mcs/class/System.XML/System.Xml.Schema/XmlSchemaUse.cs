// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
using System.Xml.Serialization;

#if !INCLUDE_MONO_XML_SCHEMA
namespace System.Xml.Schema
#else
namespace Mono.Xml.Schema
#endif
{
	/// <summary>
	/// Summary description for XmlSchemaUse.
	/// </summary>
#if !INCLUDE_MONO_XML_SCHEMA
	public
#else
	internal
#endif
	enum XmlSchemaUse 
	{
		[XmlIgnore]
		None = 0x00000000, 
		[XmlEnum("optional")]
		Optional = 0x00000001, 
		[XmlEnum("prohibited")]
		Prohibited = 0x00000002, 
		[XmlEnum("required")]
		Required = 0x00000003, 
	}
}
