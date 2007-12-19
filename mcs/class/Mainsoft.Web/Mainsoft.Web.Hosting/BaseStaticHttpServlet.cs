//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Configuration;
using System.Web;
using System.Threading;

using javax.servlet;
using javax.servlet.http;
using vmw.common;
using System.Diagnostics;

namespace Mainsoft.Web.Hosting
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class BaseStaticHttpServlet : HttpServlet
	{
		public BaseStaticHttpServlet()
		{
		}

		override public void init(ServletConfig config)
		{
			base.init(config);
			AppDir = config.getServletContext ().getInitParameter (IAppDomainConfig.APP_DIR_NAME);
			if (AppDir != null) {
				AppDir = AppDir.Replace('\\', '/');
				if (AppDir[AppDir.Length - 1] != '/')
					AppDir += '/';
			}
		}

		override protected void service(HttpServletRequest req, HttpServletResponse resp)
		{
			resp.setHeader("X-Powered-By", "ASP.NET");
			resp.setHeader("X-AspNet-Version", "1.1.4322");

			String filename = getServletContext().getRealPath(req.getServletPath());
			ServletOutputStream hos;
			try {
				hos = resp.getOutputStream();
			}
			catch (java.lang.IllegalStateException e)
			{
				string mimeType = getServletContext().getMimeType(filename);
				if (mimeType == null || mimeType.StartsWith("text")) {
					sendFileUsingWriter(resp, filename);
					return;
				}
				else
					throw e;
			}
			try 
			{
				string mimeType = this.getServletContext().getMimeType(filename);
				if (mimeType == null)
					mimeType = "text/plain";
				
				resp.setContentType(mimeType);

				FileStream fis = null;
				try {
					fis = new FileStream(filename,FileMode.Open,FileAccess.Read);
					byte[] buf = new byte[4 * 1024];  // 4K buffer
					int bytesRead;
					while ((bytesRead = fis.Read(buf,0,buf.Length)) != -1 &&
						   bytesRead != 0) {
						hos.write(TypeUtils.ToSByteArray(buf), 0, bytesRead);
					}
				}
				finally {
					if (fis != null) fis.Close();
				}
			}
			catch (System.IO.FileNotFoundException e) 
			{
				resp.setStatus(404,"Object Not Found.");
				HttpException myExp = new HttpException (404, "File '" + filename + "' not found.");
				hos.print(((HttpException) myExp).GetHtmlErrorMessage ());
				hos.flush();
			}
			catch(Exception e) 
			{
				Trace.WriteLine (String.Format ("ERROR in Static File Reading {0},{1}", e.GetType (), e.Message));
				resp.setStatus(500);
				HttpException myExp = new HttpException ("Exception in Reading static file", e);
				hos.print(((HttpException) myExp).GetHtmlErrorMessage ());
				hos.flush();
			}
		}

		void sendFileUsingWriter(HttpServletResponse resp, string filename)
		{
			java.io.PrintWriter writer = resp.getWriter();
			try 
			{
				resp.setContentType(this.getServletContext().getMimeType(filename));
				StreamReader fis = null;
				char[] buf = new char[4 * 1024];  // 4K buffer
				try {
					fis = new StreamReader(filename);
					int charsRead;
					while ((charsRead = fis.Read(buf,0,buf.Length)) != -1 &&
					   	charsRead != 0) {
						writer.write(buf, 0, charsRead);
					}
				}
				finally {
					if (fis != null) fis.Close();
				}
			}
			catch (System.IO.FileNotFoundException e) 
			{
				resp.setStatus(404,"Object Not Found.");
				HttpException myExp = new HttpException (404, "File '" + filename + "' not found.");
				writer.print(((HttpException) myExp).GetHtmlErrorMessage ());
				writer.flush();
			}
			catch(Exception e) 
			{
				Trace.WriteLine (String.Format ("ERROR in Static File Reading {0},{1}", e.GetType (), e.Message));
				resp.setStatus(500);
				HttpException myExp = new HttpException ("Exception in Reading static file", e);
				writer.print(((HttpException) myExp).GetHtmlErrorMessage ());
				writer.flush();
			}
		}

		override public void destroy()
		{
			base.destroy();
		}

		private string AppDir;
	}
}

namespace System.Web.J2EE
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class BaseStaticHttpServlet : Mainsoft.Web.Hosting.BaseStaticHttpServlet
	{
	}

}


namespace System.Web.GH
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class BaseStaticHttpServlet : Mainsoft.Web.Hosting.BaseStaticHttpServlet
	{
	}

}

