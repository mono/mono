//
// System.Web.TraceData
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web {

	internal class TraceData {

		private bool is_first_time;
		private DateTime first_time;
		private double prev_time;
		
		private DataTable info;
		private DataTable control_data;
		private DataTable cookie_data;
		private DataTable header_data;
		private DataTable servervar_data;

		private string request_path;
		private string session_id;
		private DateTime request_time;
		private Encoding request_encoding;
		private Encoding response_encoding;
		private string request_type;
		private int status_code;

		public TraceData ()
		{
			info = new DataTable ();
			info.Columns.Add (new DataColumn ("Category", typeof (string)));
			info.Columns.Add (new DataColumn ("Message", typeof (string)));
			info.Columns.Add (new DataColumn ("Exception", typeof (string)));
			info.Columns.Add (new DataColumn ("TimeSinceFirst", typeof (double)));
			info.Columns.Add (new DataColumn ("IsWarning", typeof (bool)));

			control_data = new DataTable ();
			control_data.Columns.Add (new DataColumn ("ControlId", typeof (string)));
			control_data.Columns.Add (new DataColumn ("Type", typeof (System.Type)));
			control_data.Columns.Add (new DataColumn ("RenderSize", typeof (int)));
			control_data.Columns.Add (new DataColumn ("ViewstateSize", typeof (int)));
			control_data.Columns.Add (new DataColumn ("Depth", typeof (int)));

			cookie_data = new DataTable ();
			cookie_data.Columns.Add (new DataColumn ("Name", typeof (string)));
			cookie_data.Columns.Add (new DataColumn ("Value", typeof (string)));

			header_data = new DataTable ();
			header_data.Columns.Add (new DataColumn ("Name", typeof (string)));
			header_data.Columns.Add (new DataColumn ("Value", typeof (string)));

			servervar_data = new DataTable ();
			servervar_data.Columns.Add (new DataColumn ("Name", typeof (string)));
			servervar_data.Columns.Add (new DataColumn ("Value", typeof (string)));

			is_first_time = true;
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
			if (is_first_time) {
				time = 0;
				is_first_time = false;
				first_time = DateTime.Now;
			} else
				time = (DateTime.Now - first_time).TotalSeconds;

			DataRow r = info.NewRow ();
			r ["Category"] = category;
			r ["Message"] = msg;
			r ["Exception"] = (error != null ? error.ToString () : null);
			r ["TimeSinceFirst"] = time;
			r ["IsWarning"] = Warning;

			info.Rows.Add (r);
		}

		public void AddControlTree (Page page)
		{
			AddControl (page, 0);
		}

		private void AddControl (Control c, int control_pos)
		{
			DataRow r = control_data.NewRow ();
			r ["ControlId"] = c.UniqueID;
			r ["Type"] = c.GetType ();
			r ["Depth"] = control_pos;

			control_data.Rows.Add (r);
			
			foreach (Control child in c.Controls)
				AddControl (child, control_pos + 1);
		}

		public void AddCookie (string name, string value)
		{
			DataRow r = cookie_data.NewRow ();

			r ["Name"] = name;
			r ["Value"] = value;

			cookie_data.Rows.Add (r);
		}

		public void AddHeader (string name, string value)
		{
			DataRow r = header_data.NewRow ();

			r ["Name"] = name;
			r ["Value"] = value;

			header_data.Rows.Add (r);
		}

		public void AddServerVar (string name, string value)
		{
			DataRow r = servervar_data.NewRow ();

			r ["Name"] = name;
			r ["Value"] = value;

			servervar_data.Rows.Add (r);
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
		
		private void RenderRequestDetails (HtmlTextWriter output)
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
		
		private void RenderTraceInfo (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Trace Information"));
			table.Rows.Add (SubHeadRow ("Category", "Message", "From First(s)", "From Lasts(s)"));
			
			int pos = 0;
			foreach (DataRow r in info.Rows)
				RenderTraceInfoRow (table, r, pos++);
			
			table.RenderControl (output);
		}
		
		private void RenderControlTree (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Control Tree"));
			table.Rows.Add (SubHeadRow ("Control Id", "Type",
							"Render Size Bytes (including children)",
							"View state Size Bytes (excluding children)"));
			
			int pos = 0;
			foreach (DataRow r in control_data.Rows) {
				int depth = (int) r ["Depth"];
				string prefix = String.Empty;
				for (int i=0; i<depth; i++)
					prefix += "&nbsp;&nbsp;&nbsp;&nbsp;";
				RenderAltRow (table, pos++, prefix + r ["ControlId"],
						r ["Type"].ToString (), "&nbsp;", "&nbsp;");
			}
			
			table.RenderControl (output);
		}
		   
		private void RenderCookies (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Cookies Collection"));
			table.Rows.Add (SubHeadRow ("Name", "Value", "Size"));
			
			int pos = 0;
			foreach (DataRow r in cookie_data.Rows) {
				string name = (string) r ["Name"];
				string value = (string) r ["Value"];
				int length = name.Length + (value == null ? 0 : value.Length);
				RenderAltRow (table, pos++, name, value, length.ToString ());
			}
			
			table.RenderControl (output);
		}
		
		private void RenderHeaders (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Headers Collection"));
			table.Rows.Add (SubHeadRow ("Name", "Value"));
			
			int pos = 0;
			foreach (DataRow r in header_data.Rows)
				RenderAltRow (table, pos++, (string) r ["Name"], (string) r ["Value"]);
			
			table.RenderControl (output);
		}
		
		private void RenderServerVars (HtmlTextWriter output)
		{
			Table table = CreateTable ();
			
			table.Rows.Add (AltRow ("Server Variables"));
			table.Rows.Add (SubHeadRow ("Name", "Value"));
			
			int pos = 0;
			foreach (DataRow r in servervar_data.Rows)
				RenderAltRow (table, pos++, (string) r ["Name"], (string) r ["Value"]);
			
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
		
		private TableRow RenderTraceInfoRow (Table table, DataRow r, int pos)
		{
			string open, close;
			open = close = String.Empty;
			if ((bool) r ["IsWarning"]) {
				open = "<font color=\"Red\">";
				close = "</font>";
			}
			
			double t = (double) r ["TimeSinceFirst"];
			string t1, t2;
			if (t == 0) {
				t1 = t2 = String.Empty;
				prev_time = 0;
			} else {
				t1 = t.ToString ("0.000000");
				t2 = (t - prev_time).ToString ("0.000000");
				prev_time = t;
			}
			
			return RenderAltRow (table, pos, open + (string) r ["Category"] + close,
					open + (string) r ["Message"] + close, t1, t2);
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
	   
		private TableRow InfoRow2 (string title1, string info1, string title2, string info2)
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

