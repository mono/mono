//
// System.Net.WebResponse
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

using System;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public abstract class WebResponse : MarshalByRefObject, ISerializable, IDisposable {
		// Constructors
		
		protected WebResponse () { }
		
		protected WebResponse (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		
		public virtual long ContentLength {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual string ContentType {		
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual WebHeaderCollection Headers {		
			get { throw new NotSupportedException (); }
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual bool IsFromCache
		{
			get {
				return false;
				// Better to return false than to kill the application
				// throw GetMustImplement ();
			}
		}
		
		[MonoTODO]
		public virtual bool IsMutuallyAuthenticated
		{
			get {
				throw GetMustImplement ();
			}
		}
		
		public virtual Uri ResponseUri {		
			get { throw new NotSupportedException (); }
		}		
#if NET_4_0
		[MonoTODO ("for portable library support")]
		public virtual bool SupportsHeaders {
			get { throw new NotImplementedException (); }
		}
#endif
		// Methods
		
		public virtual void Close()
		{
			throw new NotSupportedException ();
		}
		
		public virtual Stream GetResponseStream()
		{
			throw new NotSupportedException ();
		}
#if TARGET_JVM //enable overrides for extenders
		public virtual void Dispose()
#elif NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose()
#endif
		{
#if NET_4_0
			Dispose (true);
#else
			Close ();
#endif
		}
		
#if NET_4_0
		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Close ();
		}
#endif

		void ISerializable.GetObjectData
			(SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		protected virtual void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw GetMustImplement ();
		}
	}
}
