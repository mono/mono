//------------------------------------------------------------------------------
// <copyright file="TraceHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Trace Handler
 *
 * Copyright (c) 1998-1999, Microsoft Corporation
 *
 */
namespace System.Web.Handlers {
    using System;
    using System.Collections;
    using System.Web;
    using System.Globalization;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using System.Web.Util;
    using System.Web.SessionState;
    using System.Data;
    using System.Text;
    using System.Drawing;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class TraceHandler : IHttpHandler {
        private const string _style =
        "<style type=\"text/css\">\r\n" +
        "span.tracecontent b { color:white }\r\n" +
        "span.tracecontent { background-color:white; color:black;font: 10pt verdana, arial; }\r\n" +
        "span.tracecontent table { clear:left; font: 10pt verdana, arial; cellspacing:0; cellpadding:0; margin-bottom:25}\r\n" +
        "span.tracecontent tr.subhead { background-color:#cccccc;}\r\n" +
        "span.tracecontent th { padding:0,3,0,3 }\r\n" +
        "span.tracecontent th.alt { background-color:black; color:white; padding:3,3,2,3; }\r\n" +
        "span.tracecontent td { color: black; padding:0,3,0,3; text-align: left }\r\n" +
        "span.tracecontent td.err { color: red; }\r\n" +
        "span.tracecontent tr.alt { background-color:#eeeeee }\r\n" +
        "span.tracecontent h1 { font: 24pt verdana, arial; margin:0,0,0,0}\r\n" +
        "span.tracecontent h2 { font: 18pt verdana, arial; margin:0,0,0,0}\r\n" +
        "span.tracecontent h3 { font: 12pt verdana, arial; margin:0,0,0,0}\r\n" +
        "span.tracecontent th a { color:darkblue; font: 8pt verdana, arial; }\r\n" +
        "span.tracecontent a { color:darkblue;text-decoration:none }\r\n" +
        "span.tracecontent a:hover { color:darkblue;text-decoration:underline; }\r\n" +
        "span.tracecontent div.outer { width:90%; margin:15,15,15,15}\r\n" +
        "span.tracecontent table.viewmenu td { background-color:#006699; color:white; padding:0,5,0,5; }\r\n" +
        "span.tracecontent table.viewmenu td.end { padding:0,0,0,0; }\r\n" +
        "span.tracecontent table.viewmenu a {color:white; font: 8pt verdana, arial; }\r\n" +
        "span.tracecontent table.viewmenu a:hover {color:white; font: 8pt verdana, arial; }\r\n" +
        "span.tracecontent a.tinylink {color:darkblue; background-color:black; font: 8pt verdana, arial;text-decoration:underline;}\r\n" +
        "span.tracecontent a.link {color:darkblue; text-decoration:underline;}\r\n" +
        "span.tracecontent div.buffer {padding-top:7; padding-bottom:17;}\r\n" +
        "span.tracecontent .small { font: 8pt verdana, arial }\r\n" +
        "span.tracecontent table td { padding-right:20 }\r\n" +
        "span.tracecontent table td.nopad { padding-right:5 }\r\n" +
        "</style>\r\n";

        private HttpContext     _context;
        private HttpResponse    _response;
        private HttpRequest     _request;
        private HtmlTextWriter  _writer;

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public TraceHandler() {
        }

        // IHttpMethods exposed to derived classes

        protected void ProcessRequest(HttpContext context) {
            ((IHttpHandler)this).ProcessRequest(context);
        }

        protected bool IsReusable {
            get {
                return ((IHttpHandler)this).IsReusable;
            }
        }

        void IHttpHandler.ProcessRequest(HttpContext context) {
            // VSWhidbey 448844: Disable handler if retail is set to true
            if (DeploymentSection.RetailInternal ||
                (!context.Request.IsLocal && HttpRuntime.Profile.LocalOnly)) {
                HttpException e = new HttpException(403, null);
                e.SetFormatter(new TraceHandlerErrorFormatter(!DeploymentSection.RetailInternal));
                throw e;
            }

            _context = context;
            _response = _context.Response;
            _request = _context.Request;
            _writer = Page.CreateHtmlTextWriterInternal(_response.Output, _request);

            // if we're in integrated mode, we need to set the content type explicitly
            if (context.WorkerRequest is IIS7WorkerRequest) {
                _response.ContentType = _request.Browser.PreferredRenderingMime;
            }

            if (_writer == null) {
                // Can't create a writer, horked at this point, just return
                return;
            }

            _context.Trace.IsEnabled = false;

            // Validate the input to prevent XSS attacks.
            _request.ValidateInput();

            _writer.Write("<html>\r\n");
            _writer.Write("<head>\r\n");
            _writer.Write(StyleSheet);
            _writer.Write("</head>\r\n");

            _writer.Write("<body>\r\n");
            _writer.Write("<span class=\"tracecontent\">\r\n");

            if (!HttpRuntime.Profile.IsConfigEnabled) {
                HttpException e = new HttpException();
                e.SetFormatter(new TraceHandlerErrorFormatter(false));
                throw e;
            }

            IList datasets = HttpRuntime.Profile.GetData();

            // first check if we should clear data
            if (_request.QueryString["clear"] != null) {
                HttpRuntime.Profile.Reset();
                string url = _request.RawUrl;
                _response.Redirect(url.Substring(0, url.IndexOf("?", StringComparison.Ordinal)));
            }

            // then check if we are drilling down
            string strid = _request.QueryString["id"];
            if (strid != null) {
                int index = Int32.Parse(strid, CultureInfo.InvariantCulture);
                if (index >=0 && index < datasets.Count) {
                    ShowDetails((DataSet) datasets[index]);
                    ShowVersionDetails();
                    _writer.Write("</span>\r\n</body>\r\n</html>\r\n");
                    return;
                }
            }

            // if we get here, its just generic request
            ShowRequests(datasets);
            ShowVersionDetails();
            _writer.Write("</span>\r\n</body>\r\n</html>\r\n");
        }

        bool IHttpHandler.IsReusable {
            get { return false; }
        }

        protected void ShowDetails(DataSet data) {
            if (data == null) return;

            Table table;

            _writer.Write("<h1>" + SR.GetString(SR.Trace_Request_Details) + "</h1><br>");

            table = CreateDetailsTable(data.Tables[SR.Trace_Request]);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTraceTable(data.Tables[SR.Trace_Trace_Information]);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateControlTable(data.Tables[SR.Trace_Control_Tree]);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Session_State], true /*encodeSpaces*/);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Application_State], true /*encodeSpaces*/);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Request_Cookies_Collection], true /*encodeSpaces*/);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Response_Cookies_Collection], true /*encodeSpaces*/);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Headers_Collection], true /*encodeSpaces*/);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Form_Collection]);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Querystring_Collection]);
            if (table != null)
                table.RenderControl(_writer);

            table = CreateTable(data.Tables[SR.Trace_Server_Variables], true /*encodeSpaces*/);
            if (table != null)
                table.RenderControl(_writer);

        }

        protected void ShowVersionDetails() {
            _writer.Write("<hr width=100% size=1 color=silver>\r\n\r\n");
            _writer.Write(SR.GetString(SR.Error_Formatter_CLR_Build) + VersionInfo.ClrVersion +
                          SR.GetString(SR.Error_Formatter_ASPNET_Build) + VersionInfo.EngineVersion + "\r\n\r\n");
            _writer.Write("</font>\r\n\r\n");
        }

        protected void ShowRequests(IList data) {
            // add the title, application name, physical path, etc.
            Table table = new Table();
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Width = Unit.Percentage(100);
            TableRow trow = AddRow(table);

            TableCell tcell;
            AddCell(trow, SR.GetString(SR.Trace_Application_Trace));

            string vroot = _request.ApplicationPath;
            int vrootLen = vroot.Length;

            trow = AddRow(table);
            AddCell(trow, "<h2>" + HttpUtility.HtmlEncode(vroot.Substring(1)) + "<h2><p>");

            trow = AddRow(table);
            AddCell(trow, "[ <a href=\"Trace.axd?clear=1\" class=\"link\">" + SR.GetString(SR.Trace_Clear_Current) + "</a> ]");

            // check if we have permission to show the physical path.  If not, don't show anything.
            string physicalPath = "&nbsp";
            if (HttpRuntime.HasAppPathDiscoveryPermission())
                physicalPath  = SR.GetString(SR.Trace_Physical_Directory) + _request.PhysicalApplicationPath;
            trow = AddRow(table);
            tcell = AddCell(trow, physicalPath);

            table.RenderControl(_writer);

            //////// add the table of requests  ///////
            table = new Table();
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Width = Unit.Percentage(100);

            trow = AddRow(table);

            // title for the table
            tcell = AddHeaderCell(trow, "<h3><b>" + SR.GetString(SR.Trace_Requests_This) + "</b></h3>");
            tcell.ColumnSpan = 5;
            tcell.CssClass = "alt";
            tcell.HorizontalAlign = HorizontalAlign.Left;

            tcell = AddHeaderCell(trow, SR.GetString(SR.Trace_Remaining) + " " + HttpRuntime.Profile.RequestsRemaining.ToString(NumberFormatInfo.InvariantInfo));
            tcell.CssClass = "alt";
            tcell.HorizontalAlign = HorizontalAlign.Right;


            // add headers for the columns
            trow = AddRow(table);
            trow.HorizontalAlign = HorizontalAlign.Left;
            trow.CssClass = "subhead";
            AddHeaderCell(trow, SR.GetString(SR.Trace_No));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Time_of_Request));
            AddHeaderCell(trow, SR.GetString(SR.Trace_File));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Status_Code));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Verb));
            AddHeaderCell(trow, "&nbsp");

            // now fill the table with requests
            bool isAlt = true;
            for (int i = 0; i < data.Count; i++) {
                // for each request
                DataSet current = (DataSet)data[i];
                trow = AddRow(table);
                if (isAlt)
                    trow.CssClass = "alt";

                AddCell(trow, (i + 1).ToString(NumberFormatInfo.InvariantInfo));
                AddCell(trow, (string) current.Tables[SR.Trace_Request].Rows[0][SR.Trace_Time_of_Request]);
                AddCell(trow, HttpUtility.HtmlEncode((string) current.Tables[SR.Trace_Request].Rows[0][SR.Trace_Url]).Substring(vrootLen));
                AddCell(trow, current.Tables[SR.Trace_Request].Rows[0][SR.Trace_Status_Code].ToString());
                AddCell(trow, (string) current.Tables[SR.Trace_Request].Rows[0][SR.Trace_Request_Type]);

                TableCell linkcell = AddCell(trow, String.Empty);
                HtmlAnchor a = new HtmlAnchor();
                a.HRef = "Trace.axd?id=" + i;
                a.InnerHtml = "<nobr>" + SR.GetString(SR.Trace_View_Details);
                a.Attributes["class"] = "link";
                linkcell.Controls.Add(a);

                isAlt = !isAlt;
            }
            table.RenderControl(_writer);
        }

        ////// Static methods for creating tables //////////
        static private TableRow AddRow(Table t) {
            TableRow trow = new TableRow();
            t.Rows.Add(trow);
            return trow;
        }

        static private TableCell AddHeaderCell(TableRow trow, string text) {
            TableHeaderCell tcell = new TableHeaderCell();
            tcell.Text = text;
            trow.Cells.Add(tcell);
            return tcell;
        }

        static private TableCell AddCell(TableRow trow, string text) {
            TableCell tcell = new TableCell();
            tcell.Text = text;
            trow.Cells.Add(tcell);
            return tcell;
        }

        static internal string StyleSheet {
            get { return _style;}
        }

        static internal Table CreateControlTable(DataTable datatable) {
            Table table = new Table();
            if (datatable == null) return table;

            TableRow trow;
            TableCell       tcell;
            IEnumerator     en;
            string          parent;
            string          control;
            Hashtable       indentLevels = new Hashtable();
            int             indent;
            bool            isAlt = false;

            table.Width = Unit.Percentage(100);
            table.CellPadding = 0;
            table.CellSpacing = 0;


            // add a title for the table - same as table name
            trow = AddRow(table);
            tcell = AddHeaderCell(trow, "<h3><b>" + SR.GetString(datatable.TableName) + "</b></h3>");
            tcell.CssClass = "alt";
            tcell.ColumnSpan = 5;
            tcell.HorizontalAlign = HorizontalAlign.Left;

            // add the header information
            trow = AddRow(table);
            trow.CssClass = "subhead";
            trow.HorizontalAlign = HorizontalAlign.Left;
            AddHeaderCell(trow, SR.GetString(SR.Trace_Control_Id));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Type));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Render_Size_children));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Viewstate_Size_Nochildren));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Controlstate_Size_Nochildren));

            // prime the indentLevels hashtable with an initial value
            indentLevels["ROOT"] = 0;

            // now show the tree
            en = datatable.Rows.GetEnumerator();
            while (en.MoveNext()) {
                // DevDivBugs 173345: Error when enabling trace in an ASPX page
                // We also need to HtmlEncode parentId, as we HtmlEncode the controlId.
                parent = HttpUtility.HtmlEncode ((string) ((DataRow) en.Current)[SR.Trace_Parent_Id]);
                control = HttpUtility.HtmlEncode((string) ((DataRow) en.Current)[SR.Trace_Control_Id]);

                // this lets us determine how far to indent each control
                indent = (int) indentLevels[parent];
                indentLevels[control] = indent + 1;

                // do the indent
                StringBuilder indentedControl = new StringBuilder();

                // Don't want the ID's to break across lines
                indentedControl.Append("<nobr>");
                for (int i=0; i<indent; i++)
                    indentedControl.Append("&nbsp;&nbsp;&nbsp;&nbsp;");

                // page has a blank ID, so we'll fill in something nice for it
                if (control.Length == 0)
                    indentedControl.Append(SR.GetString(SR.Trace_Page));
                else
                    indentedControl.Append(control);

                trow = AddRow(table);
                AddCell(trow, indentedControl.ToString());
                AddCell(trow, (string) ((DataRow) en.Current)[SR.Trace_Type]);

                object size = ((DataRow) en.Current)[SR.Trace_Render_Size];
                if (size != null)
                    AddCell(trow, ((int) size).ToString(NumberFormatInfo.InvariantInfo));
                else
                    AddCell(trow, "---");

                size = ((DataRow) en.Current)[SR.Trace_Viewstate_Size];
                if (size != null)
                    AddCell(trow, ((int) size).ToString(NumberFormatInfo.InvariantInfo));
                else
                    AddCell(trow, "---");

                size = ((DataRow) en.Current)[SR.Trace_Controlstate_Size];
                if (size != null)
                    AddCell(trow, ((int) size).ToString(NumberFormatInfo.InvariantInfo));
                else
                    AddCell(trow, "---");

                // alternate colors
                if (isAlt)
                    trow.CssClass = "alt";
                isAlt = !isAlt;
            }

            return table;
        }

        static internal Table CreateTraceTable(DataTable datatable) {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.CellPadding = 0;
            table.CellSpacing = 0;

            if (datatable == null) return table;

            IEnumerator en;
            bool isAlt = false;
            TableRow trow;
            TableCell tcell;
            DataRow datarow;

            // add a title for the table - same as table name
            trow = AddRow(table);
            tcell = AddHeaderCell(trow, "<h3><b>" + SR.GetString(datatable.TableName) + "</b></h3>");
            tcell.CssClass = "alt";
            tcell.ColumnSpan = 10;
            tcell.HorizontalAlign = HorizontalAlign.Left;

            // add the header information - same as column names
            trow = AddRow(table);
            trow.CssClass = "subhead";
            trow.HorizontalAlign = HorizontalAlign.Left;
            AddHeaderCell(trow, SR.GetString(SR.Trace_Category));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Message));
            AddHeaderCell(trow, SR.GetString(SR.Trace_From_First));
            AddHeaderCell(trow, SR.GetString(SR.Trace_From_Last));

            // now fill in the values, but don't display null values
            en = datatable.DefaultView.GetEnumerator();
            while (en.MoveNext()) {
                trow = AddRow(table);
                datarow = ((DataRowView) en.Current).Row;

                bool isErr = datarow[SR.Trace_Warning].Equals("yes");

                // FormatPlainTextAsHtml the values first
                tcell = AddCell(trow, HttpUtility.FormatPlainTextAsHtml((string) datarow[SR.Trace_Category]));
                if (isErr) tcell.CssClass = "err";

                StringBuilder message = new StringBuilder(HttpUtility.FormatPlainTextAsHtml((string) datarow[SR.Trace_Message]));

                object errormessage = datarow["ErrorInfoMessage"];
                object errorstack =   datarow["ErrorInfoStack"];
                if (!(errormessage is System.DBNull))
                    message.Append("<br>" + HttpUtility.FormatPlainTextAsHtml((string) errormessage));
                if (!(errorstack is System.DBNull))
                    message.Append("<br>" + HttpUtility.FormatPlainTextAsHtml((string) errorstack));

                tcell = AddCell(trow, message.ToString());
                if (isErr) tcell.CssClass = "err";

                tcell = AddCell(trow, FormatPotentialDouble(datarow[SR.Trace_From_First]));
                if (isErr) tcell.CssClass = "err";

                tcell = AddCell(trow, FormatPotentialDouble(datarow[SR.Trace_From_Last]));
                if (isErr) tcell.CssClass = "err";


                // alternate colors
                if (isAlt)
                    trow.CssClass = "alt";

                isAlt = !isAlt;
            }

            return table;
        }

        private static string FormatPotentialDouble(object o) {
            // pretty-prints double values (no scientific notation)
            return (o is double) ? ((double)o).ToString("F6", CultureInfo.CurrentCulture) : o.ToString();
        }

        static internal Table CreateTable(DataTable datatable) {
            return CreateTable(datatable, false);
        }

        static internal Table CreateTable(DataTable datatable, bool encodeSpaces) {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.CellPadding = 0;
            table.CellSpacing = 0;

            if (datatable == null) return table;

            IEnumerator en;
            bool isAlt = false;
            Object[] cells;
            TableRow trow;
            TableCell tcell;

            // add a title for the table - same as table name
            trow = AddRow(table);
            tcell = AddHeaderCell(trow, "<h3><b>" + SR.GetString(datatable.TableName) + "</b></h3>");
            tcell.CssClass = "alt";
            tcell.ColumnSpan = 10;
            tcell.HorizontalAlign = HorizontalAlign.Left;

            // add the header information - same as column names
            trow = AddRow(table);
            trow.CssClass = "subhead";
            trow.HorizontalAlign = HorizontalAlign.Left;
            en = datatable.Columns.GetEnumerator();
            while (en.MoveNext())
                AddHeaderCell(trow, SR.GetString(((DataColumn) en.Current).ColumnName));

            // now fill in the values, but don't display null values
            en = datatable.Rows.GetEnumerator();
            while (en.MoveNext()) {
                cells = ((DataRow) en.Current).ItemArray;
                trow = AddRow(table);

                for (int i=0; i<cells.Length; i++) {
                    string temp;
                    if(encodeSpaces)
                        temp = HttpUtility.FormatPlainTextSpacesAsHtml(HttpUtility.HtmlEncode(cells[i].ToString()));
                    else
                        temp = HttpUtility.HtmlEncode(cells[i].ToString());

                    AddCell(trow, (temp.Length != 0) ? temp : "&nbsp;");
                }

                // alternate colors
                if (isAlt)
                    trow.CssClass = "alt";
                isAlt = !isAlt;
            }

            return table;
        }

        static internal Table CreateDetailsTable(DataTable datatable) {
            Table table = new Table();
            table.Width = Unit.Percentage(100);
            table.CellPadding = 0;
            table.CellSpacing = 0;

            if (datatable == null) return table;

            TableRow trow = AddRow(table);
            TableCell tcell = AddHeaderCell(trow, "<h3><b>" + SR.GetString(SR.Trace_Request_Details) + "</b></h3>");
            tcell.ColumnSpan = 10;
            tcell.CssClass = "alt";
            tcell.HorizontalAlign = HorizontalAlign.Left;

            trow = AddRow(table);
            trow.HorizontalAlign = HorizontalAlign.Left;
            AddHeaderCell(trow, SR.GetString(SR.Trace_Session_Id) + ":");
            AddCell(trow, HttpUtility.HtmlEncode(datatable.Rows[0][SR.Trace_Session_Id].ToString()));
            AddHeaderCell(trow, SR.GetString(SR.Trace_Request_Type) + ":");
            AddCell(trow, datatable.Rows[0][SR.Trace_Request_Type].ToString());

            trow = AddRow(table);
            trow.HorizontalAlign = HorizontalAlign.Left;
            AddHeaderCell(trow, SR.GetString(SR.Trace_Time_of_Request) + ":");
            AddCell(trow, datatable.Rows[0][SR.Trace_Time_of_Request].ToString());
            AddHeaderCell(trow, SR.GetString(SR.Trace_Status_Code) + ":");
            AddCell(trow, datatable.Rows[0][SR.Trace_Status_Code].ToString());

            trow = AddRow(table);
            trow.HorizontalAlign = HorizontalAlign.Left;
            AddHeaderCell(trow, SR.GetString(SR.Trace_Request_Encoding) + ":");
            AddCell(trow, datatable.Rows[0][SR.Trace_Request_Encoding].ToString());
            AddHeaderCell(trow, SR.GetString(SR.Trace_Response_Encoding) + ":");
            AddCell(trow, datatable.Rows[0][SR.Trace_Response_Encoding].ToString());

            return table;
        }

    }
}
