//
// System.Net.WebException.cs
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

using System.Runtime.Serialization;

namespace System.Net 
{
#if MOONLIGHT && INSIDE_SYSTEM
	internal class WebException : InvalidOperationException, ISerializable {
#else
	[Serializable]
	public class WebException : InvalidOperationException, ISerializable {
#endif
		private WebResponse response;
		private WebExceptionStatus status = WebExceptionStatus.UnknownError;

		// Constructors
		
		public WebException () : base ()
		{
		}
		
		public WebException (string message) : base (message)
		{
		}

		protected WebException (SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base (serializationInfo, streamingContext)
		{
		}

		public WebException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public WebException (string message, WebExceptionStatus status)
			: base (message)
		{
			this.status = status;
		}
		
		internal WebException (string message, Exception innerException, WebExceptionStatus status)
			: base (message, innerException)
		{
			this.status = status;
		}

		public WebException(string message, 
				    Exception innerException,
		   		    WebExceptionStatus status, 
		   		    WebResponse response)
			: base (message, innerException)
		{
			this.status = status;
			this.response = response;
		}
		
		// Properties
		
		public WebResponse Response {
			get { return this.response; }
		}
		
		public WebExceptionStatus Status {
			get { return this.status; }
		}
		
		// Methods
#if !TARGET_JVM
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}
#endif	

		public override void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			base.GetObjectData (serializationInfo,
					    streamingContext);
		}
	}
}
