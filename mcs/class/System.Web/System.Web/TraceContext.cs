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
	 Write(category, msg, null, true);
      }

      public void Write(string category, string msg, Exception error) {
	 Write(category, msg, error, true);
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

		   RenderRequestDetails (output);
		   RenderTraceInfo (output);
		   
		   output.RenderEndTag ();
	   }

	   private void RenderRequestDetails (HtmlTextWriter output)
	   {
		   Table table = new Table ();

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
		   Table table = new Table ();

		   table.Rows.Add (AltRow ("Trace Information"));
		   table.Rows.Add (SubHeadRow ("Category", "Message", "From First(s)", "From Lasts(s)"));

		   bool even = false;
		   foreach (DataRow r in info.Rows) {
			   TableRow row = TraceInfoRow (r);
			   if (even) {
				   row.CssClass = "alt";
				   even = false;
			   } else
				   even = true;
			   table.Rows.Add (row);
		   }
		   
		   table.RenderControl (output);
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

	   private TableRow TraceInfoRow (DataRow r)
	   {
		   TableRow row = new TableRow ();
		   TableCell cell1 = new TableCell ();
		   TableCell cell2 = new TableCell ();
		   TableCell cell3 = new TableCell ();
		   TableCell cell4 = new TableCell ();

		   string open, close;

		   open = close = String.Empty;
		   if ((bool) r ["IsWarning"]) {
			   open = "<font color=\"Red\">";
			   close = "</font>";
		   }

		   cell1.Text = open + (string) r ["Category"] + close;
		   cell2.Text = open + (string) r ["Message"] + close;
		   // Ignore these for now
		   cell3.Text = "&nbsp";
		   cell4.Text = "&nbsp";

		   row.Cells.Add (cell1);
		   row.Cells.Add (cell2);
		   row.Cells.Add (cell3);
		   row.Cells.Add (cell4);

		   return row;
	   }
	   
	   private TableRow SubHeadRow (string a, string b, string c, string d)
	   {
		   TableRow row = new TableRow ();
		   TableHeaderCell cella = new TableHeaderCell ();
		   TableHeaderCell cellb = new TableHeaderCell ();
		   TableHeaderCell cellc = new TableHeaderCell ();
		   TableHeaderCell celld = new TableHeaderCell ();

		   cella.Text = a;
		   cellb.Text = b;
		   cellc.Text = c;
		   celld.Text = d;

		   row.CssClass = "subhead";
		   row.HorizontalAlign = HorizontalAlign.Left;
		   
		   row.Cells.Add (cella);
		   row.Cells.Add (cellb);
		   row.Cells.Add (cellc);
		   row.Cells.Add (celld);

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

		   return row;
	   }
   }
}
