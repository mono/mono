// 
// SoapHeaderHandling.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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


using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols
{
	public sealed class SoapHeaderHandling
	{
		// static members

		[MonoTODO]
		public static void EnsureHeadersUnderstood (SoapHeaderCollection headers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void GetHeaderMembers (
			SoapHeaderCollection headers,
			object target,
			SoapHeaderMapping [] mappings,
			SoapHeaderDirection direction,
			bool client)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetHeaderMembers (
			SoapHeaderCollection headers,
			object target,
			SoapHeaderMapping [] mappings,
			SoapHeaderDirection direction,
			bool client)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void WriteHeaders (
			XmlWriter writer,
			XmlSerializer serializer,
			SoapHeaderCollection headers,
			SoapHeaderMapping [] mappings,
			SoapHeaderDirection direction,
			bool isEncoded,
			string defaultNS,
			bool serviceDefaultIsEncoded,
			string envelopeNS)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void WriteUnknownHeaders (
			XmlWriter writer,
			SoapHeaderCollection headers,
			string envelopeNS)
		{
			throw new NotImplementedException ();
		}

		// instance members

		public SoapHeaderHandling ()
		{
		}

		[MonoTODO]
		public string ReadHeaders (
			XmlReader reader,
			XmlSerializer serializer,
			SoapHeaderCollection headers,
			SoapHeaderMapping [] mappings,
			SoapHeaderDirection direction,
			string envelopeNS,
			string encodingStyle,
			bool checkRequiredHeaders)
		{
			throw new NotImplementedException ();
		}
	}
}

