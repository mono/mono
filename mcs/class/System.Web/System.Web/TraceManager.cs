//
// System.Web.TraceManager
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Web.Configuration;

namespace System.Web {

	internal class TraceManager {
#if !NET_2_0
		static string traceConfigPath = "system.web/trace";
#endif
		bool enabled = false;
		bool local_only = true;
		bool page_output = false;
		TraceMode mode;
		int request_limit = 10;

		int cur_item;
		TraceData[] data;

		Exception initialException;
		
		public TraceManager ()
		{
			try {
#if NET_2_0
				mode = TraceMode.SortByTime;
				TraceSection config = WebConfigurationManager.GetWebApplicationSection ("system.web/trace") as TraceSection;
				if (config == null)
					config = new TraceSection ();
#else
				TraceConfig config = (TraceConfig) HttpContext.GetAppConfig (traceConfigPath);
#endif

				if (config == null)
					return;
			
				enabled = config.Enabled;
				local_only = config.LocalOnly;
				page_output = config.PageOutput;
#if NET_2_0
				if (config.TraceMode == TraceDisplayMode.SortByTime)
					mode = TraceMode.SortByTime;
				else
					mode = TraceMode.SortByCategory;
#else
				mode = config.TraceMode;
#endif
				request_limit = config.RequestLimit;
			} catch (Exception ex) {
				initialException = ex;
			}
		}

		public bool HasException {
			get { return initialException != null; }
		}

		public Exception InitialException {
			get { return initialException; }
		}
		
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		public bool LocalOnly {
			get { return local_only; }
			set { local_only = value; }
		}

		public bool PageOutput {
			get { return page_output; }
			set { page_output = value; }
		}

		public int RequestLimit {
			get { return request_limit; }
			set {
				if (request_limit == value)
					return;
				TraceData[] swap = new TraceData [value];
				Array.Copy (data, swap, (cur_item > value ? value : cur_item));
				if (cur_item > value)
					cur_item = value;
				request_limit = value;
			}
		}

		public TraceMode TraceMode {
			get { return mode; }
			set { mode = value; }
		}

		public TraceData[] TraceData {
			get { return data; }
		}
		
		public void AddTraceData (TraceData item)
		{
			if (data == null)
				data = new TraceData [request_limit];
			if (cur_item == request_limit)
				return;
			data [cur_item++] = item;
		}

		public void Clear ()
		{
			if (data == null)
				return;
			
			Array.Clear (data, 0, data.Length);
			cur_item = 0;
		}
		
		public int ItemCount {
			get { return cur_item; }
		}
	}
}

