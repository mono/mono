//
// System.Web.TraceData
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Web {

	class InfoTraceData
	{
		public string Category;
		public string Message;
		public string Exception;
		public double TimeSinceFirst;
		public double TimeSinceLast;
		public bool IsWarning;

		public InfoTraceData (string category, string message, string exception, double timeSinceFirst, double timeSinceLast, bool isWarning)
		{
			this.Category = category;
			this.Message = message;
			this.Exception = exception;
			this.TimeSinceFirst = timeSinceFirst;
			this.TimeSinceLast = timeSinceLast;
			this.IsWarning = isWarning;
		}
	}

	class ControlTraceData
	{
		public string ControlId;
		public Type Type;
		public int RenderSize;
		public int ViewstateSize;
		public int Depth;
#if NET_2_0
		public int ControlstateSize;
#endif

#if NET_2_0
		public ControlTraceData (string controlId, Type type, int renderSize, int viewstateSize, int controlstateSize, int depth)
#else
		public ControlTraceData (string controlId, Type type, int renderSize, int viewstateSize, int depth)
#endif
		{
			this.ControlId = controlId;
			this.Type = type;
			this.RenderSize = renderSize;
			this.ViewstateSize = viewstateSize;
			this.Depth = depth;
#if NET_2_0
			this.ControlstateSize = controlstateSize;
#endif
		}
	}

	class NameValueTraceData
	{
		public string Name;
		public string Value;

		public NameValueTraceData (string name, string value)
		{
			this.Name = name;
			this.Value = value;
		}
	}
	
	internal class TraceData {

		bool is_first_time;
		DateTime first_time;
		double prev_time;

#if NET_2_0
		Queue <InfoTraceData> info;
		Queue <ControlTraceData> control_data;
		Queue <NameValueTraceData> cookie_data;
		Queue <NameValueTraceData> header_data;
		Queue <NameValueTraceData> servervar_data;
		Hashtable ctrl_cs;
#else
		Queue info;
		Queue control_data;
		Queue cookie_data;
		Queue header_data;
		Queue servervar_data;
		//DataTable viewstate_data;
#endif
		
		string request_path;
		string session_id;
		DateTime request_time;
		Encoding request_encoding;
		Encoding response_encoding;
		string request_type;
		int status_code;
		Page page;
		TraceMode _traceMode = HttpRuntime.TraceManager.TraceMode;

		public TraceData ()
		{
#if NET_2_0
			info = new Queue <InfoTraceData> ();
			control_data = new Queue <ControlTraceData> ();
			cookie_data = new Queue <NameValueTraceData> ();
			header_data = new Queue <NameValueTraceData> ();
			servervar_data = new Queue <NameValueTraceData> ();
#else
			info = new Queue ();
			control_data = new Queue ();
			cookie_data = new Queue ();
			header_data = new Queue ();
			servervar_data = new Queue ();
#endif

			/* TODO
			viewstate_data = new DataTable ();
			viewstate_data.Columns.Add (new DataColumn ("ControlId", typeof (string)));
			viewstate_data.Columns.Add (new DataColumn ("Data", typeof (string)));
			*/

			is_first_time = true;
		}

		public TraceMode TraceMode {
			get { return _traceMode; }
			set { _traceMode = value; }
		}

		public string RequestPath {
			get { return request_path; }
			set { request_path = value; }
		}
		
		public string SessionID {
			get { return session_id; }
			set { session_id = value; }
		}

		public DateTime RequestTime {
			get { return request_time; }
			set { request_time = value; }
		}

		public Encoding RequestEncoding {
			get { return request_encoding; }
			set { request_encoding = value; }
		}

		public Encoding ResponseEncoding {
			get { return response_encoding; }
			set { response_encoding = value; }
		}

		public string RequestType {
			get { return request_type; }
			set { request_type = value; }
		}

		public int StatusCode {
			get { return status_code; }
			set { status_code = value; }
		}

		public void Write (string category, string msg, Exception error, bool Warning)
		{
			double time;
			double time_from_last;
			if (is_first_time) {
				time = 0;
				time_from_last = 0; 
				prev_time = 0;
				is_first_time = false;
				first_time = DateTime.Now;
			}
			else {
				time = (DateTime.Now - first_time).TotalSeconds;
				time_from_last = time - prev_time;
				prev_time = time;
			}

			info.Enqueue (
				new InfoTraceData (category,
						   HtmlEncode (msg),
						   (error != null ? error.ToString () : null),
						   time,
						   time_from_last,
						   Warning));
		}

		static string HtmlEncode (string s)
		{
			if (s == null)
				return "";

			string res = HttpUtility.HtmlEncode (s);
			res = res.Replace ("\n", "<br>");
			return res.Replace (" ", "&nbsp;");
		}
		
#if NET_2_0
		public void AddControlTree (Page page, Hashtable ctrl_vs, Hashtable ctrl_cs, Hashtable sizes)
#else
		public void AddControlTree (Page page, Hashtable ctrl_vs, Hashtable sizes)
#endif
		{
			this.page = page;
			this.ctrl_vs = ctrl_vs;
			this.sizes = sizes;
#if NET_2_0
			this.ctrl_cs = ctrl_cs;
#endif
			AddControl (page, 0);
		}

		Hashtable sizes;
		Hashtable ctrl_vs;
		void AddControl (Control c, int control_pos)
		{
			control_data.Enqueue (
				new ControlTraceData (
					c.UniqueID,
					c.GetType (),
					GetRenderSize (c),
					GetViewStateSize (c, (ctrl_vs != null) ? ctrl_vs [c] : null),
#if NET_2_0
					GetViewStateSize (c, (ctrl_cs != null) ? ctrl_cs [c] : null),
#endif
					control_pos));
			
			if (c.HasControls ()) {
				foreach (Control child in c.Controls)
					AddControl (child, control_pos + 1);
			}
		}

		int GetRenderSize (Control ctrl)
		{
			if (sizes == null)
				return 0;

			object s = sizes [ctrl];
			return s == null ? 0 : (int) s;
		}

		static int GetViewStateSize (Control ctrl, object vs)
		{
			if (vs == null)
				return 0;

			StringWriter sr = new StringWriter ();
			LosFormatter fmt = new LosFormatter ();
			fmt.Serialize (sr, vs);
			return sr.GetStringBuilder ().Length;
		}

		public void AddCookie (string name, string value)
		{
			cookie_data.Enqueue (new NameValueTraceData (name, value));
		}

		public void AddHeader (string name, string value)
		{
			header_data.Enqueue (new NameValueTraceData (name, value));
		}

		public void AddServerVar (string name, string value)
		{
			servervar_data.Enqueue (new NameValueTraceData (name, value));
		}
		
		public void Render (HtmlTextWriter output)
		{
			output.AddAttribute ("id", "__asptrace");
			output.RenderBeginTag (HtmlTextWriterTag.Div);
			
			RenderStyleSheet (output);
			
			output.AddAttribute ("class", "tracecontent");
			output.RenderBeginTag (HtmlTextWriterTag.Span);
			
			RenderRequestDetails (output);
			RenderTraceInfo (output);
			RenderControlTree (output);
			RenderCookies (output);
			RenderHeaders (output);
			RenderServerVars (output);
			
			output.RenderEndTag ();
			output.RenderEndTag ();
		}
		
		void RenderRequestDetails (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Request Details:"));
			table.Rows.Add (InfoRow2 ("Session Id:", session_id,
							"Request Type", request_type));
			table.Rows.Add (InfoRow2 ("Time of Request:", request_time.ToString (),
							"State Code:", status_code.ToString ()));
			table.Rows.Add (InfoRow2 ("Request Encoding:", request_encoding.EncodingName,
						   "Response Encoding:", response_encoding.EncodingName));	     
			table.RenderControl (output);
		}
		
		void RenderTraceInfo (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Trace Information"));
			table.Rows.Add (SubHeadRow ("Category", "Message", "From First(s)", "From Lasts(s)"));
			
			int pos = 0;
#if NET_2_0
			IEnumerable<InfoTraceData> enumerable = info;

			if (TraceMode == TraceMode.SortByCategory) {
				List<InfoTraceData> list = new List<InfoTraceData> (info);
				list.Sort (delegate (InfoTraceData x, InfoTraceData y) { return String.Compare (x.Category, y.Category, StringComparison.Ordinal); });
				enumerable = list;
			}

			foreach (InfoTraceData i in enumerable)
				RenderTraceInfoRow (table, i, pos++);
#else
			foreach (object o in info)
				RenderTraceInfoRow (table, o as InfoTraceData, pos++);
#endif
			table.RenderControl (output);
		}
		
		void RenderControlTree (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			int page_vs_size = page == null ? 0 : GetViewStateSize (page, page.GetSavedViewState ());
			table.Rows.Add (AltRow ("Control Tree"));
			table.Rows.Add (SubHeadRow ("Control Id", "Type",
						"Render Size Bytes (including children)",
#if TARGET_J2EE
						"ViewState Size (excluding children)"
#else
						String.Format ("ViewState Size (total: {0} bytes)(excluding children)",
								page_vs_size)
#endif
#if NET_2_0
						,"ControlState Size (excluding children)"
#endif
							));
			
			int pos = 0;
#if NET_2_0
			foreach (ControlTraceData r in control_data)
				RenderControlTraceDataRow (table, r, pos++);
#else
			foreach (object o in control_data)
				RenderControlTraceDataRow (table, o as ControlTraceData, pos++);
#endif
			table.RenderControl (output);
		}

		void RenderControlTraceDataRow (Table table, ControlTraceData r, int pos)
		{
			if (r == null)
				return;
			
			int depth = r.Depth;
			string prefix = String.Empty;
			for (int i=0; i<depth; i++)
				prefix += "&nbsp;&nbsp;&nbsp;&nbsp;";
			RenderAltRow (table, pos, prefix + r.ControlId,
				      r.Type.ToString (), r.RenderSize.ToString (),
#if NET_2_0
					  r.ViewstateSize.ToString (), r.ControlstateSize.ToString ());
#else
				      r.ViewstateSize.ToString ());
#endif
		}
		
		void RenderCookies (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Cookies Collection"));
			table.Rows.Add (SubHeadRow ("Name", "Value", "Size"));
			
			int pos = 0;
#if NET_2_0
			foreach (NameValueTraceData r in cookie_data)
				RenderCookieDataRow (table, r, pos++);
#else
			foreach (object o in cookie_data)
				RenderCookieDataRow (table, o as NameValueTraceData, pos++);
#endif		
			
			table.RenderControl (output);
		}

		void RenderCookieDataRow (Table table, NameValueTraceData r, int pos)
		{
			if (r == null)
				return;
			
			int length = r.Name.Length + (r.Value == null ? 0 : r.Value.Length);
			RenderAltRow (table, pos++, r.Name, r.Value, length.ToString ());
		}
		
		void RenderHeaders (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Headers Collection"));
			table.Rows.Add (SubHeadRow ("Name", "Value"));
			
			int pos = 0;
#if NET_2_0
			foreach (NameValueTraceData r in header_data)
				RenderAltRow (table, pos++, r.Name, r.Value);
#else
			NameValueTraceData r;
			foreach (object o in header_data) {
				r = o as NameValueTraceData;
				if (r == null)
					continue;
				
				RenderAltRow (table, pos++, r.Name, r.Value);
			}
#endif
			table.RenderControl (output);
		}
		
		void RenderServerVars (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Server Variables"));
			table.Rows.Add (SubHeadRow ("Name", "Value"));
			
			int pos = 0;
#if NET_2_0
			foreach (NameValueTraceData r in servervar_data)
				RenderAltRow (table, pos++, r.Name, r.Value);
#else
			NameValueTraceData r;
			foreach (object o in servervar_data) {
				r = o as NameValueTraceData;
				if (r == null)
					continue;
				
				RenderAltRow (table, pos++, r.Name, r.Value);
			}
#endif			
			table.RenderControl (output);
		}

		internal static TableRow AltRow (string title)
		{
			TableRow row = new TableRow ();
			TableHeaderCell header = new TableHeaderCell ();
			header.CssClass = "alt";
			header.HorizontalAlign = HorizontalAlign.Left;
			header.Attributes [" colspan"] = "10";
			header.Text = "<h3><b>" + title + "</b></h3>";

			row.Cells.Add (header);
			return row;
		}
		
		void RenderTraceInfoRow (Table table, InfoTraceData i, int pos)
		{
			if (i == null)
				return;
			
			string open, close;
			open = close = String.Empty;
			if ((bool) i.IsWarning) {
				open = "<font color=\"Red\">";
				close = "</font>";
			}

			string t1, t2;
#if !TARGET_J2EE
			if (i.TimeSinceFirst == 0) {
				t1 = t2 = String.Empty;
			} else
#endif
			{
				t1 = i.TimeSinceFirst.ToString ("0.000000");
				t2 = i.TimeSinceLast.ToString ("0.000000");
			}
			
			RenderAltRow (table, pos, open + (string) i.Category + close,
				      open + (string) i.Message + close, t1, t2);
		}
	   
		internal static TableRow SubHeadRow (params string[] cells)
		{
			TableRow row = new TableRow ();
			foreach (string s in cells) {
				TableHeaderCell cell = new TableHeaderCell ();
				cell.Text = s;
				row.Cells.Add (cell);
			}
			
			row.CssClass = "subhead";
			row.HorizontalAlign = HorizontalAlign.Left;
			
			return row;
		}
		
		internal static TableRow RenderAltRow (Table table, int pos, params string[] cells)
		{
			TableRow row = new TableRow ();
			foreach (string s in cells) {
				TableCell cell = new TableCell ();
				cell.Text = s;
				row.Cells.Add (cell);
		   }
			
			if ((pos % 2) != 0)
				row.CssClass = "alt";
			
			table.Rows.Add (row);
			return row;
		}
	   
		TableRow InfoRow2 (string title1, string info1, string title2, string info2)
		{
			TableRow row = new TableRow ();
			TableHeaderCell header1 = new TableHeaderCell ();
			TableHeaderCell header2 = new TableHeaderCell ();
			TableCell cell1 = new TableCell ();
			TableCell cell2 = new TableCell ();
			
			header1.Text = title1;
			header2.Text = title2;
			cell1.Text = info1;
			cell2.Text = info2;
			
			row.Cells.Add (header1);
			row.Cells.Add (cell1);
			row.Cells.Add (header2);
			row.Cells.Add (cell2);

			row.HorizontalAlign = HorizontalAlign.Left;
			
			return row;
		}
		
		internal static Table CreateTable ()
		{
			Table table = new Table ();
			
			table.Width = Unit.Percentage (100);
			table.CellSpacing = 0;
			table.CellPadding = 0;
			
			return table;
		}
		
		internal static void RenderStyleSheet (HtmlTextWriter o)
		{
			o.WriteLine ("<style type=\"text/css\">");
			o.Write ("span.tracecontent { background-color:white; ");
			o.WriteLine ("color:black;font: 10pt verdana, arial; }");
			o.Write ("span.tracecontent table { font: 10pt verdana, ");
			o.WriteLine ("arial; cellspacing:0; cellpadding:0; margin-bottom:25}");
			o.WriteLine ("span.tracecontent tr.subhead { background-color:cccccc;}");
			o.WriteLine ("span.tracecontent th { padding:0,3,0,3 }");
			o.WriteLine ("span.tracecontent th.alt { background-color:black; color:white; padding:3,3,2,3; }");
			o.WriteLine ("span.tracecontent td { padding:0,3,0,3 }");
			o.WriteLine ("span.tracecontent tr.alt { background-color:eeeeee }");
			o.WriteLine ("span.tracecontent h1 { font: 24pt verdana, arial; margin:0,0,0,0}");
			o.WriteLine ("span.tracecontent h2 { font: 18pt verdana, arial; margin:0,0,0,0}");
			o.WriteLine ("span.tracecontent h3 { font: 12pt verdana, arial; margin:0,0,0,0}");
			o.WriteLine ("span.tracecontent th a { color:darkblue; font: 8pt verdana, arial; }");
			o.WriteLine ("span.tracecontent a { color:darkblue;text-decoration:none }");
			o.WriteLine ("span.tracecontent a:hover { color:darkblue;text-decoration:underline; }");
			o.WriteLine ("span.tracecontent div.outer { width:90%; margin:15,15,15,15}");
			o.Write ("span.tracecontent table.viewmenu td { background-color:006699; ");
			o.WriteLine ("color:white; padding:0,5,0,5; }");
			o.WriteLine ("span.tracecontent table.viewmenu td.end { padding:0,0,0,0; }");
			o.WriteLine ("span.tracecontent table.viewmenu a {color:white; font: 8pt verdana, arial; }");
			o.WriteLine ("span.tracecontent table.viewmenu a:hover {color:white; font: 8pt verdana, arial; }");
			o.WriteLine ("span.tracecontent a.tinylink {color:darkblue; font: 8pt verdana, ");
			o.WriteLine ("arial;text-decoration:underline;}");
			o.WriteLine ("span.tracecontent a.link {color:darkblue; text-decoration:underline;}");
			o.WriteLine ("span.tracecontent div.buffer {padding-top:7; padding-bottom:17;}");
			o.WriteLine ("span.tracecontent .small { font: 8pt verdana, arial }");
			o.WriteLine ("span.tracecontent table td { padding-right:20 }");
			o.WriteLine ("span.tracecontent table td.nopad { padding-right:5 }");
			o.WriteLine ("</style>");
		}
	}
}

