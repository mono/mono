//
// System.Net.Mail.AlternateView.cs
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
	public class AlternateView : AttachmentBase
	{
		#region Fields
		Uri baseUri;
		LinkedResourceCollection linkedResources = new LinkedResourceCollection ();

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public AlternateView (string fileName) : base (fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
		}

		[MonoTODO]
		public AlternateView (string fileName, ContentType contentType) : base (fileName, contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
		}

		[MonoTODO]
		public AlternateView (string fileName, string mediaType) : base (fileName, mediaType)
		{
			if (fileName == null)
				throw new ArgumentNullException ();
		}

		[MonoTODO]
		public AlternateView (Stream contentStream) : base (contentStream)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
		}
		
		[MonoTODO]
		public AlternateView (Stream contentStream, string mediaType) : base (contentStream, mediaType)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
		}
		
		[MonoTODO]
		public AlternateView (Stream contentStream, ContentType contentType) : base (contentStream, contentType)
		{
			if (contentStream == null)
				throw new ArgumentNullException ();
		}

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		public Uri BaseUri {
			get { return baseUri; }
			set { baseUri = value; }
		}

		public LinkedResourceCollection LinkedResources {
			get { return linkedResources; }
		}
		
		#region Methods

		[MonoTODO]
		public static AlternateView CreateAlternateViewFromString (string content)
		{
			if (content == null)
				throw new ArgumentNullException ();
			MemoryStream ms = new MemoryStream (Encoding.UTF8.GetBytes (content));
			return new AlternateView (ms);
		}

		[MonoTODO]
		public static AlternateView CreateAlternateViewFromString (string content, ContentType contentType)
		{
			if (content == null)
				throw new ArgumentNullException ();
			MemoryStream ms = new MemoryStream (Encoding.UTF8.GetBytes (content));
			return new AlternateView (ms, contentType);
		}

		[MonoTODO]
		public static AlternateView CreateAlternateViewFromString (string content, Encoding encoding, string mediaType)
		{
			if (content == null)
				throw new ArgumentNullException ();
			MemoryStream ms = new MemoryStream (encoding.GetBytes (content));
			return new AlternateView (ms, mediaType);
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
		
		#endregion // Methods
	}
}

#endif // NET_2_0
