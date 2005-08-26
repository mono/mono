//
// System.Web.Hosting.SimpleWorkerRequest.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Hosting {

	[ComVisible (false)]
	public class SimpleWorkerRequest : HttpWorkerRequest {
		string page;
		string query;
		string app_virtual_dir;
		string app_physical_dir;
		TextWriter output;

		bool hosted;
			
		// computed
		string raw_url;
		
		//
		// Constructor used when the target application domain
		// was created with ApplicationHost.CreateApplicationHost
		//
		public SimpleWorkerRequest (string page, string query, TextWriter output)
		{
			this.page = page;
			this.query = query;
			this.output = output;

			app_virtual_dir = HttpRuntime.AppDomainAppVirtualPath;
			app_physical_dir = HttpRuntime.AppDomainAppPath;
			hosted = true;
		}

		//
		// Creates a SimpleWorkerRequest that can be used from any AppDomain
		//
		// This is used for user instantiates HttpContext (my_SimpleWorkerRequest)
		//
		public SimpleWorkerRequest (string appVirtualDir, string appPhysicalDir, string page, string query, TextWriter output)
		{
			this.page = page;
			this.query = query;
			this.output = output;
			app_virtual_dir = appVirtualDir;
			app_physical_dir = appPhysicalDir;
		}
		
		
		public override string MachineConfigPath {
			get {
				if (hosted)
					return ICalls.GetMachineConfigPath ();
				return null;
			}
		}

		public override string MachineInstallDirectory {
			get {
				if (hosted)
					return ICalls.GetMachineInstallDirectory ();
				return null;
			}
		}

		public override void EndOfRequest ()
		{
		}
		
		public override void FlushResponse (bool finalFlush)
		{
		}
		
		public override string GetAppPath ()
		{
			return app_virtual_dir;
		}

		public override string GetAppPathTranslated ()
		{
			return app_physical_dir;
		}

		public override string GetFilePath ()
		{
			return Path.Combine (app_virtual_dir, page);
		}

		public override string GetFilePathTranslated ()
		{
			string local_page;
			
			if (Path.DirectorySeparatorChar == '\\')
				local_page = page.Replace ('/', '\\');
			else
				local_page = page;
			
			return Path.Combine (app_physical_dir, local_page);
		}

		public override string GetHttpVerbName ()
		{
			return "GET";
		}
		
		public override string GetHttpVersion ()
		{
			return "HTTP/1.0";
		}
		
		public override string GetLocalAddress ()
		{
			return "127.0.0.1";
		}

		public override int GetLocalPort ()
		{
			return 80;
		}

		public override string GetPathInfo ()
		{
			return "";
		}

		public override string GetQueryString ()
		{
			return query;
		}
		
		public override string GetRawUrl ()
		{
			if (raw_url == null){
				string q = ((query == null || query == "") ? "" : "?" + query);
				
				raw_url = Path.Combine (app_virtual_dir, page) + q;
			}
			return raw_url;
		}
		
		public override string GetRemoteAddress ()
		{
			return "127.0.0.1";
		}
		
		public override int GetRemotePort ()
		{
			return 0;
		}
		
		public override string GetServerVariable (string name)
		{
			return "";
		}

		public override string GetUriPath ()
		{
			return app_virtual_dir + "/" + page;
		}

		public override IntPtr GetUserToken ()
		{
			return IntPtr.Zero;
		}

		public override string MapPath (string path)
		{
			if (!hosted)
				return null;

			if (!path.StartsWith (app_virtual_dir))
				throw new ArgumentNullException ("path is not rooted in the virtual directory");

			string rest = path.Substring (app_virtual_dir.Length);
			if (rest [0] == '/')
				rest = rest.Substring (1);
			return Path.Combine (app_physical_dir, rest);
		}

		public override void SendKnownResponseHeader (int index, string value)
		{
		}

		public override void SendResponseFromFile (IntPtr handle, long offset, long length)
		{
		}

		public override void SendResponseFromFile (string filename, long offset, long length)
		{
		}

		public override void SendResponseFromMemory (byte [] data, int length)
		{
		}


		public override void SendStatus (int statusCode, string statusDescription)
		{
		}

		public override void SendUnknownResponseHeader (string name, string value)
		{
		}
	}
}
