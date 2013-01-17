//
// System.Net.Mime.MediaTypeNames.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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

namespace System.Net.Mime {
	public static class MediaTypeNames
	{
		public static class Application {
			const string prefix		= "application/";
			public const string Octet	= prefix + "octet-stream";
			public const string Pdf		= prefix + "pdf";
			public const string Rtf		= prefix + "rtf";
			public const string Soap	= prefix + "soap+xml";
			public const string Zip		= prefix + "zip";
		}
		public static class Image {
			const string prefix		= "image/";
			public const string Gif		= prefix + "gif";
			public const string Jpeg	= prefix + "jpeg";
			public const string Tiff	= prefix + "tiff";
		}
		public static class Text {
			const string prefix		= "text/";
			public const string Html	= prefix + "html";
			public const string Plain	= prefix + "plain";
			public const string RichText	= prefix + "richtext";
			public const string Xml		= prefix + "xml";
		}
	}
}
