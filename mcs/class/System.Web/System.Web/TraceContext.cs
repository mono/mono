// 
// System.Web.TraceContext
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) 2002 2003, Patrik Torstensson
// (C) 2003 Novell, Inc (http://www.novell.com) 
//

using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web {
   public sealed class TraceContext {
      private HttpContext _Context;
      private bool _Enabled;
      private TraceMode _Mode;
      private DataTable info;
      private int control_pos;

      public TraceContext(HttpContext Context) {
	 _Context = Context;
	 _Enabled = false;
      }

      public bool IsEnabled {
	 get {
	    return _Enabled;
	 }

	 set {
		 if (value && info == null)
			 InitInfoTable ();
	    _Enabled = value;
	 }
      }

      public TraceMode TraceMode {
	 get {
	    return _Mode;
	 }

	 set {
	    _Mode = value;
	 }
      }

      public void Warn(string msg) {
	 Write(String.Empty, msg, null, true);
      }

      public void Warn(string category, string msg) {
	 Write(category, msg, null, true);
      }

      public void Warn(string category, string msg, Exception error) {
	 Write(category, msg, error, true);
      }

      public void Write(string msg) {
	 Write(String.Empty, msg, null, true);
      }

      public void Write(string category, string msg) {
	 Write(category, msg, null, false);
      }

      public void Write(string category, string msg, Exception error) {
	 Write(category, msg, error, false);
      }

      [MonoTODO("Save the data into a web dataset directly...")]
      private void Write(string category, string msg, Exception error, bool Warning) {
	      if (!_Enabled)
		      return;
	      DataRow r = info.NewRow ();
	      r ["Category"] = category;
	      r ["Message"] = msg;
	      r ["Exception"] = (error != null ? error.ToString () : null);
	      r ["IsWarning"] = Warning;

	      info.Rows.Add (r);
      }

	   private void InitInfoTable ()
	   {
		   info = new DataTable ();
		   info.Columns.Add (new DataColumn ("Category", typeof (string)));
		   info.Columns.Add (new DataColumn ("Message", typeof (string)));
		   info.Columns.Add (new DataColumn ("Exception", typeof (string)));
		   info.Columns.Add (new DataColumn ("IsWarning", typeof (bool)));
	   }

	   internal void Render (HtmlTextWriter output)
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
		   table.Rows.Add (InfoRow2 ("Session Id:", _Context.Session.SessionID,
						   "Request Type", _Context.Request.RequestType));
		   table.Rows.Add (InfoRow2 ("Time of Request:", _Context.Timestamp.ToString (),
						   "State Code:", _Context.Response.StatusCode.ToString ()));
		   table.Rows.Add (InfoRow2 ("Request Encoding:", _Context.Request.ContentEncoding.EncodingName,
						   "Response Encoding:", _Context.Response.ContentEncoding.EncodingName));						     
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
						   "Viewstate Size Bytes (excluding children)"));

		   Page page = (Page) _Context.Handler;
		   control_pos = 0;
		   RenderControl (table, page, "__PAGE", "", control_pos);

		   table.RenderControl (output);
		   }
		   
	   private void RenderCookies (HtmlTextWriter output)
	   {
		   Table table = CreateTable ();

		   table.Rows.Add (AltRow ("Cookies Collection"));
		   table.Rows.Add (SubHeadRow ("Name", "Value", "Size"));

		   int pos = 0;
		   foreach (string key in _Context.Request.Cookies.Keys)
			   RenderCookie (table, _Context.Request.Cookies [key], pos++);
		   
		   table.RenderControl (output);
	   }
	   
	   private void RenderHeaders (HtmlTextWriter output)
	   {
		   Table table = CreateTable ();

		   table.Rows.Add (AltRow ("Headers Collection"));
		   table.Rows.Add (SubHeadRow ("Name", "Value"));

		   int pos = 0;
		   foreach (string key in _Context.Request.Headers.Keys)
			   RenderHeader (table, key, _Context.Request.Headers [key], pos++);

		   table.RenderControl (output);
	   }

	   private void RenderServerVars (HtmlTextWriter output)
	   {
		   Table table = CreateTable ();

		   table.Rows.Add (AltRow ("Server Variables"));
		   table.Rows.Add (SubHeadRow ("Name", "Value"));

		   int pos = 0;
		   foreach (string key in _Context.Request.ServerVariables)
			   RenderServerVar (table, key, _Context.Request.ServerVariables [key], pos++);

		   table.RenderControl (output);
	   }

	   private void RenderServerVar (Table table, string name, string value, int pos)
	   {
		   RenderAltRow (table, pos, name, value);
	   }
	   
	   private void RenderHeader (Table table, string name, string value, int pos)
	   {
		   RenderAltRow (table, pos, name, value);
	   }
	 
	   private void RenderCookie (Table table, HttpCookie c, int pos)
	   {
		   RenderAltRow (table, pos, c.Name, c.Value, "&nbsp;");
	   }
	   
	   private void RenderControl (Table table, Control c, string id, string p, int pos)
	   {
		   Console.WriteLine ("pos:  " + pos);
		   RenderAltRow (table, pos, p + id, c.GetType ().FullName, "&nbsp;", "&nbsp;");
		   
		   foreach (Control child in c.Controls)
			   RenderControl (table, child, c.UniqueID, p + "&nbsp;&nbsp;&nbsp;&nbsp;", ++control_pos);
	   }
	   
	   private TableRow AltRow (string title)
	   {
		   TableRow row = new TableRow ();
		   TableHeaderCell header = new TableHeaderCell ();
		   header.CssClass = "alt";
		   header.HorizontalAlign = HorizontalAlign.Left;
		   header.Attributes ["colspan"] = "10";
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

		   return RenderAltRow (table, pos, open + (string) r ["Category"] + close,
				   open + (string) r ["Message"] + close,
				   "&nbsp;", "&nbsp;");
	   }
	   
	   private TableRow SubHeadRow (params string[] cells)
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
	   
	   private TableRow RenderAltRow (Table table, int pos, params string[] cells)
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

	   private Table CreateTable ()
	   {
		   Table table = new Table ();

		   table.Width = Unit.Percentage (100);
		   table.CellSpacing = 0;
		   table.CellPadding = 0;

		   return table;
	   }
	   
	   private void RenderStyleSheet (HtmlTextWriter o)
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
