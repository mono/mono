//
// Pkcs9DocumentName.cs - System.Security.Cryptography.Pkcs.Pkcs9DocumentName
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
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
using System.Text;

namespace System.Security.Cryptography.Pkcs {

	public sealed class Pkcs9DocumentName : Pkcs9Attribute	{

		private const string oid = "1.3.6.1.4.1.311.88.2.1";

		private string _name;

		[MonoTODO ("encode for RawData using Mono.Security")]
		public Pkcs9DocumentName () 
			: base (new Oid (oid), null)
		{
		}

		[MonoTODO ("encode for RawData using Mono.Security")]
		public Pkcs9DocumentName (string documentName) 
			: base (new Oid (oid), Encoding.Unicode.GetBytes (documentName))
		{
			_name = documentName;
		}

		[MonoTODO ("decode using Mono.Security")]
		public Pkcs9DocumentName (byte[] encodedDocumentName)
			: base (new Oid (oid), encodedDocumentName)
		{
		}

		public string DocumentName {
			get { return _name; }
		}
	}
}

#endif
