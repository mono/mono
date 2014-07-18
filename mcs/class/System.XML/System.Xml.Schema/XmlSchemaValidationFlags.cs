//
// XmlSchemaValidationFlags.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
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
#if NET_2_0
using System;

#if !INCLUDE_MONO_XML_SCHEMA
namespace System.Xml.Schema
#else
namespace Mono.Xml.Schema
#endif
{
	[Flags]
#if !INCLUDE_MONO_XML_SCHEMA
	public
#else
	internal
#endif
	enum XmlSchemaValidationFlags
	{
		None = 0,
		ProcessInlineSchema = 1,
		ProcessSchemaLocation = 2,
		ReportValidationWarnings = 4,
		ProcessIdentityConstraints = 8,
		[Obsolete ("It is really idiotic idea to include such validation option that breaks W3C XML Schema specification compliance and interoperability.")]
		AllowXmlAttributes = 16,
	}
}
#endif
