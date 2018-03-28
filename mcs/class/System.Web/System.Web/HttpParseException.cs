// 
// System.Web.HttpParseException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

namespace System.Web
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[Serializable]
	public sealed class HttpParseException : HttpException
	{
		int line;
		string virtualPath;
		ParserErrorCollection errors = new ParserErrorCollection ();

		internal HttpParseException (string message, string virtualPath, int line)
			: base (message)
		{
			this.virtualPath = virtualPath;
			this.line = line;
		}
		
		HttpParseException (SerializationInfo info, StreamingContext context)
			: base (info, context)
                {
			line = info.GetInt32 ("_line");
			virtualPath = info.GetString ("_virtualPath");
			errors = info.GetValue ("_parserErrors", typeof (ParserErrorCollection)) as ParserErrorCollection;
                }
		
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

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("_virtualPath", virtualPath);
			info.AddValue ("_parserErrors", errors);
			info.AddValue ("_line", line);
		}

		public string FileName {
			get { return virtualPath; }
		}

		public int Line {
			get { return line; }
		}
		
		public string VirtualPath {
			get { return virtualPath; }
		}
		
		public ParserErrorCollection ParserErrors {
			get { return errors; }
		}
	}
}

