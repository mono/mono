//------------------------------------------------------------------------------
// <copyright file="ErrorFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*********************************

Class hierarchy

ErrorFormatter (abstract)
    UnhandledErrorFormatter
        SecurityErrorFormatter
        UseLastUnhandledErrorFormatter
        TemplatedMailRuntimeErrorFormatter
    PageNotFoundErrorFormatter
    PageForbiddenErrorFormatter
    GenericApplicationErrorFormatter
    FormatterWithFileInfo (abstract)
        ParseErrorFormatter
        ConfigErrorFormatter
    DynamicCompileErrorFormatter
        TemplatedMailCompileErrorFormatter    
    UrlAuthFailedErrorFormatter
    TraceHandlerErrorFormatter
    TemplatedMailErrorFormatterGenerator
    AuthFailedErrorFormatter
    FileAccessFailedErrorFormatter
    PassportAuthFailedErrorFormatter

**********************************/

/*
 * Object used to put together ASP.NET HTML error messages
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web {
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.IO;
    using System.Globalization;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using System.Web.Compilation;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using Debug=System.Web.Util.Debug;
    using System.Web.Management;
    using System.Configuration;
    using System.Security;
    using System.Security.Permissions;

    /*
     * This is an abstract base class from which we derive other formatters.
     */
    internal abstract class ErrorFormatter {

        private StringCollection _adaptiveMiscContent;
        private StringCollection _adaptiveStackTrace;
        protected bool           _dontShowVersion = false;

        private const string startExpandableBlock =
            "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">" +
            "{1}" +
            ":</div>\r\n" +
            "<div id=\"{0}\" style=\"display: none;\">\r\n" +
            "            <br><table width=100% bgcolor=\"#ffffcc\">\r\n" +
            "               <tr>\r\n" +
            "                  <td>\r\n" +
            "                      <code><pre>\r\n\r\n";

        private const string endExpandableBlock =
            "                      </pre></code>\r\n\r\n" +
            "                  </td>\r\n" +
            "               </tr>\r\n" +
            "            </table>\r\n\r\n" +
            "            \r\n\r\n" +
            "</div>\r\n";

        private const string toggleScript = @"
        <script type=""text/javascript"">
        function OnToggleTOCLevel1(level2ID)
        {
        var elemLevel2 = document.getElementById(level2ID);
        if (elemLevel2.style.display == 'none')
        {
            elemLevel2.style.display = '';
        }
        else {
            elemLevel2.style.display = 'none';
        }
        }
        </script>
                            ";

        protected const string BeginLeftToRightTag = "<div dir=\"ltr\">";
        protected const string EndLeftToRightTag = "</div>";

        internal static bool RequiresAdaptiveErrorReporting(HttpContext context)
        {

            // If HostingInit failed, don't try to continue, as we are not sufficiently
            // initialized to execute this code (VSWhidbey 210495)
            if (HttpRuntime.HostingInitFailed)
                return false;

            HttpRequest request = (context != null) ? context.Request : null;
            if (context != null && context.WorkerRequest is System.Web.SessionState.StateHttpWorkerRequest)
                return false;

            // Request.Browser might throw if the configuration file has some
            // bad format.
            HttpBrowserCapabilities browser = null;
            try {
                browser = (request != null) ? request.Browser : null;
            }
            catch {
                return false;
            }

            if (browser != null &&
                browser["requiresAdaptiveErrorReporting"] == "true") {
                return true;
            }
            return false;
        }

        private Literal CreateBreakLiteral() {
            Literal breakControl = new Literal();
            breakControl.Text = "<br/>";
            return breakControl;
        }

        private Label CreateLabelFromText(String text) {
            Label label = new Label();
            label.Text = text;
            return label;
        }

        // Return error message in markup using adaptive rendering of web
        // controls.  This would also set the corresponding headers of the
        // response accordingly so content can be shown properly on devices.
        // This method has been added with the same signature of
        // GetHtmlErrorMessage for consistency.
        internal virtual string GetAdaptiveErrorMessage(HttpContext context, bool dontShowSensitiveInfo) {

            // This call will compute and set all the necessary properties of
            // this instance of ErrorFormatter.  Then the controls below can
            // collect info from the properties.  The returned html is safely
            // ignored.
            GetHtmlErrorMessage(dontShowSensitiveInfo);

            // We need to inform the Response object that adaptive error is used
            // so it can adjust the status code right before headers are written out.
            // It is because some mobile devices/browsers can display a page
            // content only if it is a normal response instead of response that
            // has error status code.
            context.Response.UseAdaptiveError = true;

            try {
                Page page = new ErrorFormatterPage();
                page.EnableViewState = false;

                HtmlForm form = new HtmlForm();
                page.Controls.Add(form);
                IParserAccessor formAdd = (IParserAccessor) form;

                // Display a server error text with the application name
                Label label = CreateLabelFromText(SR.GetString(SR.Error_Formatter_ASPNET_Error, HttpRuntime.AppDomainAppVirtualPath));
                label.ForeColor = Color.Red;
                label.Font.Bold = true;
                label.Font.Size = FontUnit.Large;
                formAdd.AddParsedSubObject(label);
                formAdd.AddParsedSubObject(CreateBreakLiteral());

                // Title
                label = CreateLabelFromText(ErrorTitle);
                label.ForeColor = Color.Maroon;
                label.Font.Bold = true;
                label.Font.Italic = true;
                formAdd.AddParsedSubObject(label);
                formAdd.AddParsedSubObject(CreateBreakLiteral());

                // Description
                formAdd.AddParsedSubObject(CreateLabelFromText(SR.GetString(SR.Error_Formatter_Description) + " " + Description));
                formAdd.AddParsedSubObject(CreateBreakLiteral());

                // Misc Title
                String miscTitle = MiscSectionTitle;
                if (!String.IsNullOrEmpty(miscTitle)) {
                    formAdd.AddParsedSubObject(CreateLabelFromText(miscTitle));
                    formAdd.AddParsedSubObject(CreateBreakLiteral());
                }

                // Misc Info
                StringCollection miscContent = AdaptiveMiscContent;
                if (miscContent != null && miscContent.Count > 0) {
                    foreach (String contentLine in miscContent) {
                        formAdd.AddParsedSubObject(CreateLabelFromText(contentLine));
                        formAdd.AddParsedSubObject(CreateBreakLiteral());
                    }
                }

                // File & line# info
                String sourceFilePath = GetDisplayPath();
                if (!String.IsNullOrEmpty(sourceFilePath)) {
                    String text = SR.GetString(SR.Error_Formatter_Source_File) + " " + sourceFilePath;
                    formAdd.AddParsedSubObject(CreateLabelFromText(text));
                    formAdd.AddParsedSubObject(CreateBreakLiteral());

                    text = SR.GetString(SR.Error_Formatter_Line) + " " + SourceFileLineNumber;
                    formAdd.AddParsedSubObject(CreateLabelFromText(text));
                    formAdd.AddParsedSubObject(CreateBreakLiteral());
                }

                // Stack trace info
                StringCollection stackTrace = AdaptiveStackTrace;
                if (stackTrace != null && stackTrace.Count > 0) {
                    foreach (String stack in stackTrace) {
                        formAdd.AddParsedSubObject(CreateLabelFromText(stack));
                        formAdd.AddParsedSubObject(CreateBreakLiteral());
                    }
                }

                // Temporarily use a string writer to capture the output and
                // return it accordingly.
                StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture);
                TextWriter textWriter = context.Response.SwitchWriter(stringWriter);
                page.ProcessRequest(context);
                context.Response.SwitchWriter(textWriter);

                return stringWriter.ToString();
            }
            catch {
                return GetStaticErrorMessage(context);
            }
        }

        private string GetPreferredRenderingType(HttpContext context) {
            HttpRequest request = (context != null) ? context.Request : null;

            // Request.Browser might throw if the configuration file has some
            // bad format.
            HttpBrowserCapabilities browser = null;
            try {
                browser = (request != null) ? request.Browser : null;
            }
            catch {
                return String.Empty;
            }
            return ((browser != null) ? browser["preferredRenderingType"] : String.Empty);
        }

        private string GetStaticErrorMessage(HttpContext context) {
            string preferredRenderingType = GetPreferredRenderingType(context);
            Debug.Assert(preferredRenderingType != null);

            string errorMessage;
            if (StringUtil.StringStartsWithIgnoreCase(preferredRenderingType, "xhtml")) {
                errorMessage = FormatStaticErrorMessage(StaticErrorFormatterHelper.XhtmlErrorBeginTemplate,
                                                        StaticErrorFormatterHelper.XhtmlErrorEndTemplate);
            }
            else if (StringUtil.StringStartsWithIgnoreCase(preferredRenderingType, "wml")) {
                errorMessage = FormatStaticErrorMessage(StaticErrorFormatterHelper.WmlErrorBeginTemplate,
                                                        StaticErrorFormatterHelper.WmlErrorEndTemplate);

                // VSWhidbey 161754: In the case that headers have been written,
                // we should try to set the content type only if needed.
                const string wmlContentType = "text/vnd.wap.wml";
                if (String.Compare(context.Response.ContentType, 0,
                                   wmlContentType, 0, wmlContentType.Length,
                                   StringComparison.OrdinalIgnoreCase) != 0) {
                    context.Response.ContentType = wmlContentType;
                }
            }
            else {
                errorMessage = FormatStaticErrorMessage(StaticErrorFormatterHelper.ChtmlErrorBeginTemplate,
                                                        StaticErrorFormatterHelper.ChtmlErrorEndTemplate);
            }
            return errorMessage;
        }

        private string FormatStaticErrorMessage(string errorBeginTemplate,
                                                string errorEndTemplate) {
            StringBuilder errorContent = new StringBuilder();

            // Server error text with the application name and Title
            string errorHeader = SR.GetString(SR.Error_Formatter_ASPNET_Error, HttpRuntime.AppDomainAppVirtualPath);
            errorContent.Append(String.Format(CultureInfo.CurrentCulture, errorBeginTemplate, errorHeader, ErrorTitle));

            // Description
            errorContent.Append(SR.GetString(SR.Error_Formatter_Description) + " " + Description);
            errorContent.Append(StaticErrorFormatterHelper.Break);

            // Misc Title
            String miscTitle = MiscSectionTitle;
            if (miscTitle != null && miscTitle.Length > 0) {
                errorContent.Append(miscTitle);
                errorContent.Append(StaticErrorFormatterHelper.Break);
            }

            // Misc Info
            StringCollection miscContent = AdaptiveMiscContent;
            if (miscContent != null && miscContent.Count > 0) {
                foreach (String contentLine in miscContent) {
                    errorContent.Append(contentLine);
                    errorContent.Append(StaticErrorFormatterHelper.Break);
                }
            }

            // File & line# info
            String sourceFilePath = GetDisplayPath();
            if (!String.IsNullOrEmpty(sourceFilePath)) {
                String text = SR.GetString(SR.Error_Formatter_Source_File) + " " + sourceFilePath;
                errorContent.Append(text);
                errorContent.Append(StaticErrorFormatterHelper.Break);

                text = SR.GetString(SR.Error_Formatter_Line) + " " + SourceFileLineNumber;
                errorContent.Append(text);
                errorContent.Append(StaticErrorFormatterHelper.Break);
            }

            // Stack trace info
            StringCollection stackTrace = AdaptiveStackTrace;
            if (stackTrace != null && stackTrace.Count > 0) {
                foreach (String stack in stackTrace) {
                    errorContent.Append(stack);
                    errorContent.Append(StaticErrorFormatterHelper.Break);
                }
            }

            errorContent.Append(errorEndTemplate);
            return errorContent.ToString();
        }

        internal string GetErrorMessage() {
            return GetErrorMessage(HttpContext.Current, true);
        }

        // Return error message by checking if adaptive error formatting
        // should be used.
        internal virtual string GetErrorMessage(HttpContext context, bool dontShowSensitiveInfo) {
            if (RequiresAdaptiveErrorReporting(context)) {
                return GetAdaptiveErrorMessage(context, dontShowSensitiveInfo);
            }
            return GetHtmlErrorMessage(dontShowSensitiveInfo);
        }

        internal /*public*/ string GetHtmlErrorMessage() {
            return GetHtmlErrorMessage(true);
        }

        internal /*public*/ string GetHtmlErrorMessage(bool dontShowSensitiveInfo) {

            // Give the formatter a chance to prepare its state
            PrepareFormatter();

            StringBuilder sb = new StringBuilder();

            // 


            sb.Append("<!DOCTYPE html>\r\n");
            sb.Append("<html");

            // VSWhidbey 477678: Honor right to left language text format.
            if (IsTextRightToLeft) {
                sb.Append(" dir=\"rtl\"");
            }

            sb.Append(">\r\n");
            sb.Append("    <head>\r\n");
            sb.Append("        <title>" + ErrorTitle + "</title>\r\n");
            sb.Append("        <meta name=\"viewport\" content=\"width=device-width\" />\r\n");
            sb.Append("        <style>\r\n");
            sb.Append("         body {font-family:\"Verdana\";font-weight:normal;font-size: .7em;color:black;} \r\n");
            sb.Append("         p {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n");
            sb.Append("         b {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n");
            sb.Append("         H1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n");
            sb.Append("         H2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n");
            sb.Append("         pre {font-family:\"Consolas\",\"Lucida Console\",Monospace;font-size:11pt;margin:0;padding:0.5em;line-height:14pt}\r\n");
            sb.Append("         .marker {font-weight: bold; color: black;text-decoration: none;}\r\n");
            sb.Append("         .version {color: gray;}\r\n");
            sb.Append("         .error {margin-bottom: 10px;}\r\n");
            sb.Append("         .expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n");
            sb.Append("         @media screen and (max-width: 639px) {\r\n");
            sb.Append("          pre { width: 440px; overflow: auto; white-space: pre-wrap; word-wrap: break-word; }\r\n");
            sb.Append("         }\r\n");
            sb.Append("         @media screen and (max-width: 479px) {\r\n");
            sb.Append("          pre { width: 280px; }\r\n");
            sb.Append("         }\r\n");
            sb.Append("        </style>\r\n");
            sb.Append("    </head>\r\n\r\n");
            sb.Append("    <body bgcolor=\"white\">\r\n\r\n");
            sb.Append("            <span><H1>" + SR.GetString(SR.Error_Formatter_ASPNET_Error, HttpRuntime.AppDomainAppVirtualPath) + "<hr width=100% size=1 color=silver></H1>\r\n\r\n");
            sb.Append("            <h2> <i>" + ErrorTitle + "</i> </h2></span>\r\n\r\n");
            sb.Append("            <font face=\"Arial, Helvetica, Geneva, SunSans-Regular, sans-serif \">\r\n\r\n");
            sb.Append("            <b> " + SR.GetString(SR.Error_Formatter_Description) +  " </b>" + Description + "\r\n");
            sb.Append("            <br><br>\r\n\r\n");
            if (MiscSectionTitle != null) {
                sb.Append("            <b> " + MiscSectionTitle + ": </b>" + MiscSectionContent + "<br><br>\r\n\r\n");
            }

            WriteColoredSquare(sb, ColoredSquareTitle, ColoredSquareDescription, ColoredSquareContent, WrapColoredSquareContentLines);
            if (ShowSourceFileInfo) {
                string displayPath = GetDisplayPath();
                if (displayPath == null)
                    displayPath = SR.GetString(SR.Error_Formatter_No_Source_File);
                sb.Append("            <b> " + SR.GetString(SR.Error_Formatter_Source_File) + " </b> " + displayPath + "<b> &nbsp;&nbsp; " + SR.GetString(SR.Error_Formatter_Line) + " </b> " + SourceFileLineNumber + "\r\n");
                sb.Append("            <br><br>\r\n\r\n");
            }

            ConfigurationErrorsException configErrors = Exception as ConfigurationErrorsException;
            if (configErrors != null && configErrors.Errors.Count > 1) {
                sb.Append(String.Format(CultureInfo.InvariantCulture, startExpandableBlock, "additionalConfigurationErrors",
                    SR.GetString(SR.TmplConfigurationAdditionalError)));

                //
                // Get the configuration message as though there were user code on the stack,
                // so that the full path to the configuration file is not shown if the app
                // does not have PathDiscoveryPermission.
                // 
                bool revertPermitOnly = false;
                try {
                    PermissionSet ps = HttpRuntime.NamedPermissionSet;
                    if (ps != null) {
                        ps.PermitOnly();
                        revertPermitOnly = true;
                    }
                    
                    int errorNumber = 0;
                    foreach(ConfigurationException configurationError in configErrors.Errors) {
                        if (errorNumber > 0) {
                            sb.Append(configurationError.Message);
                            sb.Append("<BR/>\r\n");
                        }

                        errorNumber++;
                    }
                }
                finally {
                    if (revertPermitOnly) {
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }

                sb.Append(endExpandableBlock);
                sb.Append(toggleScript);
            }
            // If it's a FileNotFoundException/FileLoadException/BadImageFormatException with a FusionLog,
            // write it out (ASURT 83587)
            if (!dontShowSensitiveInfo && Exception != null) {
                // (Only display the fusion log in medium or higher (ASURT 126827)
                if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                    WriteFusionLogWithAssert(sb);
                }
            }

            WriteColoredSquare(sb, ColoredSquare2Title, ColoredSquare2Description, ColoredSquare2Content, false);

            if (!(dontShowSensitiveInfo || _dontShowVersion)) {  // don't show version for security reasons
                sb.Append("            <hr width=100% size=1 color=silver>\r\n\r\n");
                sb.Append("            <b>" + SR.GetString(SR.Error_Formatter_Version) + "</b>&nbsp;" +
                                       SR.GetString(SR.Error_Formatter_CLR_Build) + VersionInfo.ClrVersion +
                                       SR.GetString(SR.Error_Formatter_ASPNET_Build) + VersionInfo.EngineVersion + "\r\n\r\n");
                sb.Append("            </font>\r\n\r\n");
            }
            sb.Append("    </body>\r\n");
            sb.Append("</html>\r\n");

            sb.Append(PostMessage);

            return sb.ToString();
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void WriteFusionLogWithAssert(StringBuilder sb) {
            for (Exception e = Exception; e != null; e = e.InnerException) {
                string fusionLog = null;
                string filename = null;
                FileNotFoundException fnfException = e as FileNotFoundException;
                if (fnfException != null) {
                    fusionLog = fnfException.FusionLog;
                    filename = fnfException.FileName;
                }
                FileLoadException flException = e as FileLoadException;
                if (flException != null) {
                    fusionLog = flException.FusionLog;
                    filename = flException.FileName;
                }
                BadImageFormatException bifException = e as BadImageFormatException;
                if (bifException != null) {
                    fusionLog = bifException.FusionLog;
                    filename = bifException.FileName;
                }
                if (!String.IsNullOrEmpty(fusionLog)) {
                    WriteColoredSquare(sb,
                                       SR.GetString(SR.Error_Formatter_FusionLog),
                                       SR.GetString(SR.Error_Formatter_FusionLogDesc, filename),
                                       HttpUtility.HtmlEncode(fusionLog),
                                       false /*WrapColoredSquareContentLines*/);
                    break;
                }
            }
        }

        private void WriteColoredSquare(StringBuilder sb, string title, string description,
            string content, bool wrapContentLines) {
            if (title != null) {
                sb.Append("            <b>" + title + ":</b> " + description + "<br><br>\r\n\r\n");
                sb.Append("            <table width=100% bgcolor=\"#ffffcc\">\r\n");
                sb.Append("               <tr>\r\n");
                sb.Append("                  <td>\r\n");
                sb.Append("                      <code>");
                if (!wrapContentLines)
                    sb.Append("<pre>");
                sb.Append("\r\n\r\n");
                sb.Append(content);
                if (!wrapContentLines)
                    sb.Append("</pre>");
                sb.Append("</code>\r\n\r\n");
                sb.Append("                  </td>\r\n");
                sb.Append("               </tr>\r\n");
                sb.Append("            </table>\r\n\r\n");
                sb.Append("            <br>\r\n\r\n");
            }
        }


        internal /*public*/ virtual void PrepareFormatter() {
            // VSWhidbey 139210: ErrorFormatter object might be reused and
            // the properties would be gone through again.  So we need to
            // clear the adaptive error content to avoid duplicate content.
            if (_adaptiveMiscContent != null) {
                _adaptiveMiscContent.Clear();
            }

            if (_adaptiveStackTrace != null) {
                _adaptiveStackTrace.Clear();
            }
        }

        /*
         * Return the associated exception object (if any)
         */
        protected virtual Exception Exception {
            get { return null; }
        }

        /*
         * Return the type of error.  e.g. "Compilation Error."
         */
        protected abstract string ErrorTitle {
            get;
        }

        /*
         * Return a description of the error
         * e.g. "An error occurred during the compilation of a resource required to service"
         */
        protected abstract string Description {
            get;
        }

        /*
         * A section used differently by different types of errors (title)
         * e.g. "Compiler Error Message"
         * e.g. "Exception Details"
         */
        protected abstract string MiscSectionTitle {
            get;
        }

        /*
         * A section used differently by different types of errors (content)
         * e.g. "BC30198: Expected: )"
         * e.g. "System.NullReferenceException"
         */
        protected abstract string MiscSectionContent {
            get;
        }

        /*
         * e.g. "Source Error"
         */
        protected virtual string ColoredSquareTitle {
            get { return null;}
        }

        /*
         * Optional text between color square title and the color square itself
         */
        protected virtual string ColoredSquareDescription {
            get { return null;}
        }

        /*
         * e.g. a piece of source code with the error context
         */
        protected virtual string ColoredSquareContent {
            get { return null;}
        }

        /*
         * If false, use a <pre></pre> tag around it
         */
        protected virtual bool WrapColoredSquareContentLines {
            get { return false;}
        }

        /*
         * e.g. "Source Error"
         */
        protected virtual string ColoredSquare2Title {
            get { return null;}
        }

        /*
         * Optional text between color square title and the color square itself
         */
        protected virtual string ColoredSquare2Description {
            get { return null;}
        }

        /*
         * e.g. a piece of source code with the error context
         */
        protected virtual string ColoredSquare2Content {
            get { return null;}
        }

        /*
         * Misc content which will be shown to mobile devices
         * e.g. compile error code
         */
        protected virtual StringCollection AdaptiveMiscContent {
            get {
                if (_adaptiveMiscContent == null) {
                    _adaptiveMiscContent = new StringCollection();
                }
                return _adaptiveMiscContent;
            }
        }

        /*
         * Exception stack trace which will be shown to mobile devices
         * e.g. stack trace of a runtime error
         */
        protected virtual StringCollection AdaptiveStackTrace {
            get {
                if (_adaptiveStackTrace == null) {
                    _adaptiveStackTrace = new StringCollection();
                }
                return _adaptiveStackTrace;
            }
        }

        /*
         * Determines whether SourceFileName and SourceFileLineNumber will be used
         */
        protected abstract bool ShowSourceFileInfo {
            get;
        }

        /*
         * e.g. d:\samples\designpreview\test.aspx
         */
        protected virtual string PhysicalPath {
            get { return null;}
        }

        /*
         * e.g. /myapp/test.aspx
         */
        protected virtual string VirtualPath {
            get { return null;}
        }

        /*
         * The line number in the source file
         */
        protected virtual int SourceFileLineNumber {
            get { return 0;}
        }

        protected virtual String PostMessage {
            get { return null; }
        }

        /*
         * Does this error have only information that we want to
         * show over the web to random users?
         */
        internal virtual bool CanBeShownToAllUsers {
            get { return false;}
        }

        // VSWhidbey 477678: Respect current language text format that is right
        // to left.  To be used by subclasses who need to adjust text format for
        // code area accordingly.
        protected static bool IsTextRightToLeft {
            get {
                return CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
            }
        }


        protected string WrapWithLeftToRightTextFormatIfNeeded(string content) {
            if (IsTextRightToLeft) {
                content = BeginLeftToRightTag + content + EndLeftToRightTag;
            }
            return content;
        }

        // Make an HTTP line pragma from a virtual path
        internal static string MakeHttpLinePragma(string virtualPath) {
            string server = "http://server";
            // We should only append a "/" if the virtual path does not
            // already start with "/". Otherwise, we end up with double
            // slashes, eg http://server//vpp/foo.aspx , and this breaks
            // the VirtualPathProvider. (DevDiv 157238)
            if (virtualPath != null && !virtualPath.StartsWith("/", StringComparison.Ordinal)) {
                server += "/";
            }

            return (new Uri(server + virtualPath)).ToString();
        }

        internal static string GetSafePath(string linePragma) {

            // First, check if it's an http line pragma
            string virtualPath = GetVirtualPathFromHttpLinePragma(linePragma);

            // If so, just return the virtual path
            if (virtualPath != null)
                return virtualPath;

            // If not, it must be a physical path, which we need to make safe
            return HttpRuntime.GetSafePath(linePragma);
        }

        internal static string GetVirtualPathFromHttpLinePragma(string linePragma) {

            if (String.IsNullOrEmpty(linePragma))
                return null;

            try {
                Uri uri = new Uri(linePragma);
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    return uri.LocalPath;
            }
            catch {}

            return null;
        }

        internal static string ResolveHttpFileName(string linePragma) {

            // When running under VS debugger, we use URL's instead of paths in our #line pragmas.
            // When we detect this situation, we need to do a MapPath to get back to the file name (ASURT 76211/114867)

            string virtualPath = GetVirtualPathFromHttpLinePragma(linePragma);

            // If we didn't detect a virtual path, just return the input
            if (virtualPath == null)
                return linePragma;

            return HostingEnvironment.MapPathInternal(virtualPath);
        }

        /*
         * This can be either a virtual or physical path, depending on what's available
         */
        private string GetDisplayPath() {

            if (VirtualPath != null)
                return VirtualPath;

            // It used to be an Assert on the following check but since
            // adaptive error rendering uses this method where both
            // VirtualPath and PhysicalPath might not set, it is changed to
            // an if statement.
            if (PhysicalPath != null)
                return HttpRuntime.GetSafePath(PhysicalPath);

            return null;
        }
    }

    /*
     * This formatter is used for runtime exceptions that don't fall into a
     * specific category.
     */
    internal class UnhandledErrorFormatter : ErrorFormatter {
        protected Exception _e;
        protected Exception _initialException;
        protected ArrayList _exStack = new ArrayList();
        protected string _physicalPath;
        protected int _line;
        private string _coloredSquare2Content;
        private bool _fGeneratedCodeOnStack;
        protected String _message;
        protected String _postMessage;

        internal UnhandledErrorFormatter(Exception e) : this(e, null, null){
        }

        internal UnhandledErrorFormatter(Exception e, String message, String postMessage) {
            _message = message;
            _postMessage = postMessage;
            _e = e;
        }

        internal /*public*/ override void PrepareFormatter() {

            // Build a stack of exceptions
            for (Exception e = _e; e != null; e = e.InnerException) {
                _exStack.Add(e);

                // Keep track of the initial exception (first one thrown)
                _initialException = e;
            }

            // Get the Square2Content first so the line number gets calculated
            _coloredSquare2Content = ColoredSquare2Content;
        }

        protected override Exception Exception {
            get { return _e; }
        }

        protected override string ErrorTitle {
            get {
                // Use the exception's message if there is one
                string msg = _initialException.Message;
                if (!String.IsNullOrEmpty(msg))
                    return HttpUtility.FormatPlainTextAsHtml(msg);

                // Otherwise, use some default string
                return SR.GetString(SR.Unhandled_Err_Error);
            }
        }

        protected override string Description {
            get {
                if (_message != null) {
                    return _message;
                }
                else {
                    return SR.GetString(SR.Unhandled_Err_Desc);
                }
            }
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.Unhandled_Err_Exception_Details);}
        }

        protected override string MiscSectionContent {
            get {
                string exceptionName = _initialException.GetType().FullName;
                StringBuilder msg = new StringBuilder(exceptionName);
                string adaptiveMiscLine = exceptionName;

                if (_initialException.Message != null) {
                    string errorMessage = HttpUtility.FormatPlainTextAsHtml(_initialException.Message);
                    msg.Append(": ");
                    msg.Append(errorMessage);
                    adaptiveMiscLine += ": " + errorMessage;
                }
                AdaptiveMiscContent.Add(adaptiveMiscLine);

                if (_initialException is UnauthorizedAccessException) {
                    msg.Append("\r\n<br><br>");
                    String errDesc = SR.GetString(SR.Unauthorized_Err_Desc1);
                    errDesc = HttpUtility.HtmlEncode(errDesc);
                    msg.Append(errDesc);
                    AdaptiveMiscContent.Add(errDesc);

                    msg.Append("\r\n<br><br>");
                    errDesc = SR.GetString(SR.Unauthorized_Err_Desc2);
                    errDesc = HttpUtility.HtmlEncode(errDesc);
                    msg.Append(errDesc);
                    AdaptiveMiscContent.Add(errDesc);
                }
                else if (_initialException is HostingEnvironmentException) {
                    String details = ((HostingEnvironmentException)_initialException).Details;

                    if (!String.IsNullOrEmpty(details)) {
                        msg.Append("\r\n<br><br><b>");
                        msg.Append(details);
                        msg.Append("</b>");
                        AdaptiveMiscContent.Add(details);
                    }
                }

                return msg.ToString();
            }
        }

        protected override string ColoredSquareTitle {
            get { return SR.GetString(SR.TmplCompilerSourceSecTitle);}
        }

        protected override string ColoredSquareContent {
            get {

                // If we couldn't get line info for the error, display a standard message
                if (_physicalPath == null) {

                    const string BeginLeftToRightMarker = "BeginMarker";
                    const string EndLeftToRightMarker = "EndMarker";
                    bool setLeftToRightMarker = false;

                    // The error text depends on whether .aspx code was found on the stack
                    // Also, if trust is less than medium, never display the message that
                    // explains how to turn on debugging, since it's not allowed (Whidbey 9176)
                    string msg;
                    if (!_fGeneratedCodeOnStack ||
                        !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                        msg = SR.GetString(SR.Src_not_available_nodebug);
                    }
                    else {
                        if (IsTextRightToLeft) {
                            setLeftToRightMarker = true;
                        }

                        // Because the resource string has both normal language text and config/code samples,
                        // left-to-right markup tags need to be wrapped around the config/code samples if
                        // right to left language format is being used.
                        //
                        // Note that the retrieved resource string will be passed to the call
                        // HttpUtility.FormatPlainTextAsHtml(), which does HtmlEncode.  In order to preserve
                        // the left-to-right markup tags, the resource string has been added with markers
                        // that identify the beginnings and ends of config/code samples.  After
                        // FormatPlainTextAsHtml() is called, and the markers will be replaced with
                        // left-to-right markup tags below.
                        msg = SR.GetString(SR.Src_not_available,
                                           ((setLeftToRightMarker) ? BeginLeftToRightMarker : string.Empty),
                                           ((setLeftToRightMarker) ? EndLeftToRightMarker : string.Empty),
                                           ((setLeftToRightMarker) ? BeginLeftToRightMarker : string.Empty),
                                           ((setLeftToRightMarker) ? EndLeftToRightMarker : string.Empty));
                    }

                    msg = HttpUtility.FormatPlainTextAsHtml(msg);

                    if (setLeftToRightMarker) {
                        // If only <div dir=ltr> was used to wrap around the left-to-right code text,
                        // the font rendering on Firefox was not good.  We use <code> in addition to
                        // the <div> tag to workaround the problem.
                        const string BeginLeftToRightTags = "</code>" + BeginLeftToRightTag + "<code>";
                        const string EndLeftToRightTags = "</code>" + EndLeftToRightTag + "<code>";
                        msg = msg.Replace(BeginLeftToRightMarker, BeginLeftToRightTags);
                        msg = msg.Replace(EndLeftToRightMarker, EndLeftToRightTags);
                    }

                    return msg;
                }

                return FormatterWithFileInfo.GetSourceFileLines(_physicalPath, Encoding.Default, null, _line);
            }
        }

        protected override bool WrapColoredSquareContentLines {
            // Only wrap the text if we're displaying the standard message
            get { return (_physicalPath == null);}
        }

        protected override string ColoredSquare2Title {
            get { return SR.GetString(SR.Unhandled_Err_Stack_Trace);}
        }

        protected override string ColoredSquare2Content {
            get {
                if (_coloredSquare2Content != null)
                    return _coloredSquare2Content;

                StringBuilder sb = new StringBuilder();
                bool addAdaptiveStackTrace = true;
                int sbBeginIndex = 0;

                for (int i = _exStack.Count - 1; i >=0; i--) {
                    if (i < _exStack.Count - 1)
                        sb.Append("\r\n");

                    Exception e = (Exception)_exStack[i];

                    sb.Append("[" + _exStack[i].GetType().Name);

                    // Display the error code if there is one
                    if ((e is ExternalException) && ((ExternalException) e).ErrorCode != 0)
                        sb.Append(" (0x" + (((ExternalException)e).ErrorCode).ToString("x", CultureInfo.CurrentCulture) + ")");

                    // Display the message if there is one
                    if (e.Message != null && e.Message.Length > 0)
                        sb.Append(": " + e.Message);

                    sb.Append("]\r\n");

                    // Display the stack trace
                    StackTrace st = new StackTrace(e, true /*fNeedFileInfo*/);
                    for (int j = 0; j < st.FrameCount; j++) {

                        if (addAdaptiveStackTrace) {
                            sbBeginIndex = sb.Length;
                        }
                        StackFrame sf = st.GetFrame(j);

                        MethodBase mb = sf.GetMethod();
                        Type declaringType = mb.DeclaringType;
                        string ns = String.Empty;
                        if (declaringType != null) {

                            // Check if this stack item is for ASP generated code (ASURT 51063).
                            // To do this, we check if the assembly lives in the codegen dir.
                            // But if the native offset is 0, it is likely that the method simply
                            // failed to JIT, in which case don't treat it as an ASP.NET stack,
                            // since no line number can ever be shown for it (VSWhidbey 87014).
                            string assemblyDir = null;
                            try {
                                // This could throw if the assembly is dynamic
                                assemblyDir = System.Web.UI.Util.GetAssemblyCodeBase(declaringType.Assembly);
                            }
                            catch {}

                            if (assemblyDir != null) {
                                assemblyDir = Path.GetDirectoryName(assemblyDir);
                                if (string.Compare(assemblyDir, HttpRuntime.CodegenDirInternal,
                                    StringComparison.OrdinalIgnoreCase) == 0 && sf.GetNativeOffset() > 0) {
                                    _fGeneratedCodeOnStack = true;
                                }
                            }

                            ns = declaringType.Namespace;
                        }

                        if (ns != null)
                            ns = ns + ".";

                        if (declaringType == null) {
                            sb.Append("   " + mb.Name + "(");
                        }
                        else {
                            sb.Append("   " + ns + declaringType.Name + "." +
                                mb.Name + "(");
                        }

                        ParameterInfo[] arrParams = mb.GetParameters();

                        for (int k = 0; k < arrParams.Length; k++) {
                            sb.Append((k != 0 ? ", " : String.Empty) + arrParams[k].ParameterType.Name + " " +
                                arrParams[k].Name);
                        }

                        sb.Append(")");

                        string fileName = GetFileName(sf);
                        if (fileName != null) {

                            // ASURT 114867: if it's an http path, turn it into a local path
                            fileName = ResolveHttpFileName(fileName);
                            if (fileName != null) {

                                // Remember the file/line number of the top level stack
                                // item for which we have symbols
                                if (_physicalPath == null && FileUtil.FileExists(fileName)) {
                                    _physicalPath = fileName;

                                    _line = sf.GetFileLineNumber();
                                }

                                sb.Append(" in " + HttpRuntime.GetSafePath(fileName) +
                                    ":" + sf.GetFileLineNumber());
                            }
                        }
                        else {
                            sb.Append(" +" + sf.GetNativeOffset());
                        }

                        if (addAdaptiveStackTrace) {
                            string stackTraceText = sb.ToString(sbBeginIndex,
                                                                sb.Length - sbBeginIndex);
                            AdaptiveStackTrace.Add(HttpUtility.HtmlEncode(stackTraceText));
                        }

                        sb.Append("\r\n");
                    }
                    // Due to size limitation, we only want to add the top
                    // stack trace for mobile devices.
                    addAdaptiveStackTrace = false;
                }

                _coloredSquare2Content = HttpUtility.HtmlEncode(sb.ToString());

                _coloredSquare2Content = WrapWithLeftToRightTextFormatIfNeeded(_coloredSquare2Content);

                return _coloredSquare2Content;
            }
        }

        // Dev10 786146: partial trust apps may not have PathDiscovery, so just pretend
        // the path is unknown.
        private string GetFileName(StackFrame sf) {
            string fileName = null;
            try {
                fileName = sf.GetFileName();
            }
            catch (SecurityException) {
            }
            return fileName;
        }

        protected override String PostMessage {
            get { return _postMessage; }
        }

        protected override bool ShowSourceFileInfo {
            get { return _physicalPath != null; }
        }

        protected override string PhysicalPath {
            get { return _physicalPath; }
        }

        protected override int SourceFileLineNumber {
            get { return _line; }
        }
    }

    /*
     * This formatter is used for security exceptions.
     */
    internal class SecurityErrorFormatter : UnhandledErrorFormatter {

        internal SecurityErrorFormatter(Exception e) : base(e) {}

        protected override string ErrorTitle {
            get {
                return SR.GetString(SR.Security_Err_Error);
            }
        }

        protected override string Description {
            get {
                // VSWhidbey 493720: Do Html encode to preserve space characters
                return HttpUtility.FormatPlainTextAsHtml(SR.GetString(SR.Security_Err_Desc));
            }
        }
    }

    /*
     * This formatter is used for 404: page not found errors
     */
    internal class PageNotFoundErrorFormatter : ErrorFormatter {
        protected string _htmlEncodedUrl;
        private StringCollection _adaptiveMiscContent = new StringCollection();

        internal PageNotFoundErrorFormatter(string url) {
            _htmlEncodedUrl = HttpUtility.HtmlEncode(url);
            _adaptiveMiscContent.Add(_htmlEncodedUrl);
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.NotFound_Resource_Not_Found);}
        }

        protected override string Description {
            get { return HttpUtility.FormatPlainTextAsHtml(SR.GetString(SR.NotFound_Http_404));}
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.NotFound_Requested_Url);}
        }

        protected override string MiscSectionContent {
            get { return _htmlEncodedUrl;}
        }

        protected override StringCollection AdaptiveMiscContent {
            get { return _adaptiveMiscContent;}
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }

        internal override bool CanBeShownToAllUsers {
            get { return true;}
        }
    }

    /*
     * This formatter is used for 403: forbidden
     */
    internal class PageForbiddenErrorFormatter : ErrorFormatter {
        protected string _htmlEncodedUrl;
        private StringCollection _adaptiveMiscContent = new StringCollection();
        private string _description;

        internal PageForbiddenErrorFormatter(string url): this(url, null) {
        }

        internal PageForbiddenErrorFormatter(string url, string description) {
            _htmlEncodedUrl = HttpUtility.HtmlEncode(url);
            _adaptiveMiscContent.Add(_htmlEncodedUrl);
            _description = description;
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Forbidden_Type_Not_Served);}
        }

        protected override string Description {
            get {
                if (_description != null) {
                    return _description;
                }
                Match m = Regex.Match(_htmlEncodedUrl, @"\.\w+$");

                String extMessage = String.Empty;

                if (m.Success)
                    extMessage = SR.GetString(SR.Forbidden_Extension_Incorrect, m.ToString());

                return HttpUtility.FormatPlainTextAsHtml(SR.GetString(SR.Forbidden_Extension_Desc, extMessage));
            }
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.NotFound_Requested_Url);}
        }

        protected override string MiscSectionContent {
            get { return _htmlEncodedUrl;}
        }

        protected override StringCollection AdaptiveMiscContent {
            get { return _adaptiveMiscContent;}
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }

        internal override bool CanBeShownToAllUsers {
            get { return true;}
        }
    }

    /*
     * This formatter is used for generic errors that hide sensitive information
     * error text is sometimes different for remote vs. local machines
     */
    internal class GenericApplicationErrorFormatter : ErrorFormatter {
        private bool _local;

        internal GenericApplicationErrorFormatter(bool local) {
            _local = local;
        }

        protected override string ErrorTitle {
            get {
                return SR.GetString(SR.Generic_Err_Title);
            }
        }

        protected override string Description {
            get {
                return SR.GetString(
                                    _local ? SR.Generic_Err_Local_Desc
                                           : SR.Generic_Err_Remote_Desc);
            }
        }

        protected override string MiscSectionTitle {
            get {
                return null;
            }
        }

        protected override string MiscSectionContent {
            get {
                return null;
            }
        }

        protected override string ColoredSquareTitle {
            get {
                String detailsTitle = SR.GetString(SR.Generic_Err_Details_Title);
                AdaptiveMiscContent.Add(detailsTitle);
                return detailsTitle;
            }
        }

        protected override string ColoredSquareDescription {
            get {
                String detailsDesc = SR.GetString(
                                    _local ? SR.Generic_Err_Local_Details_Desc
                                           : SR.Generic_Err_Remote_Details_Desc);
                detailsDesc = HttpUtility.HtmlEncode(detailsDesc);
                AdaptiveMiscContent.Add(detailsDesc);
                return detailsDesc;
            }
        }

        protected override string ColoredSquareContent {
            get {
                string content = HttpUtility.HtmlEncode(SR.GetString(
                                    _local ? SR.Generic_Err_Local_Details_Sample
                                           : SR.Generic_Err_Remote_Details_Sample));

                return (WrapWithLeftToRightTextFormatIfNeeded(content));
            }
        }

        protected override string ColoredSquare2Title {
            get {
                String noteTitle = SR.GetString(SR.Generic_Err_Notes_Title);
                AdaptiveMiscContent.Add(noteTitle);
                return noteTitle;
            }
        }

        protected override string ColoredSquare2Description {
            get {
                String notesDesc = SR.GetString(SR.Generic_Err_Notes_Desc);
                notesDesc = HttpUtility.HtmlEncode(notesDesc);
                AdaptiveMiscContent.Add(notesDesc);
                return notesDesc;
            }
        }

        protected override string ColoredSquare2Content {
            get {
                string content = HttpUtility.HtmlEncode(SR.GetString(
                                    _local ? SR.Generic_Err_Local_Notes_Sample
                                           : SR.Generic_Err_Remote_Notes_Sample));

                return (WrapWithLeftToRightTextFormatIfNeeded(content));
            }
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }

        internal override bool CanBeShownToAllUsers {
            get { return true;}
        }
    }

    /*
    * This formatter is used when we couldn't run the normal custom error page (due to it also failing)
    */
    internal class CustomErrorFailedErrorFormatter : ErrorFormatter {
        internal CustomErrorFailedErrorFormatter() {
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Generic_Err_Title); }
        }

        protected override string Description {
            get { return HttpUtility.FormatPlainTextAsHtml(SR.GetString(SR.CustomErrorFailed_Err_Desc)); }
        }

        protected override string MiscSectionTitle {
            get { return null; }
        }

        protected override string MiscSectionContent {
            get { return null; }
        }

        protected override bool ShowSourceFileInfo {
            get { return false; }
        }

        internal override bool CanBeShownToAllUsers {
            get { return true; }
        }
    }


    /*
     * This is the base class for formatter that handle errors that have an
     * associated file / line number.
     */
    internal abstract class FormatterWithFileInfo : ErrorFormatter {
        protected string _virtualPath;
        protected string _physicalPath;
        protected string _sourceCode;
        protected int _line;

        // Number of lines before and after the error lines included in the report
        private const int errorRange = 2;

        /*
         * Return the text of the error line in the source file, with a few
         * lines around it.  It is returned in HTML format.
         */
        internal static string GetSourceFileLines(string fileName, Encoding encoding, string sourceCode, int lineNumber) {

            // Don't show any source file if the user doesn't have access to it (ASURT 122430)
            if (fileName != null && !HttpRuntime.HasFilePermission(fileName))
                return SR.GetString(SR.WithFile_No_Relevant_Line);

            // 
            StringBuilder sb = new StringBuilder();

            if (lineNumber <= 0) {
                return SR.GetString(SR.WithFile_No_Relevant_Line);
            }

            TextReader reader = null;

            // Check if it's an http line pragma, from which we can get a VirtualPath
            string virtualPath = GetVirtualPathFromHttpLinePragma(fileName);

            // If we got a virtual path, open a TextReader from it
            if (virtualPath != null) {
                Stream stream = VirtualPathProvider.OpenFile(virtualPath);
                if (stream != null)
                    reader = System.Web.UI.Util.ReaderFromStream(stream, System.Web.VirtualPath.Create(virtualPath));
            }

            try {
                // Otherwise, open the physical file
                if (reader == null && fileName != null)
                    reader = new StreamReader(fileName, encoding, true, 4096);
            }
            catch { }

            if (reader == null) {
                if (sourceCode == null)
                    return SR.GetString(SR.WithFile_No_Relevant_Line);

                // Can't open the file?  Use the dynamically generated content...
                reader = new StringReader(sourceCode);
            }

            try {
                bool fFoundLine = false;

                if (IsTextRightToLeft) {
                    sb.Append(BeginLeftToRightTag);
                }

                for (int i=1; ; i++) {
                    // Get the current line from the source file
                    string sourceLine = reader.ReadLine();
                    if (sourceLine == null)
                        break;

                    // If it's the error line, make it red
                    if (i == lineNumber)
                        sb.Append("<font color=red>");

                    // Is it in the range we want to display
                    if (i >= lineNumber-errorRange && i <= lineNumber+errorRange) {
                        fFoundLine = true;
                        String linestr = i.ToString("G", CultureInfo.CurrentCulture);

                        sb.Append(SR.GetString(SR.WithFile_Line_Num, linestr));
                        if (linestr.Length < 3)
                            sb.Append(' ', 3 - linestr.Length);
                        sb.Append(HttpUtility.HtmlEncode(sourceLine));

                        if (i != lineNumber+errorRange)
                            sb.Append("\r\n");
                    }

                    if (i == lineNumber)
                        sb.Append("</font>");

                    if (i>lineNumber+errorRange)
                        break;
                }

                if (IsTextRightToLeft) {
                    sb.Append(EndLeftToRightTag);
                }

                if (!fFoundLine)
                    return SR.GetString(SR.WithFile_No_Relevant_Line);
            }
            finally {
                // Make sure we always close the reader
                reader.Close();
            }

            return sb.ToString();
        }

        private string GetSourceFileLines() {
            return GetSourceFileLines(_physicalPath, SourceFileEncoding, _sourceCode, _line);
        }

        internal FormatterWithFileInfo(string virtualPath, string physicalPath,
            string sourceCode, int line) {

            _virtualPath = virtualPath;
            _physicalPath = physicalPath;

            if (sourceCode == null && _physicalPath == null && _virtualPath != null) {

                // Make sure _virtualPath is really a virtual path.  Sometimes,
                // it can actually be a physical path, in which case we keep
                // it as is.
                if (UrlPath.IsValidVirtualPathWithoutProtocol(_virtualPath))
                    _physicalPath = HostingEnvironment.MapPath(_virtualPath);
                else
                    _physicalPath = _virtualPath;
            }

            _sourceCode = sourceCode;
            _line = line;
        }

        protected virtual Encoding SourceFileEncoding {
            get { return Encoding.Default; }
        }

        protected override string ColoredSquareContent {
            get { return GetSourceFileLines();}
        }

        protected override bool ShowSourceFileInfo {
            get { return true;}
        }

        protected override string PhysicalPath {
            get { return _physicalPath;}
        }

        protected override string VirtualPath {
            get { return _virtualPath;}
        }

        protected override int SourceFileLineNumber {
            get { return _line;}
        }
    }

    /*
     * Formatter used for compilation errors
     */
    internal class DynamicCompileErrorFormatter : ErrorFormatter {

        private const string startExpandableBlock =
            "<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('{0}')\">" +
            "{1}" +
            ":</div>\r\n" +
            "<div id=\"{0}\" style=\"display: none;\">\r\n" +
            "            <br><table width=100% bgcolor=\"#ffffcc\">\r\n" +
            "               <tr>\r\n" +
            "                  <td>\r\n" +
            "                      <code><pre>\r\n\r\n";

        private const string endExpandableBlock =
            "</pre></code>\r\n\r\n" +
            "                  </td>\r\n" +
            "               </tr>\r\n" +
            "            </table>\r\n\r\n" +
            "            \r\n\r\n" +
            "</div>\r\n";

        // Number of lines before and after the error lines included in the report
        private const int errorRange = 2;

        HttpCompileException _excep;
        private string _sourceFilePath = null;
        private int _sourceFileLineNumber = 0;
        protected bool _hideDetailedCompilerOutput = false;

        internal DynamicCompileErrorFormatter(HttpCompileException excep) {
            _excep = excep;
        }

        protected override Exception Exception {
            get { return _excep; }
        }

        protected override bool ShowSourceFileInfo {
            get {
                return false;
            }
        }

        protected override string ErrorTitle {
            get {
                return SR.GetString(SR.TmplCompilerErrorTitle);
            }
        }

        protected override string Description {
            get {
                return SR.GetString(SR.TmplCompilerErrorDesc);
            }
        }

        protected override string MiscSectionTitle {
            get {
                return SR.GetString(SR.TmplCompilerErrorSecTitle);
            }
        }

        protected override string MiscSectionContent {
            get {
                StringBuilder sb = new StringBuilder(128);

                CompilerResults results = _excep.ResultsWithoutDemand;

                // Handle fatal errors where we couldn't find an error line
                if (results.Errors.Count == 0 && results.NativeCompilerReturnValue != 0) {
                    string fatalError = SR.GetString(SR.TmplCompilerFatalError,
                                            results.NativeCompilerReturnValue.ToString("G",
                                                CultureInfo.CurrentCulture));
                    AdaptiveMiscContent.Add(fatalError);
                    sb.Append(fatalError);
                    sb.Append("<br><br>\r\n");
                }

                if (results.Errors.HasErrors) {

                    CompilerError e = _excep.FirstCompileError;

                    if (e != null) {
                        string htmlEncodedText = HttpUtility.HtmlEncode(e.ErrorNumber);
                        string adaptiveContentLine = htmlEncodedText;
                        sb.Append(htmlEncodedText);
                        // Don't show the error message in low trust (VSWhidbey 87012)
                        if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                            htmlEncodedText = HttpUtility.HtmlEncode(e.ErrorText);
                            sb.Append(": ");
                            sb.Append(htmlEncodedText);
                            adaptiveContentLine += ": " + htmlEncodedText;
                        }
                        AdaptiveMiscContent.Add(adaptiveContentLine);
                        sb.Append("<br><br>\r\n");

                        sb.Append("<b>");
                        sb.Append(SR.GetString(SR.TmplCompilerSourceSecTitle));
                        sb.Append(":</b><br><br>\r\n");
                        sb.Append("            <table width=100% bgcolor=\"#ffffcc\">\r\n");
                        sb.Append("               <tr><td>\r\n");
                        sb.Append("               ");
                        sb.Append("               </td></tr>\r\n");
                        sb.Append("               <tr>\r\n");
                        sb.Append("                  <td>\r\n");
                        sb.Append("                      <code><pre>\r\n\r\n");
                        sb.Append(FormatterWithFileInfo.GetSourceFileLines(e.FileName, Encoding.Default, _excep.SourceCodeWithoutDemand, e.Line));
                        sb.Append("</pre></code>\r\n\r\n");
                        sb.Append("                  </td>\r\n");
                        sb.Append("               </tr>\r\n");
                        sb.Append("            </table>\r\n\r\n");
                        sb.Append("            <br>\r\n\r\n");

                        // display file
                        sb.Append("            <b>");
                        sb.Append(SR.GetString(SR.TmplCompilerSourceFileTitle));
                        sb.Append(":</b> ");
                        _sourceFilePath = GetSafePath(e.FileName);
                        sb.Append(HttpUtility.HtmlEncode(_sourceFilePath));
                        sb.Append("\r\n");

                        // display number
                        TypeConverter itc = new Int32Converter();
                        sb.Append("            &nbsp;&nbsp; <b>");
                        sb.Append(SR.GetString(SR.TmplCompilerSourceFileLine));
                        sb.Append(":</b>  ");
                        _sourceFileLineNumber = e.Line;
                        sb.Append(HttpUtility.HtmlEncode(itc.ConvertToString(_sourceFileLineNumber)));
                        sb.Append("\r\n");
                        sb.Append("            <br><br>\r\n");
                    }
                }

                if (results.Errors.HasWarnings) {
                    sb.Append("<br><div class=\"expandable\" onclick=\"OnToggleTOCLevel1('warningDiv')\">");
                    sb.Append(SR.GetString(SR.TmplCompilerWarningBanner));
                    sb.Append(":</div>\r\n");
                    sb.Append("<div id=\"warningDiv\" style=\"display: none;\">\r\n");
                    foreach (CompilerError e in results.Errors) {
                        if (e.IsWarning) {
                            sb.Append("<b>");
                            sb.Append(SR.GetString(SR.TmplCompilerWarningSecTitle));
                            sb.Append(":</b> ");
                            sb.Append(HttpUtility.HtmlEncode(e.ErrorNumber));
                            // Don't show the error message in low trust (VSWhidbey 87012)
                            if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                                sb.Append(": ");
                                sb.Append(HttpUtility.HtmlEncode(e.ErrorText));
                            }
                            sb.Append("<br>\r\n");

                            sb.Append("<b>");
                            sb.Append(SR.GetString(SR.TmplCompilerSourceSecTitle));
                            sb.Append(":</b><br><br>\r\n");
                            sb.Append("            <table width=100% bgcolor=\"#ffffcc\">\r\n");
                            sb.Append("               <tr><td>\r\n");
                            sb.Append("               <b>");
                            sb.Append(HttpUtility.HtmlEncode(HttpRuntime.GetSafePath(e.FileName)));
                            sb.Append("</b>\r\n");
                            sb.Append("               </td></tr>\r\n");
                            sb.Append("               <tr>\r\n");
                            sb.Append("                  <td>\r\n");
                            sb.Append("                      <code><pre>\r\n\r\n");
                            sb.Append(FormatterWithFileInfo.GetSourceFileLines(e.FileName, Encoding.Default, _excep.SourceCodeWithoutDemand, e.Line));
                            sb.Append("</pre></code>\r\n\r\n");
                            sb.Append("                  </td>\r\n");
                            sb.Append("               </tr>\r\n");
                            sb.Append("            </table>\r\n\r\n");
                            sb.Append("            <br>\r\n\r\n");
                        }
                    }
                    sb.Append("</div>\r\n");
                }

                if (!_hideDetailedCompilerOutput) {
                    if (results.Output.Count > 0) {
                        // (Only display the compiler output in medium or higher (ASURT 126827)
                        if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                            sb.Append(String.Format(CultureInfo.CurrentCulture, startExpandableBlock, "compilerOutputDiv",
                                SR.GetString(SR.TmplCompilerCompleteOutput)));
                            foreach (string line in results.Output) {
                                sb.Append(HttpUtility.HtmlEncode(line));
                                sb.Append("\r\n");
                            }
                            sb.Append(endExpandableBlock);
                        }
                    }

                    // If we have the generated source code, display it
                    // (Only display the source in medium or higher (ASURT 128039)
                    if (_excep.SourceCodeWithoutDemand != null &&
                        HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {

                        sb.Append(String.Format(CultureInfo.CurrentCulture, startExpandableBlock, "dynamicCodeDiv",
                            SR.GetString(SR.TmplCompilerGeneratedFile)));

                        string[] sourceLines = _excep.SourceCodeWithoutDemand.Split('\n');
                        int currentLine = 1;
                        foreach (string s in sourceLines) {
                            string number = currentLine.ToString("G", CultureInfo.CurrentCulture);
                            sb.Append(SR.GetString(SR.TmplCompilerLineHeader, number));
                            if (number.Length < 5) {
                                sb.Append(' ', 5 - number.Length);
                            }
                            currentLine++;

                            sb.Append(HttpUtility.HtmlEncode(s));
                        }
                        sb.Append(endExpandableBlock);
                    }

                    sb.Append(@"
    <script type=""text/javascript"">
    function OnToggleTOCLevel1(level2ID)
    {
      var elemLevel2 = document.getElementById(level2ID);
      if (elemLevel2.style.display == 'none')
      {
        elemLevel2.style.display = '';
      }
      else {
        elemLevel2.style.display = 'none';
      }
    }
    </script>
                          ");
                }

                return sb.ToString();
            }
        }

        // This is calculated in MiscSectionContent
        protected override string PhysicalPath {
            get { return _sourceFilePath;}
        }

        protected override int SourceFileLineNumber {
            get { return _sourceFileLineNumber;}
        }
    }

    /*
     * Formatter used for parse errors
     */
    internal class ParseErrorFormatter : FormatterWithFileInfo {
        protected string _message;
        HttpParseException _excep;
        private StringCollection _adaptiveMiscContent = new StringCollection();

        internal ParseErrorFormatter(HttpParseException e, string virtualPath,
            string sourceCode, int line, string message)
        : base(virtualPath, null /*physicalPath*/, sourceCode, line) {
            _excep = e;
            _message = HttpUtility.FormatPlainTextAsHtml(message);
            _adaptiveMiscContent.Add(_message);
        }

        protected override Exception Exception {
            get { return _excep; }
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Parser_Error);}
        }

        protected override string Description {
            get { return SR.GetString(SR.Parser_Desc);}
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.Parser_Error_Message);}
        }

        protected override string MiscSectionContent {
            get { return _message;}
        }

        protected override string ColoredSquareTitle {
            get { return SR.GetString(SR.Parser_Source_Error);}
        }

        protected override StringCollection AdaptiveMiscContent {
            get { return _adaptiveMiscContent;}
        }
    }

    /*
     * Formatter used for configuration errors
     */
    internal class ConfigErrorFormatter : FormatterWithFileInfo {
        protected string _message;
        private Exception _e;
        private StringCollection _adaptiveMiscContent = new StringCollection();

        internal ConfigErrorFormatter(System.Configuration.ConfigurationException e)
        : base(null /*virtualPath*/, e.Filename, null, e.Line) {
            _e = e;
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_PRE_PROCESSING);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
            _message = HttpUtility.FormatPlainTextAsHtml(e.BareMessage);
            _adaptiveMiscContent.Add(_message);
        }

        protected override Encoding SourceFileEncoding {
            get { return Encoding.UTF8; }
        }

        protected override Exception Exception {
            get { return _e; }
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Config_Error);}
        }

        protected override string Description {
            get { return SR.GetString(SR.Config_Desc);}
        }

        protected override string MiscSectionTitle {
            get { return SR.GetString(SR.Parser_Error_Message);}
        }

        protected override string MiscSectionContent {
            get { return _message;}
        }

        protected override string ColoredSquareTitle {
            get { return SR.GetString(SR.Parser_Source_Error);}
        }

        protected override StringCollection AdaptiveMiscContent {
            get { return _adaptiveMiscContent;}
        }
    }

    /*
     * Formatter to allow user-specified description strings
     * use if showing inner-most exception message is not appropriate
     */
    internal class UseLastUnhandledErrorFormatter : UnhandledErrorFormatter {

        internal UseLastUnhandledErrorFormatter(Exception e)
            : base(e) {
        }

        internal /*public*/ override void PrepareFormatter() {
            base.PrepareFormatter();

            // use the outer-most exception instead of the inner-most in the misc section
            _initialException = Exception;
        }
    }

    internal class StaticErrorFormatterHelper {
        internal const string ChtmlErrorBeginTemplate = @"<html>
<body>
<form>
<font color=""Red"" size=""5"">{0}</font><br/>
<font color=""Maroon"">{1}</font><br/>
";
        internal const string ChtmlErrorEndTemplate = @"</form>
</body>
</html>";

        internal const string WmlErrorBeginTemplate = @"<?xml version='1.0'?>
<!DOCTYPE wml PUBLIC '-//WAPFORUM//DTD WML 1.1//EN' 'http://www.wapforum.org/DTD/wml_1.1.xml'><wml><head>
<meta http-equiv=""Cache-Control"" content=""max-age=0"" forua=""true""/>
</head>
<card>
<p>
<b><big>{0}</big></b><br/>
<b><i>{1}</i></b><br/>
";
        internal const string WmlErrorEndTemplate = @"</p>
</card>
</wml>
";

        internal const string XhtmlErrorBeginTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE html PUBLIC ""-//WAPFORUM//DTD XHTML Mobile 1.0//EN"" ""http://www.wapforum.org/DTD/xhtml-mobile10.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
<title></title>
</head>
<body>
<form>
<div>
<span style=""color:Red;font-size:Large;font-weight:bold;"">{0}</span><br/>
<span style=""color:Maroon;font-weight:bold;font-style:italic;"">{1}</span><br/>
";
        internal const string XhtmlErrorEndTemplate = @"</div>
</form>
</body>
</html>";
        internal const string Break = "<br/>\r\n";
    }
}
