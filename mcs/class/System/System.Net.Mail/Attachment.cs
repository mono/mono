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
	public class Attachment : AttachmentBase
	{
		#region Fields

		ContentDisposition contentDisposition = new ContentDisposition ();
		Encoding nameEncoding;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public Attachment (string fileName) : base (fileName)
		{
		}

		[MonoTODO]
		public Attachment (string fileName, ContentType contentType) : base (fileName, contentType)
		{
		}

		[MonoTODO]
		public Attachment (Stream contentStream, ContentType contentType) : base (contentStream, contentType)
		{
		}

		[MonoTODO]
		public Attachment (Stream contentStream, string mediaType) : base (contentStream, mediaType)
		{
		}

		[MonoTODO]
		public Attachment (Stream contentStream, string name, string mediaType) : base (contentStream, mediaType)
		{
			Name = name;
		}



		#endregion // Constructors

		#region Properties

		public ContentDisposition ContentDisposition {
			get { return contentDisposition; }
		}

		public string Name {
			get { return ContentType.Name; }
			set { 
				if (value == null)
					throw new ArgumentNullException ();
				if (value.Equals (""))
					throw new ArgumentException ();
				ContentType.Name = value;
			}
		}

		[MonoTODO]
		public Encoding NameEncoding {
			get { return nameEncoding; }
		}

		#endregion // Properties

		#region Methods
		
		[MonoTODO]
		public static Attachment CreateAttachmentFromString (string content, ContentType contentType)
		{
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (content);
			sw.Flush ();
			ms.Position = 0;
			return new Attachment (ms, contentType);
		}
		
		[MonoTODO]
		public static Attachment CreateAttachmentFromString (string content, string name)
		{
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (content);
			sw.Flush ();
			ms.Position = 0;
			return new Attachment (ms, name);
		}
		
		[MonoTODO]
		public static Attachment CreateAttachmentFromString (string content, string name, Encoding encoding, string mediaType)
		{
			MemoryStream ms = new MemoryStream ();
			StreamWriter sw = new StreamWriter (ms);
			sw.Write (content);
			sw.Flush ();
			ms.Position = 0;
			return new Attachment (ms, name, mediaType);
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
