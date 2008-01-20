//
// System.Web.HttpWorkerRequest partial
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
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

#if TARGET_JVM

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web
{
	public abstract partial class HttpWorkerRequest
	{
		java.io.Writer _outputWriter;

		public virtual void SendResponseFromMemory (IntPtr data, int length)
		{
			throw new NotImplementedException ("SendResponseFromMemory: unsafe buffers (IntPtr) are not supported");
		}

		internal void SendResponseFromMemory (string data, int offset, int length, Encoding encoding) {
			java.io.Writer writer = GetOutputWriter (encoding);
			if (writer == null) //if was redirected - silently returns null
				return;

			writer.write (data, offset, length);
		}

		internal void SendResponseFromMemory (char [] data, int offset, int length, Encoding encoding) {
			java.io.Writer writer = GetOutputWriter (encoding);
			if (writer == null) //if was redirected - silently returns null
				return;

			writer.write (data, offset, length);
		}

		java.io.Writer GetOutputWriter (Encoding encoding) {
			if (_outputWriter == null) {
				IServiceProvider prov = this as IServiceProvider;
				if (prov == null)
					; //throw ;

				javax.servlet.http.HttpServletResponse sr = (javax.servlet.http.HttpServletResponse) prov.GetService (typeof (javax.servlet.http.HttpServletResponse));
				if (sr != null)
					sr.setCharacterEncoding (encoding.HeaderName);

				return (java.io.Writer) prov.GetService (typeof (java.io.Writer));
			}
			return _outputWriter;
		}

	}
}


#endif
