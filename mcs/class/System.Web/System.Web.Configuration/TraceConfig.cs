//
// System.Web.Configuration.TraceConfig
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Web;

namespace System.Web.Configuration {

	internal class TraceConfig {

		private bool enabled;
		private bool local_only;
		private bool page_output;
		private int request_limit;
		private TraceMode trace_mode;

		public TraceConfig ()
		{
			request_limit = 10;
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
			set { request_limit = value; }
		}
		
		public TraceMode TraceMode {
			get { return trace_mode; }
			set { trace_mode = value; }
		}		
	}
}

