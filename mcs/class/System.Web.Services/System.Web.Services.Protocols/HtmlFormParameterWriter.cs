// 
// System.Web.Services.Protocols.HtmlFormParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.IO;
using System.Net;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class HtmlFormParameterWriter : UrlEncodedParameterWriter {

		#region Constructors

		public HtmlFormParameterWriter () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override bool UsesWriteRequest {
			get { return true; }
		}

		#endregion // Properties

		#region Methods

		public override void InitializeRequest (WebRequest request, object[] values)
		{
			if (RequestEncoding == null) request.ContentType = "application/x-www-form-urlencoded";
			else request.ContentType = "application/x-www-form-urlencoded; charset=" + RequestEncoding.BodyName;
		}

		public override void WriteRequest (Stream requestStream, object[] values)
		{
			StreamWriter sw = new StreamWriter (requestStream);
			Encode (sw, values);
			sw.Flush ();
		}

		#endregion // Methods
	}
}
