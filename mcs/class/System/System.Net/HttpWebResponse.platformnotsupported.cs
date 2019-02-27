//
// System.Net.HttpWebResponse
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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
using System.Runtime.Serialization;

namespace System.Net
{
	[Serializable]
	public class HttpWebResponse : WebResponse, ISerializable, IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Net.HttpWebResponse is not supported on the current platform.";
		
		public HttpWebResponse() { } // Added for NS2.1, it's empty in CoreFX too

		[Obsolete ("Serialization is obsoleted for this type", false)]
		protected HttpWebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public string CharacterSet {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string ContentEncoding {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override long ContentLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string ContentType {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual CookieCollection Cookies {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override WebHeaderCollection Headers {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		[MonoTODO]
		public override bool IsMutuallyAuthenticated {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DateTime LastModified {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual string Method {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Version ProtocolVersion {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override Uri ResponseUri {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Server {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual HttpStatusCode StatusCode {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual string StatusDescription {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool SupportsHeaders {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string GetResponseHeader (string headerName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal void ReadAll ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Stream GetResponseStream ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		void ISerializable.GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		void IDisposable.Dispose ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void Dispose (bool disposing)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
