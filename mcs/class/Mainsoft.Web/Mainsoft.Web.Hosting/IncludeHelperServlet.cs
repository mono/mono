//
// Mainsoft.Web.Hosting.IncludeHelperServlet
//
// Authors:
//	Eyal Alaluf (eyala@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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
using System.Web.UI;
using java.io;
using javax.servlet;
using javax.servlet.http;

namespace Mainsoft.Web.Hosting
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class IncludeHelperServlet : HttpServlet
	{
		public IncludeHelperServlet()
		{
		}

		override protected void service(HttpServletRequest req, HttpServletResponse resp)
		{
			string servletPath = ServletIncludeUtils.getServletPath(req);
			TextWriter writer = (TextWriter)ServletIncludeUtils.getTextWriter(req);
			RequestDispatcher dispatcher = getServletContext().getRequestDispatcher(servletPath);
			HttpServletResponseWrapper wrapper = new AspxResponseWrapper(resp, writer);
			dispatcher.include(req, wrapper);
		}

		sealed class AspxResponseWrapper : HttpServletResponseWrapper 
		{
			public AspxResponseWrapper(HttpServletResponse resp, TextWriter writer) : base (resp)
			{
				jwriter = new PrintWriter(new JavaWriterFromTextWriter(writer));
			}

			public override PrintWriter getWriter()
			{
				return jwriter;
			}

			readonly PrintWriter jwriter;
		}

		sealed class JavaWriterFromTextWriter : Writer
		{
			public JavaWriterFromTextWriter(TextWriter writer)
			{
				this.writer = writer;
			}

			public override void flush()
			{
				writer.Flush ();
			}

			public override void write(char[] buffer, int offset, int count)
			{
				writer.Write(buffer, offset, count);
			}

			public override void write(String s)
			{
				writer.Write(s);
			}

			public override void write(string s, int start, int count)
			{
				if (start == 0 && count == s.Length)
					writer.Write(s);
				else
					writer.Write(s.Substring(start, count));
			}

			public override void close()
			{
				// Ignore close as we don't own here the writer.
			}

			readonly TextWriter writer;
		}
	}
}

namespace System.Web.GH
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class IncludeHelperServlet : Mainsoft.Web.Hosting.IncludeHelperServlet
	{
	}

}
