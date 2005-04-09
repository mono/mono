<%@ WebHandler Language="c#" class="XSPTest.SendBack" %>

using System;
using System.IO;
using System.Web;

namespace XSPTest
{
	public class SendBack : IHttpHandler
	{
		public void ProcessRequest (HttpContext context)
		{
			// Replies with the content of the TEST form variable.
			ProcessRequestTestVar (context);
			// Replies with all the contents in the input stream
			//ProcessRequestAll (context);
		}

		void ProcessRequestTestVar (HttpContext context)
		{
			string test = context.Request ["TEST"];
			context.Response.Write (test);
			Console.WriteLine ("Done writing a string of {0} characters.", (test != null) ? test.Length : 0);
		}

		void ProcessRequestAll (HttpContext context)
		{
			byte [] bytes = new byte [10240];
			Stream input = context.Request.InputStream;
			Stream output = context.Response.OutputStream;
			int nread;
			int total = 0;
			while ((nread = input.Read (bytes, 0, 10240)) > 0) {
				output.Write (bytes, 0, nread);
				total += nread;
			}
			Console.WriteLine ("Done writing {0} bytes.", total);
		}

		public bool IsReusable {
			get { return true; }
		}
	}
}

