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

using System.Collections;
using System.Data;
using System.Security.Permissions;
using System.Web.Util;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.Handlers
{
#if NET_2_0
	[Serializable]
#endif
	class TraceNotAvailableException : HttpException
	{
		bool notLocal;

		public TraceNotAvailableException (bool notLocal) :
			base (notLocal ? 403 : 500, "Trace Error")
		{
			this.notLocal = notLocal;
		}

		internal override string Description {
			get {
				if (notLocal)
					return "Trace is not enabled for remote clients.";

				return "Trace.axd is not enabled in the configuration file for this application.";
			}
		}
	}

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TraceHandler : IHttpHandler
	{
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public TraceHandler ()
		{
			// LAMESPEC: the ctor is documented to have a Demand for a SecurityPermission
			// but doesn't specify which one it is (tests shows it's UnmanagedCode)
		}

#if NET_2_0
		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			ProcessRequest (context);
		}

		protected void ProcessRequest (HttpContext context)
#else
		void IHttpHandler.ProcessRequest (HttpContext context)
#endif
		{
			TraceManager manager = HttpRuntime.TraceManager;

			if (!manager.Enabled || manager.LocalOnly && !context.Request.IsLocal)
				throw new TraceNotAvailableException (manager.Enabled);
				
			HtmlTextWriter output = new HtmlTextWriter (context.Response.Output);

			if (context.Request.QueryString ["clear"] != null) {
				manager.Clear ();
				context.Response.Redirect (context.Request.FilePath);
			}
			
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

#if NET_2_0
		bool IHttpHandler.IsReusable {
			get { return IsReusable; }
		}

		protected bool IsReusable {
#else
		bool IHttpHandler.IsReusable {
#endif
			get {
				return false;
			}
		}

		void RenderMenu (TraceManager manager, HtmlTextWriter output, string dir)
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

		void RenderHeader (HtmlTextWriter output, string dir)
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
		
		void RenderItem (TraceManager manager, HtmlTextWriter output, int item)
		{
			manager.TraceData [item - 1].Render (output);
		}

		[MonoLimitation ("Not implemented, does nothing")]
		protected void ShowDetails (DataSet data)
		{
		}
#if NET_2_0
		[MonoLimitation ("Not implemented, does nothing")]
		protected void ShowRequests (IList data)
		{
		}

		[MonoLimitation ("Not implemented, does nothing")]
		protected void ShowVersionDetails ()
		{
		}
#else
		[MonoTODO ("Not implemented, does nothing")]
		protected void ShowRequests (ArrayList list)
		{
		}
#endif
	}
}
