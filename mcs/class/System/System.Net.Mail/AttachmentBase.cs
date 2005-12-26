//
// System.Net.Mail.AttachmentBase.cs
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

#if NET_2_0

using System.IO;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	public abstract class AttachmentBase : IDisposable
	{
		#region Fields

		string id;
		ContentType contentType = new ContentType ();
		Stream contentStream;
		TransferEncoding transferEncoding;

		#endregion // Fields

		#region Constructors

		protected AttachmentBase (Stream contentStream)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
			this.contentStream = contentStream;
			contentType.MediaType = MediaTypeNames.Application.Octet;
			contentType.CharSet = null;
			transferEncoding = TransferEncoding.Base64;
		}

		protected AttachmentBase (Stream contentStream, ContentType contentType)
		{
			if (contentStream == null || contentType == null)
				throw new ArgumentNullException ();
			this.contentStream = contentStream;
			this.contentType = contentType;
			transferEncoding = TransferEncoding.Base64;
		}

		protected AttachmentBase (Stream contentStream, string mediaType)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
			this.contentStream = contentStream;
			contentType.MediaType = mediaType;
			transferEncoding = TransferEncoding.Base64;
		}

		protected AttachmentBase (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
			contentStream = File.OpenRead (fileName);
			contentType.MediaType = MediaTypeNames.Application.Octet;
			contentType.CharSet = null;
			transferEncoding = TransferEncoding.Base64;
		}

		protected AttachmentBase (string fileName, ContentType contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
			contentStream = File.OpenRead (fileName);
			this.contentType = contentType;
			transferEncoding = TransferEncoding.Base64;
		}

		protected AttachmentBase (string fileName, string mediaType)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
			contentStream = File.OpenRead (fileName);
			contentType.MediaType = mediaType;
			transferEncoding = TransferEncoding.Base64;
		}

		#endregion // Constructors

		#region Properties

		public string ContentId {
			get { return id; }
			set {
				if (value == null)
					throw new ArgumentNullException ();
				id = value;
			}
		}

		public Stream ContentStream {
			get { return contentStream; }
		}

		public ContentType ContentType {
			get { return contentType; }
			set { contentType = value; }
		}

		public TransferEncoding TransferEncoding {
			get { return transferEncoding; }
			set { transferEncoding = value; }
		}

		#endregion // Properties

		#region Methods

		public void Dispose ()
		{
			Dispose (true);
			//GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
