//
// System.Net.Mail.Attachment.cs
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

#if NET_2_0

using System.IO;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	public class Attachment : IDisposable
	{
		#region Fields

		ContentType contentType = new ContentType ();
		Encoding encoding;

		ContentDisposition contentDisposition;
		Stream contentStream;
		string contentString;
		string name;
		TransferEncoding transferEncoding;

		#endregion // Fields

		#region Constructors

		public Attachment ()
		{
			contentDisposition = new ContentDisposition ("attachment"); 
		}

		public Attachment (string contentString)
		{
			SetContent (contentString);
		}

		public Attachment (Stream contentStream, ContentType contentType)
		{
			SetContent (contentStream);
			this.contentType = contentType;
		}

		public Attachment (Stream contentStream, string name)
		{
			SetContent (contentStream, name);
		}

		public Attachment (string contentString, string mediaType)
		{
			SetContent (contentString, mediaType);
		}

		public Attachment (Stream contentStream, string name, string mediaType)
		{
			SetContent (contentStream, name, mediaType);
		}

		public Attachment (string contentString, string mediaType, Encoding encoding)
		{
			SetContent (contentString, mediaType, encoding);
		}


		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public ContentDisposition ContentDisposition {
			get { return contentDisposition; }
		}

		public Stream ContentStream {
			get { return contentStream; }
		}

		public string ContentString {
			get { return contentString; }
		}

		public Encoding ContentStringEncoding {
			get { return encoding; }
		}

		public ContentType ContentType {
			get { return contentType; }
		}

		[MonoTODO]
		public string FileName {
			get { throw new NotImplementedException (); }
		}

		public string Name {
			get { return contentType.Name; }
			set { 
				if (value == null)
					throw new ArgumentNullException ();
				if (value.Equals (""))
					throw new ArgumentException ();
				contentType.Name = value;
			}
		}

		public TransferEncoding TransferEncoding {
			get { return transferEncoding; }
			set { transferEncoding = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Dispose ()
		{
		}

		public void SetContent (Stream contentStream)
		{
			this.contentStream = contentStream;
			contentType.MediaType = MediaTypeNames.Application.Octet;
			contentType.CharSet = null;
			transferEncoding = TransferEncoding.Base64;
		}

		public void SetContent (string contentString)
		{
			SetContent (contentString, MediaTypeNames.Text.Plain, Encoding.Default);
		}

		public void SetContent (Stream contentStream, string name)
		{
			SetContent (contentStream);
			contentType.Name = name;
		}

		public void SetContent (string contentString, string mediaType)
		{
			SetContent (contentString, mediaType, Encoding.Default);
		}

		public void SetContent (Stream contentStream, string name, string mediaType)
		{
			SetContent (contentStream, name);
			contentType.MediaType = mediaType;
		}

		public void SetContent (string contentString, string mediaType, Encoding encoding)
		{
			contentType.MediaType = mediaType;
			contentType.CharSet = encoding.WebName;

			transferEncoding = TransferEncoding.QuotedPrintable;

			contentStream = new MemoryStream ();
			StreamWriter sw = new StreamWriter (contentStream);
			sw.Write (contentString);
			sw.Flush ();

			contentStream.Position = 0;
		}

		public void SetContent (string contentString, string mediaType, Encoding encoding, TransferEncoding transferEncoding)
		{
		}

		public void SetContentFromFile (string fileName)
		{
			SetContentFromFile (fileName, null, null);
		}
		
		public void SetContentFromFile (string fileName, string name)
		{
			SetContentFromFile (fileName, name, null);
		}

		public void SetContentFromFile (string fileName, string name, string mediaType)
		{
			if (name == null) {
				char[] match = new char [2] {';','\\'};
				if (fileName.IndexOfAny (match) > 0)
					name = fileName.Substring (fileName.LastIndexOfAny (match));
			}

			Stream s = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

			SetContent (s, name, mediaType);
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
