//
// System.Web.TraceManager
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace System.Web {

	internal class TraceManager {

		private bool enabled = false;
		private bool local_only = true;
		private bool page_output = false;
		private TraceMode mode;
		private int request_limit = 10;

		private int cur_item;
		private TraceData[] data;
		
		public TraceManager ()
		{
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
		
		public void AddTraceData (TraceData item)
		{
			if (data == null)
				data = new TraceData [request_limit];
			if (cur_item == request_limit)
				return;
			data [cur_item++] = item;
		}

		public int ItemCount {
			get { return cur_item; }
		}
	}

}

