//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
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
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace StandAloneRunnerSupport
{
	public sealed class TestRunner : MarshalByRefObject, IRegisteredObject, ITestRunner
	{
		public TestRunner ()
		{
			AppDomain ad = AppDomain.CurrentDomain;

			ad.SetData ("BEGIN_CODE_MARKER", Helpers.BEGIN_CODE_MARKER);
			ad.SetData ("END_CODE_MARKER", Helpers.END_CODE_MARKER);
		}
		
		public string Run (string url)
		{
			if (String.IsNullOrEmpty (url))
				throw new ArgumentNullException ("url");
			
			var output = new StringWriter ();
			try {
				string fullUrl = "http://localhost";
				if (url [0] == '/')
					fullUrl += url;
				else
					fullUrl += "/" + url;
				
				Uri uri = new Uri (fullUrl, UriKind.RelativeOrAbsolute);
				string query = uri.Query;
				if (!String.IsNullOrEmpty (query) && query [0] == '?')
					query = query.Substring (1);
				
				var wr = new TestWorkerRequest (uri.AbsolutePath, query, output);
				
				HttpRuntime.ProcessRequest (wr);
				return output.ToString ();
			} finally {
				output.Close ();
			}
		}
		
		public void Stop (bool immediate)
		{
			HostingEnvironment.UnregisterObject (this);
		}
	}
}
