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

using System.IO;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail {
	public class Attachment : AttachmentBase
	{
		#region Fields

		ContentDisposition contentDisposition = new ContentDisposition ();
		Encoding nameEncoding;

		#endregion // Fields

		#region Constructors

		public Attachment (string fileName)
			: base (fileName) {
			InitName (fileName);
		}

		public Attachment (string fileName, string mediaType)
			: base (fileName, mediaType) {
			InitName (fileName);
		}

		public Attachment (string fileName, ContentType contentType)
			: base (fileName, contentType) {
			InitName (fileName);
		}

		public Attachment (Stream contentStream, ContentType contentType)
			: base (contentStream, contentType) {
		}

		public Attachment (Stream contentStream, string name)
			: base (contentStream) {
			Name = name;
		}

		public Attachment (Stream contentStream, string name, string mediaType)
			: base (contentStream, mediaType) {
			Name = name;
		}



		#endregion // Constructors

		#region Properties

		public ContentDisposition ContentDisposition {
			get { return contentDisposition; }
		}

		public string Name {
			get { return ContentType.Name; }
			set { ContentType.Name = value; }
		}

		public Encoding NameEncoding {
			get { return nameEncoding; }
			set { nameEncoding = value; }
		}

		#endregion // Properties

		#region Methods
		
		public static Attachment CreateAttachmentFromString (string content, ContentType contentType)
		{
			if (content == null)
				throw new ArgumentNullException ("content");
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (content);
			sw.Flush ();
			ms.Position = 0;
			Attachment a = new Attachment (ms, contentType);
			a.TransferEncoding = TransferEncoding.QuotedPrintable;
			return a;
		}
		
		public static Attachment CreateAttachmentFromString (string content, string name)
		{
			if (content == null)
				throw new ArgumentNullException ("content");
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (content);
			sw.Flush ();
			ms.Position = 0;
			Attachment a = new Attachment (ms, new ContentType ("text/plain"));
			a.TransferEncoding = TransferEncoding.QuotedPrintable;
			a.Name = name;
			return a;
		}
		
		public static Attachment CreateAttachmentFromString (string content, string name, Encoding contentEncoding, string mediaType)
		{
			if (content == null)
				throw new ArgumentNullException ("content");
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms, contentEncoding);
			sw.Write (content);
			sw.Flush ();
			ms.Position = 0;
			Attachment a = new Attachment (ms, name, mediaType);
			a.TransferEncoding = ContentType.GuessTransferEncoding (contentEncoding);
			a.ContentType.CharSet = sw.Encoding.BodyName;
			return a;
		}

		#endregion // Methods

		private void InitName (string fileName) {
			if (fileName == null) {
				throw new ArgumentNullException ("fileName");
			}

			Name = Path.GetFileName (fileName);
		}

	}
}

