//
// System.Web.Handlers.TraceHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Web;
using System.Web.Util;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.Handlers
{
	public class TraceHandler : IHttpHandler
	{
		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			TraceManager manager = HttpRuntime.TraceManager;

			if (manager.LocalOnly && !context.Request.IsLocal) {
				// Need to figure out the error message that goes here
				// but I only have cassini for testing
				return;
			}
				
			HtmlTextWriter output = new HtmlTextWriter (context.Response.Output);

			if (context.Request.QueryString ["clear"] != null)
				manager.Clear ();
			
			string id_str = context.Request.QueryString ["id"];
			int id = -1;
			if (id_str != null)
				id = Int32.Parse (id_str);
			
			if (id > 0 && id <= manager.ItemCount) {
				RenderItem (manager, output, id);
			} else {
				string dir = context.Server.MapPath (UrlUtils.GetDirectory (context.Request.FilePath));
				RenderMenu (manager, output, dir);
			}
				
		}

		bool IHttpHandler.IsReusable
		{
			get {
				return false;
			}
		}

		private void RenderMenu (TraceManager manager, HtmlTextWriter output, string dir)
		{
			
			output.RenderBeginTag (HtmlTextWriterTag.Html);
			
			output.RenderBeginTag (HtmlTextWriterTag.Head);
			TraceData.RenderStyleSheet (output);
			output.RenderEndTag ();

			RenderHeader (output, dir);
			
			output.RenderBeginTag (HtmlTextWriterTag.Body);
			output.AddAttribute ("class", "tracecontent");
			output.RenderBeginTag (HtmlTextWriterTag.Span);

			Table table = TraceData.CreateTable ();
			
			table.Rows.Add (TraceData.AltRow ("Requests to the Application"));
			table.Rows.Add (TraceData.SubHeadRow ("No", "Time of Request",
							"File", "Status Code", "Verb", "&nbsp;"));

			if (manager.TraceData != null) {
				for (int i=0; i<manager.ItemCount; i++) {
					int item = i + 1;
					TraceData d = manager.TraceData [i];
					TraceData.RenderAltRow (table, i, item.ToString (), d.RequestTime.ToString (),
							d.RequestPath, d.StatusCode.ToString (), d.RequestType,
							"<a href=\"Trace.axd?id=" + item + "\" class=\"tinylink\">" +
							"<b><nobr>View Details</a>");
				}
				table.RenderControl (output);
			}
			
			output.RenderEndTag ();
			output.RenderEndTag ();
			
			output.RenderEndTag ();
		}

		private void RenderHeader (HtmlTextWriter output, string dir)
		{
			Table table = TraceData.CreateTable ();
			TableRow row1 = new TableRow ();
			TableRow row2 = new TableRow ();
			TableCell cell1 = new TableCell ();
			TableCell cell2 = new TableCell ();
			TableCell cell3 = new TableCell ();
			TableCell cell4 = new TableCell ();
			
			cell1.Text = "<h1>Application Trace</h1>";
			cell2.Text = "[ <a href=\"Trace.axd?clear=1\" class=\"link\">clear current trace</a> ]";
			
			cell2.HorizontalAlign = HorizontalAlign.Right;
			cell2.VerticalAlign = VerticalAlign.Bottom;
			
			row1.Cells.Add (cell1);
			row1.Cells.Add (cell2);

			cell3.Text = "<h2><h2><p>"; // ummm, WTF?
			cell4.Text = "<b>Physical Directory:</b>" + dir;

			row2.Cells.Add (cell3);
			row2.Cells.Add (cell4);

			table.Rows.Add (row1);
			table.Rows.Add (row2);

			table.RenderControl (output);
		}
		
		private void RenderItem (TraceManager manager, HtmlTextWriter output, int item)
		{
			manager.TraceData [item - 1].Render (output);
		}
	}
}

