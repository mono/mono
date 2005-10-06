// 
// System.Web.HttpParseException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace System.Web {

#if NET_2_0
	[Serializable]
#endif
	public class HttpParseException : HttpException {

		int line;
		string virtualPath;
		
#if NET_2_0
		ParserErrorCollection errors = new ParserErrorCollection ();
#endif

		internal HttpParseException (string message, string virtualPath, int line)
			: base (message)
		{
			this.virtualPath = virtualPath;
			this.line = line;
		}

#if NET_2_0

		public HttpParseException (): this ("External component has thrown an exception")
		{
		}

		public HttpParseException (string message)
			: base (message)
		{
			errors.Add (new ParserError (message, null, 0));
		}
		
		public HttpParseException (string message, Exception innerException)
			: base (message, innerException)
		{
			errors.Add (new ParserError (message, null, 0));
		}

		public HttpParseException (string message, Exception innerException, string virtualPath, string sourceCode, int line)
			: base (message, innerException)
		{
			this.virtualPath = virtualPath;
			this.line = line;
			errors.Add (new ParserError (message, virtualPath, line));
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			base.GetObjectData (info, ctx);
			info.AddValue ("_virtualPath", virtualPath);
			info.AddValue ("_parserErrors", errors);
			info.AddValue ("_line", line);
		}
#endif

		[MonoTODO]
		public string FileName {
			get { return virtualPath; }
		}

		public int Line {
			get { return line; }
		}
		
#if NET_2_0
		public string VirtualPath {
			get { return virtualPath; }
		}
		
		public ParserErrorCollection ParserErrors {
			get { return errors; }
		}
#endif
	}
}

