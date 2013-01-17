//
// System.Net.Mail.LinkedResource.cs
//
// Author:
//	John Luke (john.luke@gmail.com)
//
// Copyright (C) John Luke, 2005
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
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	public class LinkedResource : AttachmentBase
	{
		#region Fields
		
		Uri contentLink;

		#endregion // Fields

		#region Constructors

		public LinkedResource (string fileName) : base (fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
		}
		
		public LinkedResource (string fileName, ContentType contentType) : base (fileName, contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
		}
		
		public LinkedResource (string fileName, string mediaType) : base (fileName, mediaType)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
		}

		public LinkedResource (Stream contentStream) : base (contentStream)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
		}
		
		public LinkedResource (Stream contentStream, ContentType contentType) : base (contentStream, contentType)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
		}
		
		public LinkedResource (Stream contentStream, string mediaType) : base (contentStream, mediaType)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
		}

		#endregion // Constructors

		#region Properties

		public Uri ContentLink {
			get { return contentLink; }
			set { contentLink = value; }
		}

		#endregion // Properties

		#region Methods

		public static LinkedResource CreateLinkedResourceFromString (string content)
		{
			if (content == null)
				throw new ArgumentNullException ();
			MemoryStream ms = new MemoryStream (Encoding.Default.GetBytes (content));
			LinkedResource lr = new LinkedResource (ms);
			lr.TransferEncoding = TransferEncoding.QuotedPrintable;
			return lr;
		}
		
		public static LinkedResource CreateLinkedResourceFromString (string content, ContentType contentType)
		{
			if (content == null)
				throw new ArgumentNullException ();
			MemoryStream ms = new MemoryStream (Encoding.Default.GetBytes (content));
			LinkedResource lr = new LinkedResource (ms, contentType);
			lr.TransferEncoding = TransferEncoding.QuotedPrintable;
			return lr;
		}
		
		public static LinkedResource CreateLinkedResourceFromString (string content, Encoding contentEncoding, string mediaType)
		{
			if (content == null)
				throw new ArgumentNullException ();
			MemoryStream ms = new MemoryStream (contentEncoding.GetBytes (content));
			LinkedResource lr = new LinkedResource (ms, mediaType);
			lr.TransferEncoding = TransferEncoding.QuotedPrintable;
			return lr;
		}

		#endregion // Methods
	}
}

