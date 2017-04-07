
//--------------------------------------------------------------------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------------------------------
// @owner=alexgor, deliant
//==========================================================================================================================
//  File:		ChartHttpHandler.cs
//
//  Namespace:	Microsoft.Reporting.Chart.WebForms
//
//	Classes:	ChartHttpHandler
//
//  Purpose:	ChartHttpHandler is a static class which is responsible to handle with 
//              chart images, interactive images, scripts and other resources.
//              
//              
//	Reviewed:	DT
//	Reviewed:	deliant on 4/14/2011 			
//              MSRC#10470, VSTS#941768 http://vstfdevdiv:8080/web/wi.aspx?id=941768
//              Please review information associated with MSRC#10470 before making any changes to this file.
//              - Fixes:
//                  - Fixed Directory Traversal/Arbitrary File Read, Delete with malformed image key.
//                  - Honor HttpContext.Current.Trace.IsEnabled when generate and deliver chart trace info.
//                  - Handle empty guid parameter ("?g=") as invalid when enforcing privacy.
//                  - Replaced the privacy byte array comparison with custom check (otherwise posible EOS marker can return 0 length string).
//                  - Added fixed string to session key to avoid direct session access.
//
//   Added:  deliant on 4/48/2011 fix for VSTS: 3593 - ASP.Net chart under web farm exhibit fast performace degradation
//           Summary: Under large web farm setup ( ~16 processes and up) chart control image handler 
//                    soon starts to show performace degradation up to denial of service, when a file system is used as storage.
//            Issues: 
//                  - The image files in count over 2000 in one single folder causes exponentially growing slow response, 
//                    especially on the remote server. The fix places the Image files  in separate subfolders for each process. 
//                  - Private protection seeks and read several times in the image file istead reading the image at once 
//                    and then check for privacy marker.  Separate small network reads are expensive.
//                  - Due missing lock in initialization stage the chart lock files number can grow more that process max 
//                    number which can create abandon chart image files
//==========================================================================================================================

#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Web.Caching;
using System.Collections;
using System.Web.Configuration;
using System.Resources;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.Hosting;
using System.Web.SessionState;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Security;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Web.UI.WebControls;

#endregion //Namespaces

namespace System.Web.UI.DataVisualization.Charting
{
    /// <summary>
    /// ChartHttpHandler processes HTTP Web requests using, handles chart images, scripts and other resources.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ChartHttpHandler : Page, IRequiresSessionState, IHttpHandler
    {

        #region Fields

        // flag that indicates whether this chart handler is installed
        private static bool _installed = false;
        
        // flag that indicates whether this chart handler is installed
        private static bool _installChecked = false;

        // storage settings
        private static ChartHttpHandlerSettings _parameters = null;
        

        // machine hash key which is part in chart image file name
        private static string _machineHash = "_" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + "_";

        // web gadren controller file. stays locked diring process lifetime.
		private static FileStream _controllerFileStream = null;
		private static string _controllerDirectory = null;
        private static object _initHandlerLock = new object();
        // used for storing Guid key in context;
        internal static string ContextGuidKey = "{89FA5660-BD13-4f1b-8C7C-355CEC92CC7E}";

        // web gadren controller file. stays locked diring process lifetime.
        private const string handlerCheckQry = "check";

        #endregion //Fields

        #region Consts
        
        internal const string ChartHttpHandlerName = "ChartImg.axd";
        internal const string ChartHttpHandlerAppSection = "ChartImageHandler";
        internal const string DefaultConfigSettings = @"storage=file;timeout=20;dir=c:\TempImageFiles\;";
        internal const string WebDevServerUseConfigSettings = "WebDevServerUseConfigSettings";
        #endregion //Consts

        #region Constructors

        /// <summary>
        /// Ensures that the handler is initialized.
        /// </summary>
        /// <param name="hardCheck">if set to <c>true</c> then will be thrown all excepitons.</param>
        private static void EnsureInitialized(bool hardCheck)
        {
            if (_installChecked)
            {
                return;
            }
            lock (_initHandlerLock)
            {
                if (_installChecked)
                {
                    return;
                }
                if (HttpContext.Current != null)
                {
                    try
                    {
                        using (TextWriter w = new StringWriter(CultureInfo.InvariantCulture))
                        {
                            HttpContext.Current.Server.Execute(ChartHttpHandlerName + "?" + handlerCheckQry + "=0", w);
                        }
                        _installed = true;
                    }
                    catch (HttpException)
                    {
                        if (hardCheck) throw;
                    }
                    catch (SecurityException)
                    {
                        // under minimal configuration we assume that the hanlder is installed if app settings are present.
                        _installed = !String.IsNullOrEmpty(WebConfigurationManager.AppSettings[ChartHttpHandlerAppSection]);
                    }
                }
                if (_installed || hardCheck)
                {
                    InitializeControllerFile();
                }
                _installChecked = true;
            }
        }
        
        /// <summary>
        /// Initializes the storage settings
        /// </summary>
        //static ChartHttpHandler()
        private static ChartHttpHandlerSettings InitializeParameters()
        {

            ChartHttpHandlerSettings result = new ChartHttpHandlerSettings();
            if (HttpContext.Current != null)
            {
                // Read settings from config; use DefaultConfigSettings in case when setting is not found
                string configSettings = WebConfigurationManager.AppSettings[ChartHttpHandlerAppSection];
                if (String.IsNullOrEmpty(configSettings))
                    configSettings = DefaultConfigSettings;
                
                result = new ChartHttpHandlerSettings(configSettings);
            }
            else
            {
                result.PrepareDesignTime();
            }

            return result;
        }

        private static void ResetControllerStream()
        {
            if (_controllerFileStream != null)
                _controllerFileStream.Dispose();
            _controllerFileStream = null;
            _controllerDirectory = null;
        }

        private static void InitializeControllerFile()
        {
            if (Settings.StorageType == ChartHttpHandlerStorageType.File && _controllerFileStream == null)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes("chart io controller file");
                // 2048 processes max.
                for (Int32 i = 0; i < 2048; i++)
                {
                    try
                    {
                        ResetControllerStream();
                        string controllerFileName = String.Format(CultureInfo.InvariantCulture, "{0}msc_cntr_{1}.txt", Settings.Directory, i);
                        _controllerDirectory = String.Format(CultureInfo.InvariantCulture, "charts_{0}", i);
                        _controllerFileStream = new System.IO.FileStream(controllerFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        _controllerFileStream.Lock(0, data.Length);
                        _controllerFileStream.Write(data, 0, data.Length);
                        _machineHash = "_" + i + "_";
                        if (!Directory.Exists(Settings.Directory + _controllerDirectory))
                        {
                            Directory.CreateDirectory(Settings.Directory + _controllerDirectory);
                        }
                        else
                        {
                            TimeSpan lastWrite = DateTime.Now - Directory.GetLastWriteTime(Settings.Directory + _controllerDirectory);
                            if (lastWrite.Seconds < Settings.Timeout.Seconds)
                            {
                                continue;
                            }
                        }
                        return;
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                    catch (Exception)
                    {
                        ResetControllerStream();
                        throw;
                    }
                }
                ResetControllerStream(); 
                throw new UnauthorizedAccessException(SR.ExceptionHttpHandlerTempDirectoryUnaccesible(Settings.Directory));
            }
        }
        
        #endregion //Constructors

        #region Methods

        #region ChartImage

        /// <summary>
        /// Processes the saved image.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>false if the image cannot be processed</returns>
        private static bool ProcessSavedChartImage(HttpContext context)
        {
            // image delivery doesn't depend if handler is intitilzed or not.
            String key = context.Request["i"];
            CurrentGuidKey = context.Request["g"];
            IChartStorageHandler handler = GetHandler();
            try
            {
                Byte[] data = handler.Load(KeyToUnc(key));
                if (data != null && data.Length > 0)
                {
                    context.Response.Charset = "";
                    context.Response.ContentType = GetMime(key);
                    context.Response.BinaryWrite(data);
                    Diagnostics.TraceWrite(SR.DiagnosticChartImageServed(key), null);
                    if (Settings.StorageType == ChartHttpHandlerStorageType.Session || Settings.DeleteAfterServicing)
                    {
                        handler.Delete(key);
                        Diagnostics.TraceWrite(SR.DiagnosticChartImageDeleted(key), null);
                    }
                    return true;
                }
                if (!(handler is DefaultImageHandler))
                {
                    // the default handler will write more detailed message
                    Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailNotFound), null);
                }
            }
            catch (NullReferenceException nre)
            {
                Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, String.Empty), nre);
                throw;
            }
            catch (IOException ioe)
            {
                Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, String.Empty), ioe);
                throw;
            }
            catch (SecurityException se)
            {
                Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, String.Empty), se);
                throw;
            }
            return false;
        }

        #endregion //ChartImage

        #region Utilities


        /// <summary>
        /// Gets or sets the current GUID key.
        /// </summary>
        /// <value>The current GUID key.</value>
        internal static string CurrentGuidKey
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return (string)HttpContext.Current.Items[ContextGuidKey];
                }
                return String.Empty;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        HttpContext.Current.Items.Remove(ContextGuidKey);
                    }
                    else
                    {
                        HttpContext.Current.Items[ContextGuidKey] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the chart image handler interface reference.
        /// </summary>
        /// <returns></returns>
        private static IChartStorageHandler GetHandler()
        {
            return ChartHttpHandler.Settings.GetHandler();
        }

        /// <summary>
        /// Determines whether this instance is installed.
        /// </summary>
        internal static void EnsureInstalled()
        {
            EnsureInitialized(true);
            EnsureSessionIsClean();
        }

        /// <summary>
        /// Gets the handler URL.
        /// </summary>
        /// <returns></returns>
        private static String GetHandlerUrl()
        {
            // the handler have to be executed in current cxecution path in order to get proper user identity
            String appDir = Path.GetDirectoryName(HttpContext.Current.Request.CurrentExecutionFilePath ?? "").Replace("\\","/");
            if (!appDir.EndsWith("/", StringComparison.Ordinal))
            {
                appDir += "/";
            }
            return appDir + ChartHttpHandlerName + "?";
		}


        /// <summary>
        /// Gets the MIME type by resource url.
        /// </summary>
        /// <param name="resourceUrl">The resource URL.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308",
            Justification = "No security decision is being made on the ToLowerInvariant() call. It is being used to ensure the file extension is lowercase")]
        private static String GetMime(String resourceUrl)
        {
            String ext = Path.GetExtension(resourceUrl);

            ext = ext.ToLowerInvariant();

            if (ext == ".js")
            {
                return "text/javascript";
            }
            else if (ext == ".htm")
            {
                return "text/html";
            }
            else if (".css,.html,.xml".IndexOf(ext, StringComparison.Ordinal) != -1)
            {
                return "text/" + ext.Substring(1);
            }
            else if (".jpg;.jpeg;.gif;.png;.emf".IndexOf(ext, StringComparison.Ordinal) != -1)
            {
                string fmt = ext.Substring(1).Replace("jpg", "jpeg");
                return "image/" + fmt;
            }
            return "text/plain";
        }

        /// <summary>
        /// Generates the chart image file name (key).
        /// </summary>
        /// <param name="ext">The ext.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private static String GenerateKey(String ext)
        {
            String fmtKey = "chart" + _machineHash + "{0}." + ext;
            RingTimeTracker rt = RingTimeTrackerFactory.GetRingTracker(fmtKey);
			if (!String.IsNullOrEmpty(_controllerDirectory) && String.IsNullOrEmpty(Settings.FolderName))
			{
				return _controllerDirectory + @"\" + rt.GetNextKey();
			}
            return Settings.FolderName + rt.GetNextKey();
        }

		private static String KeyToUnc(String key)
		{
			if (!String.IsNullOrEmpty(key))
			{
				return key.Replace("/", @"\");
			}
			return key;
		}
		
		private static String KeyFromUnc(String key)
		{
			if (!String.IsNullOrEmpty(key))
			{
				return key.Replace(@"\", "/");
			}
			return key;
		}

        /// <summary>
        /// Gets a URL by specified request query, file key.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="fileKey">The file key.</param>
        /// <param name="currentGuid">The current GUID.</param>
        /// <returns></returns>
        private static String GetUrl(String query, String fileKey, string currentGuid)
        {
			return GetHandlerUrl() + query + "=" + KeyFromUnc(fileKey) + "&g=" + currentGuid;
        }

        /// <summary>
        /// Gets the image url.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="imageExt">The image extention.</param>
        /// <returns>Generated the image source URL</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308", 
            Justification="No security decision is being made on the ToLowerInvariant() call. It is being used to ensure the file extension is lowercase")]
        internal static String GetChartImageUrl(MemoryStream stream, String imageExt)
        {
            EnsureInitialized(true);
            // generates new guid
            string guidKey = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            // set new guid in context
            CurrentGuidKey = guidKey;
            
            Int32 tryCounts = 10;
            while (tryCounts > 0)
            {
                tryCounts--;
                try
                {
                    String key = GenerateKey(imageExt.ToLowerInvariant());
                    IChartStorageHandler handler = Settings.GetHandler();
                    handler.Save(key, stream.ToArray());
                    if (!(handler is DefaultImageHandler))
                    {
                        Diagnostics.TraceWrite(SR.DiagnosticChartImageSaved(key), null);
                    }
                    Settings.FolderName = String.Empty;
                    // clear guid so is not accessable out of the scope;
                    CurrentGuidKey = String.Empty;
                    return ChartHttpHandler.GetUrl("i", key, guidKey);
                }
                catch (IOException) { }
                catch { throw;}
            }
            throw new IOException(SR.ExceptionHttpHandlerCanNotSave);
        }

        /// <summary>
        /// Ensures the session is clean.
        /// </summary>
        private static void EnsureSessionIsClean()
        {
            if (!_installed) return;
            if (Settings.StorageType == ChartHttpHandlerStorageType.Session)
            {
                IChartStorageHandler handler = ChartHttpHandler.Settings.GetHandler();
                foreach (RingTimeTracker tracker in RingTimeTrackerFactory.OpenedRingTimeTrackers())
                {
                        tracker.ForEach(true, delegate(RingItem item)
                        {
                            if (item.InUse && String.CompareOrdinal(Settings.ReadSessionKey(), item.SessionID) == 0)
                            {
                                handler.Delete(tracker.GetKey(item));
                                Diagnostics.TraceWrite(SR.DiagnosticChartImageDeleted(tracker.GetKey(item)), null);
                                item.InUse = false;
                            }
                        }
                    );
                }
            }
        }
        #endregion //Utilities

        #region Diagnostics
        
        private static void DiagnosticWriteAll(HttpContext context)
        {
            HtmlTextWriter writer;
            using (TextWriter w = new StringWriter(CultureInfo.CurrentCulture))
            {
                
                if (context.Request.Browser != null)
                    writer = context.Request.Browser.CreateHtmlTextWriter(w);
                else
                    writer = new Html32TextWriter(w);
                writer.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n\r<html xmlns=\"http://www.w3.org/1999/xhtml\" >\n\r");
                writer.Write("<head>\r\n");
                writer.Write("<style type=\"text/css\">\r\n body, span, table, td, th, div, caption {font-family: Tahoma, Arial, Helvetica, sans-serif;font-size: 10pt;} caption {background-color:Black; color: White; font-weight:bold; padding: 4px; text-align:left; } \r\n</style>\r\n");
                writer.Write("</head>\r\n<body style=\"width:978px\">\r\n");
                writer.Write("<h2>" + SR.DiagnosticHeader + "</h2>\r\n<hr/><br/>\n\r");
                DiagnosticWriteSettings(writer);
                writer.Write("<hr/>");
                DiagnosticWriteActivity(writer);
                writer.Write("<br/><hr/>\n\r<span>");
                try
                {
                    writer.Write(typeof(Chart).AssemblyQualifiedName);
                }
                catch ( SecurityException ) {}
                writer.Write("</span></body>\r\n</html>\r\n");
                context.Response.Write(w.ToString());
            }
        }

        private static void DiagnosticWriteSettings(HtmlTextWriter writer)
        {
            writer.Write("<h4>" + SR.DiagnosticSettingsConfig(WebConfigurationManager.AppSettings[ChartHttpHandlerAppSection]) + "</h4>");
            GridView grid = CreateGridView( true);
            grid.Caption = SR.DiagnosticSettingsHeader;
            BoundField field = new BoundField();
            field.DataField = "Key";
            field.HeaderText = SR.DiagnosticSettingsKey;
            field.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            grid.Columns.Add(field);

            field = new BoundField();
            field.DataField = "Value";
            field.HeaderText = SR.DiagnosticSettingsValue;
            field.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;

            grid.Columns.Add(field);
            Dictionary<String, String> settings = new Dictionary<String, String>();
            
            settings.Add("StorageType", Settings.StorageType.ToString());
            settings.Add("TimeOut", Settings.Timeout.ToString());
            if (Settings.StorageType == ChartHttpHandlerStorageType.File)
            {
                settings.Add("Directory", Settings.Directory);
            }
            settings.Add("DeleteAfterServicing", Settings.DeleteAfterServicing.ToString());
            settings.Add("PrivateImages", Settings.PrivateImages.ToString());
            settings.Add("ImageOwnerKey", Settings.ImageOwnerKey.ToString());
            settings.Add("CustomHandlerName", Settings.CustomHandlerName);
            settings.Add(ChartHttpHandler.WebDevServerUseConfigSettings, String.Equals(Settings[ChartHttpHandler.WebDevServerUseConfigSettings], "true", StringComparison.OrdinalIgnoreCase).ToString());

            grid.DataSource = settings;
            grid.DataBind();
            
            grid.RenderControl(writer);
            
        }

        private static void DiagnosticWriteActivity(HtmlTextWriter writer)
        {
            GridView grid = CreateGridView( true);
            grid.Caption = SR.DiagnosticActivityHeader;
            BoundField field = new BoundField();
            field.DataField = "DateStamp";
            field.ItemStyle.VerticalAlign = VerticalAlign.Top;
            field.HeaderText = SR.DiagnosticActivityTime;
            field.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            field.HeaderStyle.Width = 150;
            grid.Columns.Add(field);

            field = new BoundField();
            field.DataField = "Url";
            field.HeaderText = SR.DiagnosticActivityMessage;
            field.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;

            grid.Columns.Add(field);

            grid.RowDataBound += new GridViewRowEventHandler(DiagnosticActivityGrid_RowDataBound);

            grid.DataSource = Diagnostics.Messages;
            grid.DataBind();
            grid.RenderControl(writer);

        }

        static void DiagnosticActivityGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Diagnostics.HandlerPageTraceInfo currentInfo = (Diagnostics.HandlerPageTraceInfo)e.Row.DataItem;
                TableCell cell = e.Row.Cells[1];

                cell.Controls.Add(new Label() { Text = currentInfo.Verb + "," + currentInfo.Url });

                GridView grid = CreateGridView(false);
                grid.Style[HtmlTextWriterStyle.MarginLeft] = "20px";

                grid.ShowHeader = false;
                
                BoundField field = new BoundField();
                field.DataField = "Text";
                field.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                grid.Columns.Add(field);

                grid.DataSource = currentInfo.Events;
                grid.DataBind();
                cell.Controls.Add(grid);
            }
        }

        private static GridView CreateGridView(bool withAlternateStyle)
        {
            GridView result = new GridView();

            result.AutoGenerateColumns = false;
            result.CellPadding = 4;
            result.Font.Names = new string[] { "Tahoma", "Ariel" };
            result.Font.Size = new FontUnit(10, UnitType.Point);
            result.BorderWidth = 0;
            result.GridLines = GridLines.None;
            result.Width = new Unit(100, UnitType.Percentage);

            if (withAlternateStyle)
            {
                result.AlternatingRowStyle.BackColor = Color.White;
                result.RowStyle.BackColor = ColorTranslator.FromHtml("#efefef");
                result.RowStyle.ForeColor = Color.Black;
                result.AlternatingRowStyle.ForeColor = Color.Black;
            }

            result.HeaderStyle.BackColor = Color.Gray;
            result.HeaderStyle.ForeColor = Color.White;
            result.HeaderStyle.Font.Bold = true;
            return result;
        }

        #endregion //Diagnostics

        #endregion //Methods

        #region Properties

        /// <summary>
        /// Gets the chart image storage settings registred in web.config file under ChartHttpHandler key.
        /// </summary>
        /// <value>The settings.</value>
        public static ChartHttpHandlerSettings Settings
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = InitializeParameters();
                }
                return _parameters;
            }
        }

        #endregion //Properties

        #region IHttpHandler Members

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Web.UI.Page"/> object can be reused.
        /// </summary>
        /// <value></value>
        /// <returns>false in all cases. </returns>
        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            if (context.Request["i"] != null && ProcessSavedChartImage(context))
            {
                return;
            }
            else if (context.Request["trace"] != null && Diagnostics.IsTraceEnabled)
            {
                DiagnosticWriteAll(context);
                return;
            }
            else if (context.Request[handlerCheckQry] != null)
            {
                // handler execute test - returns no errors.
                return;
            }
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = SR.ExceptionHttpHandlerImageNotFound;
        }

        #endregion

    }

    #region Enumerations

    /// <summary>
    /// Determines chart image storage medium
    /// </summary>
    public enum ChartHttpHandlerStorageType
    {
        /// <summary>
        /// Static into application memory
        /// </summary>
        InProcess,

        /// <summary>
        /// File system
        /// </summary>
        File,
        /// <summary>
        /// Using session as storage
        /// </summary>
        Session

    }
    /// <summary>
    /// Determines the image owner key for privacy protection.
    /// </summary>
    internal enum ImageOwnerKeyType
    {
        /// <summary>
        /// No privacy protection.
        /// </summary>
        None,
        /// <summary>
        /// The key will be automatically determined.
        /// </summary>
        Auto,
        /// <summary>
        /// The user name will be used as key.
        /// </summary>
        UserID,
        /// <summary>
        /// The AnonymousID will be used as key.
        /// </summary>
        AnonymousID,
        /// <summary>
        /// The SessionID will be used as key.
        /// </summary>
        SessionID
    }

    #endregion

    #region IChartStorageHandler interface

    /// <summary>
    /// Defines methods to manage rendered chart images in a storage.
    /// </summary>
    public interface IChartStorageHandler
    {
        /// <summary>
        /// Saves the data into external medium.
        /// </summary>
        /// <param name="key">Index key.</param>
        /// <param name="data">Image data.</param>
        void Save(String key, Byte[] data);


        /// <summary>
        /// Loads the data from external medium.
        /// </summary>
        /// <param name="key">Index key.</param>
        /// <returns>A byte array with image data</returns>
        Byte[] Load(String key);


        /// <summary>
        /// Deletes the data from external medium.
        /// </summary>
        /// <param name="key">Index key.</param>
        void Delete(String key);

        /// <summary>
        /// Checks for existence of data under specified key.
        /// </summary>
        /// <param name="key">Index key.</param>
        /// <returns>True if data exists under specified key</returns>
        bool Exists(String key);
    }

    #endregion

    #region ChartHttpHandlerSettings Class

    /// <summary>
    /// Enables access to the chart image storage settings.
    /// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ChartHttpHandlerSettings
    {
        #region Fields

        private StorageSettingsCollection _ssCollection = new StorageSettingsCollection();
        
        private string _sesionKey   = "chartKey-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

        #endregion //Fields

        #region Properties

        private ChartHttpHandlerStorageType _chartImageStorage = ChartHttpHandlerStorageType.File;

        /// <summary>
        /// Gets or sets the chart image storage type.
        /// </summary>
        /// <value>The chart image storage.</value>
        public ChartHttpHandlerStorageType StorageType
        {
            get { return _chartImageStorage; }
            set { _chartImageStorage = value; }
        }

        private TimeSpan _timeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private String _url = "~/";
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public String Url
        {
            get { return _url; }
            set { _url = value; }
        }

        private String _directory = String.Empty;
        /// <summary>
        /// Gets or sets the directory.
        /// </summary>
        /// <value>The directory.</value>
        public String Directory
        {
            get { return _directory; }
            set { _directory = value; }
        }

        private const String _folderKeyName = "{5FF3B636-70BA-4180-B7C5-FDD77D8FA525}";
        /// <summary>
        /// Gets or sets the folder which will be used for storing images under <see cref="Directory"/>.
        /// </summary>
        /// <value>The folder name.</value>
        public String FolderName
        {
            get 
            {
                if (HttpContext.Current != null && HttpContext.Current.Items.Contains(_folderKeyName))
                {
                    return (string)HttpContext.Current.Items[_folderKeyName];
                }
                return String.Empty; 
            }
            set 
            { 
                if (!String.IsNullOrEmpty(value))
                {
                    if (!(value.EndsWith("/", StringComparison.Ordinal) || value.EndsWith("\\", StringComparison.Ordinal)))
                    {
                        value += "\\";
                    }
                    this.ValidateUri(value);
                }
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[_folderKeyName] = value;
                }
            }
        }

        internal void ValidateUri(string key)
        {
            if (this.StorageType == ChartHttpHandlerStorageType.File)
            {
                FileInfo fi = new FileInfo(this.Directory + key);
                Uri directory = new Uri(this.Directory);
                Uri combinedDirectory = new Uri(fi.FullName); 
                if (directory.IsBaseOf(combinedDirectory))
                {
                    // it is fine.
                    return;
                }
                throw new UnauthorizedAccessException(SR.ExceptionHttpHandlerInvalidLocation);  
            }
        }

        private String _customHandlerName = typeof(DefaultImageHandler).FullName;
        /// <summary>
        /// Gets or sets the name of the custom handler.
        /// </summary>
        /// <value>The name of the custom handler.</value>
        public String CustomHandlerName
        {
            get { return _customHandlerName; }
            set { _customHandlerName = value; }
        }


        private Type _customHandlerType = null;
        /// <summary>
        /// Gets the type of the custom handler.
        /// </summary>
        /// <value>The type of the custom handler.</value>
        public Type HandlerType
        {
            get
            {
                if (this._customHandlerType == null)
                {
                    this._customHandlerType = Type.GetType(this.CustomHandlerName, true);
                }
                return this._customHandlerType;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the handler will utilize private images.
        /// </summary>
        /// <value><c>true</c> if the handler will utilize private images; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When PrivateImages is set the handler will not return images out of session scope and 
        /// the client will not be able to download somebody else's images. This is default behavoiur.
        /// </remarks>
        public bool PrivateImages
        {
            get
            {
                return ImageOwnerKey != ImageOwnerKeyType.None;
            }
        }



        /// <summary>
        /// Gets a settings parameter with the specified name registred in web.config file under ChartHttpHandler key.
        /// </summary>
        /// <value></value>
        public string this[string name]
        {
            get
            {
                return this._ssCollection[name];
            }
        }
        
        #endregion //Properties

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:StorageSettings"/> class.
        /// </summary>
        internal ChartHttpHandlerSettings()
        {
            ImageOwnerKey = ImageOwnerKeyType.Auto;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ChartHttpHandlerParameters"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        internal ChartHttpHandlerSettings(String parameters) : this()
        {
            this.ParseParams(parameters);
            this._ssCollection.SetReadOnly(true);
        }

        #endregion //Constructors

        #region Methods

        private ConstructorInfo _handlerConstructor = null;
        IChartStorageHandler _storageHandler = null;
        /// <summary>
        /// Creates the handler instance.
        /// </summary>
        /// <returns></returns>
        internal IChartStorageHandler GetHandler()
        {
            if (_storageHandler == null)
            {
                if (this._handlerConstructor == null)
                {
                    this.InspectHandlerLoader();
                }
                _storageHandler = this._handlerConstructor.Invoke(new object[0]) as IChartStorageHandler;
            }
            return _storageHandler;
        }

        /// <summary>
        /// Inspects the handler if it is valid.
        /// </summary>
        private void InspectHandlerLoader()
        {
            this._handlerConstructor = this.HandlerType.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[0],
                new ParameterModifier[0]);
            if (this._handlerConstructor == null)
            {
                throw new InvalidOperationException( SR.ExceptionHttpHandlerCanNotLoadType( this.HandlerType.FullName ));
            }
            if (this.GetHandler() == null)
            {
                throw new InvalidOperationException(SR.ExceptionHttpHandlerImageHandlerInterfaceUnsupported(ChartHttpHandler.Settings.HandlerType.FullName));
            }
        }

        /// <summary>
        /// Parses the params from web.config file key.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        private void ParseParams(String parameters)
        {
            if (!String.IsNullOrEmpty(parameters))
            {

                String[] pairs = parameters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < pairs.Length; index++)
                {
                    String item = pairs[index].Trim();
                    int eqPositon = item.IndexOf('=');
                    if (eqPositon != -1)
                    {
                        String name = item.Substring(0, eqPositon).Trim();
                        String value = item.Substring(eqPositon + 1).Trim();
                        this._ssCollection.Add(name, value);
                        if (name.StartsWith("stor", StringComparison.OrdinalIgnoreCase))
                        {
                            if (value.StartsWith("inproc", StringComparison.OrdinalIgnoreCase) || value.StartsWith("memory", StringComparison.OrdinalIgnoreCase))
                            {
                                this.StorageType = ChartHttpHandlerStorageType.InProcess;
                            }
                            else if (value.StartsWith("file", StringComparison.OrdinalIgnoreCase))
                            {
                                this.StorageType = ChartHttpHandlerStorageType.File;
                            }
                            else if (value.StartsWith("session", StringComparison.OrdinalIgnoreCase))
                            {
                                this.StorageType = ChartHttpHandlerStorageType.Session;
                            }
                            else
                            {
                                throw new System.Configuration.SettingsPropertyWrongTypeException(SR.ExceptionHttpHandlerParameterUnknown(name, value));
                            }
                        }
                        else if (name.StartsWith("url", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!value.EndsWith("/", StringComparison.Ordinal))
                            {
                                value += "/";
                            }
                            this.Url = value;
                        }
                        else if (name.StartsWith("dir", StringComparison.OrdinalIgnoreCase))
                        {
                            this.Directory = value;
                        }
                        else if (name.StartsWith("time", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                int seconds = Int32.Parse(value, CultureInfo.InvariantCulture);
                                if (seconds < -1)
                                {
                                    throw new System.Configuration.SettingsPropertyWrongTypeException(SR.ExceptionHttpHandlerValueInvalid);
                                }
                                if (seconds == -1)
                                {
                                    this.Timeout = TimeSpan.MaxValue;
                                }
                                else
                                {
                                    this.Timeout = TimeSpan.FromSeconds(seconds);
                                }
                            }
                            catch (Exception exception)
                            {
                                throw new System.Configuration.SettingsPropertyWrongTypeException(SR.ExceptionHttpHandlerTimeoutParameterInvalid, exception);
                            }
                        }
                        else if (name.StartsWith("handler", StringComparison.OrdinalIgnoreCase))
                        {
                            this.CustomHandlerName = value;
                        }
                        else if (name.StartsWith("privateImages", StringComparison.OrdinalIgnoreCase))
                        {
                            bool privateImg = true;
                            if (Boolean.TryParse(value, out privateImg) && !privateImg)
                            {
                                ImageOwnerKey = ImageOwnerKeyType.None;
                            }
                        }
                        else if (name.StartsWith("imageOwnerKey", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                ImageOwnerKey = (ImageOwnerKeyType)Enum.Parse(typeof(ImageOwnerKeyType), value, true);
                            }
                            catch (ArgumentException)
                            {
                                throw new System.Configuration.SettingsPropertyWrongTypeException(SR.ExceptionHttpHandlerParameterInvalid(name, value));
                            }
                        }

                    }
                }
            }
            this.Inspect();
        }

        /// <summary>
        /// Determines whether web dev server is active.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if web dev server active; otherwise, <c>false</c>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "GetCurrentProcess will fail if there is no access. This is by design. ")]
        // VSTS: 5176	Security annotation violations in System.Web.DataVisualization.dll
        [SecuritySafeCritical]
        private static bool IsWebDevActive()
        {
            try
            {
                Process process = Process.GetCurrentProcess();
                if (process.ProcessName.StartsWith("WebDev.WebServer", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (process.ProcessName.StartsWith("ii----press", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (SecurityException)
            {
            }
            return false;
        }
        /// <summary>
        /// Inspects and validates this instance after loading params.
        /// </summary>
        internal void Inspect()
        {
            switch (this.StorageType)
            {
                case ChartHttpHandlerStorageType.InProcess:

                    break;

                case ChartHttpHandlerStorageType.File:

                    if (IsWebDevActive() && !( String.Compare(this[ChartHttpHandler.WebDevServerUseConfigSettings], "true", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        this.StorageType = ChartHttpHandlerStorageType.InProcess;
                        break;
                    }

                    if (String.IsNullOrEmpty(this.Url))
                    {
                        throw new ArgumentException(SR.ExceptionHttpHandlerUrlMissing);
                    }
                    
                    String fileDirectory = this.Directory;
                    if (String.IsNullOrEmpty(fileDirectory))
                    {
                        try
                        {
                            fileDirectory = HttpContext.Current.Server.MapPath(this.Url);
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException(SR.ExceptionHttpHandlerUrlInvalid, exception);
                        }
                    }
                    fileDirectory = fileDirectory.Replace("/", "\\");
                    if (!fileDirectory.EndsWith("\\", StringComparison.Ordinal))
                    {
                        fileDirectory += "\\";
                    }

                    if (!System.IO.Directory.Exists(fileDirectory))
                    {
                        throw new DirectoryNotFoundException(SR.ExceptionHttpHandlerTempDirectoryInvalid(fileDirectory));
                    }
                    Exception thrown = null;
                    try
                    {
                        String testFileName = fileDirectory + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                        using (FileStream fileStream = File.Create(testFileName)) { }
                        File.Delete(testFileName);

                    }
                    catch (DirectoryNotFoundException exception)
                    {
                        thrown = exception;
                    }
                    catch (NotSupportedException exception)
                    {
                        thrown = exception;
                    }
                    catch (PathTooLongException exception)
                    {
                        thrown = exception;
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        thrown = exception;
                    }

                    if (thrown != null)
                    {
                        throw new UnauthorizedAccessException(SR.ExceptionHttpHandlerTempDirectoryUnaccesible(fileDirectory));
                    }

                    this.Directory = fileDirectory;
                    break;


            }
            if (!String.IsNullOrEmpty(this.CustomHandlerName))
            {
                this.InspectHandlerLoader();
            }
        }

        /// <summary>
        /// Prepares the design time params.
        /// </summary>
        internal void PrepareDesignTime()
        {
            this.StorageType = ChartHttpHandlerStorageType.File;
            this.Timeout = TimeSpan.FromSeconds(3); ;
            this.Url = Path.GetTempPath();
            this.Directory = Path.GetTempPath();
        }

        internal string ReadSessionKey()
        {
            if (HttpContext.Current.Session != null)
            {
                // initialize session (if is empty any postsequent request will have different id);
                if (HttpContext.Current.Session.IsNewSession)
                {
                    if (HttpContext.Current.Session.IsReadOnly)
                    {
                        return string.Empty;
                    }
                    HttpContext.Current.Session[this._sesionKey] = 0;
                }
                return HttpContext.Current.Session.SessionID;
            }
            return String.Empty;
        }
        
        internal string GetPrivacyKey( out ImageOwnerKeyType keyType )
        {
            if (ImageOwnerKey == ImageOwnerKeyType.None)
            {
                keyType = ImageOwnerKeyType.None;                
                return String.Empty;
            }
            if (HttpContext.Current != null)
            {
                switch (ImageOwnerKey)
                {
                    case ImageOwnerKeyType.Auto:
                        if (HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            keyType = ImageOwnerKeyType.UserID;
                            return HttpContext.Current.User.Identity.Name;
                        }
                        if (!String.IsNullOrEmpty(HttpContext.Current.Request.AnonymousID))
                        {
                            keyType = ImageOwnerKeyType.AnonymousID;
                            return HttpContext.Current.Request.AnonymousID;
                        }
                        string sessionId = ReadSessionKey();
                        keyType = String.IsNullOrEmpty(sessionId) ? ImageOwnerKeyType.None : ImageOwnerKeyType.SessionID;
                        return sessionId;

                    case ImageOwnerKeyType.UserID:
                        if (!HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            throw new InvalidOperationException(SR.ExceptionHttpHandlerPrivacyKeyInvalid("ImageOwnerKey", ImageOwnerKey.ToString()));
                        }
                        keyType = ImageOwnerKeyType.UserID;
                        return HttpContext.Current.User.Identity.Name;

                    case ImageOwnerKeyType.AnonymousID:
                        if (String.IsNullOrEmpty(HttpContext.Current.Request.AnonymousID))
                        {
                            throw new InvalidOperationException(SR.ExceptionHttpHandlerPrivacyKeyInvalid("ImageOwnerKey", ImageOwnerKey.ToString()));
                        }
                        keyType = ImageOwnerKeyType.AnonymousID;
                        return HttpContext.Current.Request.AnonymousID;

                    case ImageOwnerKeyType.SessionID:
                        if (HttpContext.Current.Session == null)
                        {
                            throw new InvalidOperationException(SR.ExceptionHttpHandlerPrivacyKeyInvalid("ImageOwnerKey", ImageOwnerKey.ToString()));
                        }
                        keyType = ImageOwnerKeyType.SessionID;
                        return ReadSessionKey();

                    default:
                        Debug.Fail("Unknown ImageOwnerKeyType.");
                        break;
                }
            }
            keyType = ImageOwnerKeyType.None;
            return string.Empty;
        }

        internal string PrivacyKey
        {
            get
            {
                ImageOwnerKeyType keyType;
                return GetPrivacyKey(out keyType);
            }
        }

        internal bool DeleteAfterServicing
        {
            get
            {
                // default, if is missing in config,  is true.
                return !(String.Compare(this["DeleteAfterServicing"], "false", StringComparison.OrdinalIgnoreCase) == 0); 
            }
        }

        /// <summary>
        /// Gets or sets the image owner key type.
        /// </summary>
        /// <value>The image owner key.</value>
        internal ImageOwnerKeyType ImageOwnerKey { get; set; }

        #endregion //Methods

        #region SettingsCollection Class

        private class StorageSettingsCollection : NameValueCollection
        {
            public StorageSettingsCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }
            internal void SetReadOnly(bool flag)
            {
                this.IsReadOnly = flag;
            }
        }

        #endregion //SettingsCollection Class
    }

    #endregion ChartHttpHandlerParameters

    #region DefaultImageHandler Class

    /// <summary>
    /// Default implementation of ChartHttpHandler.IImageHandler interface
    /// </summary>
    internal class DefaultImageHandler : IChartStorageHandler
    {

        #region Fields
        // Hashtable for storage
        private static Hashtable _storageData = new Hashtable();
        // lock object
        private static ReaderWriterLock _rwl = new ReaderWriterLock();
        // max access timeout
        private const int accessTimeout = 10000;

        static string _privacyKeyName = "_pk";
        static byte[] _privacyMarker = (new Guid("332E3AB032904bceA82B249C25E65CB6")).ToByteArray();
        static string _sessionKeyPrefix = "chart-3ece47b3-9481-4b22-ab45-ab669972eb79";

        #endregion //Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DefaultImageHandler"/> class.
        /// </summary>
        internal DefaultImageHandler()
        {
        }
        #endregion //Constructors

        #region Members
        /// <summary>
        /// Nots the type of the supported storage.
        /// </summary>
        /// <param name="settings">The settings.</param>
        private void NotSupportedStorageType(ChartHttpHandlerSettings settings)
        {
            throw new NotSupportedException( SR.ExceptionHttpHandlerStorageTypeUnsupported( settings.StorageType.ToString() ));
        }

        #endregion //Members

        #region Methods

        /// <summary>
        /// Returns privacy hash which will be save in the file.
        /// </summary>
        /// <returns>A byte array of hash data</returns>
        private static byte[] GetHashData()
        {
                string currentGuid = ChartHttpHandler.CurrentGuidKey;
                string sessionID = ChartHttpHandler.Settings.PrivacyKey;

                if (String.IsNullOrEmpty(sessionID))
                {
                    return new byte[0];
                }

                byte[] data = Encoding.UTF8.GetBytes(sessionID + "/" + currentGuid);

                using (SHA1 sha = new SHA1CryptoServiceProvider())
                {
                    return sha.ComputeHash(data);
                }

        }

        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)  return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private static string GetSessionImageKey(string key)
        {
            // all session variables starts with _sessionKeyPrefix to avoid direct access to session by passing image key in Url query.
            return _sessionKeyPrefix + key;
        }

        #endregion //Methods

        #region ImageHandler Members

        /// <summary>
        /// Stores the data into external medium.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        void IChartStorageHandler.Save(String key, Byte[] data)
        {
            ChartHttpHandlerSettings settings = ChartHttpHandler.Settings;
            ImageOwnerKeyType imageOwnerKeyType = ImageOwnerKeyType.None;
            string privacyKey = settings.GetPrivacyKey(out imageOwnerKeyType);
            if (settings.StorageType == ChartHttpHandlerStorageType.InProcess)
            {
                _rwl.AcquireWriterLock(accessTimeout);
                try
                {
                    _storageData[key] = data;
                    if (settings.PrivateImages && !String.IsNullOrEmpty(privacyKey))
                    {
                        _storageData[key + _privacyKeyName] = privacyKey;
                        Diagnostics.TraceWrite( SR.DiagnosticChartImageSavedPrivate(key, imageOwnerKeyType.ToString()), null);
                    }
                    else
                        Diagnostics.TraceWrite(SR.DiagnosticChartImageSaved(key), null);
                }
                finally
                {
                    _rwl.ReleaseWriterLock();
                }
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.File)
            {
                using (FileStream stream = File.Create(settings.Directory + key))
                {
                    stream.Write(data, 0, data.Length);
                    if (settings.PrivateImages && !String.IsNullOrEmpty(privacyKey))
                    {
                        byte[] privacyData = GetHashData();
                        stream.Write(privacyData, 0, privacyData.Length);
                        // we will put a marker at the end of the file;
                        stream.Write(_privacyMarker, 0, _privacyMarker.Length);
                        Diagnostics.TraceWrite(SR.DiagnosticChartImageSavedPrivate(key, imageOwnerKeyType.ToString()), null);
                    }
                    else
                        Diagnostics.TraceWrite(SR.DiagnosticChartImageSaved(key), null);
                }
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.Session)
            {
                HttpContext.Current.Session[GetSessionImageKey(key)] = data;
                Diagnostics.TraceWrite(SR.DiagnosticChartImageSaved(key), null);
            }
            else this.NotSupportedStorageType(settings);
        }


        /// <summary>
        /// Retrieves the data from external medium.
        /// </summary>
        /// <param name="key">The key.</param>
        Byte[] IChartStorageHandler.Load( String key)
        {
            ChartHttpHandlerSettings settings = ChartHttpHandler.Settings;
            ImageOwnerKeyType imageOwnerKeyType = ImageOwnerKeyType.None;
            string privacyKey = settings.GetPrivacyKey(out imageOwnerKeyType);
            Byte[] data = new Byte[0];
            if (settings.StorageType == ChartHttpHandlerStorageType.InProcess)
            {
                 _rwl.AcquireReaderLock(accessTimeout);
                 try
                 {
                     if (settings.PrivateImages)
                     {
                         if (!String.IsNullOrEmpty(privacyKey))
                         {
                             if (!String.Equals((string)_storageData[key + _privacyKeyName], privacyKey, StringComparison.Ordinal))
                             {
                                 Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailPrivacyFail(imageOwnerKeyType.ToString())), null);
                                 return data;
                             }
                         }
                         else
                         {
                             if (!String.IsNullOrEmpty((string)_storageData[key + _privacyKeyName]))
                             {
                                 Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailPrivacyFail(imageOwnerKeyType.ToString())), null);
                                 return data;
                             }
                         }
                     }
                     data = (Byte[])_storageData[key];
                     if (data == null)
                     {
                         Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailNotFound), null);
                     }
                 }
                 finally
                 {
                     _rwl.ReleaseReaderLock();
                 }
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.File)
            {
                settings.ValidateUri(key);
                if (File.Exists(settings.Directory + key))
                {
                    using (FileStream fileStream = File.OpenRead(settings.Directory + key))
                    {
                        byte[] fileData = new byte[fileStream.Length];
                        fileStream.Read(fileData, 0, fileData.Length);
                        using (MemoryStream stream = new MemoryStream(fileData))
                        {
                            int streamCut = 0;
                            if (settings.PrivateImages)
                            {
                                // read the marker first
                                byte[] privacyMarkerStream = new Byte[_privacyMarker.Length];

                                streamCut += _privacyMarker.Length;
                                stream.Seek(stream.Length - streamCut, SeekOrigin.Begin);
                                stream.Read(privacyMarkerStream, 0, privacyMarkerStream.Length);

                                if (!String.IsNullOrEmpty(privacyKey))
                                {
                                    byte[] privacyData = GetHashData();
                                    streamCut += privacyData.Length;
                                    byte[] privacyDataFromStream = new Byte[privacyData.Length];
                                    stream.Seek(stream.Length - streamCut, SeekOrigin.Begin);
                                    stream.Read(privacyDataFromStream, 0, privacyDataFromStream.Length);

                                    if (!CompareBytes(privacyDataFromStream, privacyData))
                                    {
                                        Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailPrivacyFail(imageOwnerKeyType.ToString())), null);
                                        return data;
                                    }
                                }
                                else
                                {
                                    // this image is marked as private - check end return null if fails
                                    if (String.Equals(
                                        Encoding.Unicode.GetString(privacyMarkerStream),
                                        Encoding.Unicode.GetString(_privacyMarker),
                                        StringComparison.Ordinal))
                                    {
                                        Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailPrivacyFail(imageOwnerKeyType.ToString())), null);
                                        return data;
                                    }
                                    // its fine ( no user is stored )
                                    streamCut = 0;
                                }
                            }
                            stream.Seek(0, SeekOrigin.Begin);
                            data = new Byte[(int)stream.Length - streamCut];
                            stream.Read(data, 0, (int)data.Length);
                        }
                    }
                }
                else
                    Diagnostics.TraceWrite(SR.DiagnosticChartImageServedFail(key, SR.DiagnosticChartImageServedFailNotFound), null);
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.Session)
            {
                data = (Byte[])HttpContext.Current.Session[GetSessionImageKey(key)];
            }
            else this.NotSupportedStorageType(settings);
            return data;

        }


        /// <summary>
        /// Removes the data from external medium.
        /// </summary>
        /// <param name="key">The key.</param>
        void IChartStorageHandler.Delete(String key)
        {
            ChartHttpHandlerSettings settings = ChartHttpHandler.Settings;
            if (settings.StorageType == ChartHttpHandlerStorageType.InProcess)
            {

                _rwl.AcquireWriterLock(accessTimeout);
                try
                {
                    _storageData.Remove(key);
                    _storageData.Remove(key + _privacyKeyName);
                }
                finally
                {
                    _rwl.ReleaseWriterLock();
                }
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.File)
            {
                File.Delete(settings.Directory + key);
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.Session)
            {
                HttpContext.Current.Session.Remove(GetSessionImageKey(key));
            }
            else this.NotSupportedStorageType(settings);
        }

        /// <summary>
        /// Checks for existence the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool IChartStorageHandler.Exists(String key)
        {
            ChartHttpHandlerSettings settings = ChartHttpHandler.Settings;
            if (settings.StorageType == ChartHttpHandlerStorageType.InProcess)
            {
                _rwl.AcquireReaderLock(accessTimeout);
                try
                {
                    return _storageData.Contains(key);
                }
                finally
                {
                    _rwl.ReleaseReaderLock();
                }
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.File)
            {
                return File.Exists(settings.Directory + key);
            }
            else if (settings.StorageType == ChartHttpHandlerStorageType.Session)
            {
                return HttpContext.Current.Session[GetSessionImageKey(key)] is Byte[];
            }
            else this.NotSupportedStorageType(settings);
            return false;
        }

        #endregion

    }

    #endregion //DefaultImageHandler Class

    #region RingTimeTracker class

    /// <summary>
    /// RingItem contains time span of creation timedate and  index for key generation.
    /// </summary>
    internal class RingItem
    {
        internal Int32 Index;
        internal DateTime Created = DateTime.Now;
        internal string   SessionID = String.Empty;
        internal bool InUse;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RingItem"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        internal RingItem( int index)
        {
            this.Index = index;
        }
    }
    /// <summary>
    /// RingTimeTracker is a helper class for generating keys and tracking RingItem. 
    /// Contains linked list queue and tracks exprired items.
    /// </summary>
    internal class RingTimeTracker
    {
        #region Fields
        // the item life span
        private TimeSpan _itemLifeTime = TimeSpan.FromSeconds(360);
        // last requested RingItem
        private LinkedListNode<RingItem> _current;
        // default key format to format names
        private String _keyFormat = String.Empty;
        // LinkedList with ring items
        private LinkedList<RingItem> _list = new LinkedList<RingItem>();
        // Record session ID
        private bool _recordSessionID = false;

        #endregion //Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RingTimeTracker"/> class.
        /// </summary>
        /// <param name="itemLifeTime">The item life time.</param>
        /// <param name="keyFormat">The key format.</param>
        /// <param name="recordSessionID">if set to <c>true</c> the session ID will be recorded.</param>
        internal RingTimeTracker(TimeSpan itemLifeTime, String keyFormat, bool recordSessionID)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(keyFormat));
            this._itemLifeTime = itemLifeTime;
            this._keyFormat = keyFormat;
            this._list.AddLast(new RingItem(_list.Count));
            this._current = this._list.First;
            this._current.Value.Created = DateTime.Now - this._itemLifeTime - TimeSpan.FromSeconds(1);
            this._recordSessionID = recordSessionID;
        }

        #endregion //Constructors

        #region Methods

        

        /// <summary>
        /// Determines whether the specified item is expired.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="now">The now.</param>
        /// <returns>
        /// 	<c>true</c> if the specified item is expired; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsExpired(RingItem item, DateTime now)
        {
            TimeSpan elapsed = (now - item.Created);
            return elapsed > this._itemLifeTime;
        }


        /// <summary>
        /// Gets the next key.
        /// </summary>
        /// <returns></returns>
        internal String GetNextKey()
        {
            DateTime now = DateTime.Now;
            lock (this)
            {
                if ( !this.IsExpired(this._current.Value, now))
                {
                    if (this._current.Next == null)
                    {
                        if (!this.IsExpired(this._list.First.Value, now))
                        {
                            this._list.AddLast(new RingItem(_list.Count));
                            this._current = this._list.Last;
                        }
                        else
                        {
                            this._current = this._list.First;
                        }
                    }
                    else
                    {
                        if (!this.IsExpired(this._current.Next.Value, now))
                        {
                            this._list.AddAfter(this._current, new RingItem(_list.Count));
                        }
                        this._current = this._current.Next;
                    }
                }
                this._current.Value.Created = now;
                if (this._recordSessionID)
                {
                    this._current.Value.SessionID = ChartHttpHandler.Settings.ReadSessionKey();
                    this._current.Value.InUse = true;
                }
                return this.GetCurrentKey();
            }
        }

        /// <summary>
        /// Gets the current key.
        /// </summary>
        /// <returns></returns>
        internal String GetCurrentKey()
        {
            return String.Format( CultureInfo.InvariantCulture, this._keyFormat, this._current.Value.Index);
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="ringItem">The ring item.</param>
        /// <returns></returns>
        internal String GetKey(RingItem ringItem)
        {
            return String.Format(CultureInfo.InvariantCulture, this._keyFormat, ringItem.Index);
        }

        /// <summary>
        /// Do Action for each item.
        /// </summary>
        /// <param name="onlyExpired">if set to <c>true</c> do action for only expired items.</param>
        /// <param name="action">The action.</param>
        public void ForEach(bool onlyExpired, Action<RingItem> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            DateTime now = DateTime.Now;
            lock (this)
            {
                foreach (RingItem item in this._list)
                {
                    if (onlyExpired)
                    {
                        if (this.IsExpired(item, now))
                        {
                            action(item);
                        }
                    }
                    else
                    {
                        action(item);
                    }
                }
            }
        }

        #endregion //Methods

    }
    #endregion //RingTracker class

    #region  RingTimeTrackerFactory Class

    /// <summary>
    /// RingTimeTrackerFactory contains static list of RingTimeTracker for each key formats
    /// </summary>
    internal static class RingTimeTrackerFactory
    {
        
        private static ListDictionary _ringTrackers = new ListDictionary();
        private static Object _lockObject = new Object();

        /// <summary>
        /// Gets the ring tracker by specified key format.
        /// </summary>
        /// <param name="keyFormat">The key format.</param>
        /// <returns></returns>
        internal static RingTimeTracker GetRingTracker(String keyFormat)
        {
            if (_ringTrackers.Contains(keyFormat))
            {
                return (RingTimeTracker)_ringTrackers[keyFormat];
            }
            lock (_lockObject)
            {
                if (_ringTrackers.Contains(keyFormat))
                {
                    return (RingTimeTracker)_ringTrackers[keyFormat];
                }
                RingTimeTracker result = new RingTimeTracker(ChartHttpHandler.Settings.Timeout, keyFormat,ChartHttpHandler.Settings.StorageType == ChartHttpHandlerStorageType.Session);
                _ringTrackers.Add(keyFormat, result);
                return result;
            }
        }

        internal static IList OpenedRingTimeTrackers()
        {
            lock (_lockObject)
            {
                return new ArrayList(_ringTrackers.Values);
            }
        }
        
    }

    #endregion  //RingTimeTrackerFactory Class

    #region Diagnostics class

    /// <summary>
    /// Contains helpres methods for diagnostics.
    /// </summary>
    internal static class Diagnostics
    {
        /// <summary>
        /// Trace category
        /// </summary>
        const string ChartCategory = "chart.handler";
        /// <summary>
        /// Name of context item which contain the current trace item 
        /// </summary>
        const string ContextID = "Trace-{89FA5660-BD13-4f1b-8C7C-355CEC92CC7E}";
        /// <summary>
        /// Used for syncronizing.
        /// </summary>
        static object _lockObject = new object();
        /// <summary>
        /// Limit of trace messages in the history.
        /// </summary>
        const int MessageLimit = 20;
        /// <summary>
        /// Collection of request messages.
        /// </summary>
        static List<HandlerPageTraceInfo> _messages = new List<HandlerPageTraceInfo>(MessageLimit);

        /// <summary>
        /// Contains request info
        /// </summary>
        public class HandlerPageTraceInfo
        {
            /// <summary>
            /// Events collection in this request.
            /// </summary>
            private List<ChartHandlerEvents> _events = new List<ChartHandlerEvents>();
            /// <summary>
            /// Initializes a new instance of the <see cref="HandlerPageTraceInfo"/> class.
            /// </summary>
            public HandlerPageTraceInfo()
            {
                if (HttpContext.Current != null)
                {
                    DateStamp = DateTime.Now;
                    if (HttpContext.Current.Request != null)
                    {
                        Url = HttpContext.Current.Request.Url.ToString();
                        Verb = HttpContext.Current.Request.HttpMethod;
                    }
                }
            }
            /// <summary>
            /// Gets or sets the date stamp.
            /// </summary>
            /// <value>The date stamp.</value>
            public DateTime DateStamp { get; private set; }
            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>The URL.</value>
            public string Url { get; private set; }
            /// <summary>
            /// Gets or sets the verb.
            /// </summary>
            /// <value>The verb.</value>
            public string Verb { get; private set; }
            /// <summary>
            /// Gets the events.
            /// </summary>
            /// <value>The events.</value>
            public IList<ChartHandlerEvents> Events
            {
                get
                {
                    return _events.AsReadOnly();
                }
            }
            /// <summary>
            /// Adds a trace info item.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="errorInfo">The error info.</param>
            internal void AddTraceInfo(string message, string errorInfo)
            {
                lock (_events)
                {
                    _events.Add(new ChartHandlerEvents()
                        {
                            Message = message,
                            ErrorInfo = errorInfo
                        }
                     );
                }
            }
        }

        /// <summary>
        /// Contains an event in particural request.
        /// </summary>
        public class ChartHandlerEvents
        {
            /// <summary>
            /// Gets or sets the message.
            /// </summary>
            /// <value>The message.</value>
            public string Message { get; set; }
            /// <summary>
            /// Gets or sets the error info.
            /// </summary>
            /// <value>The error info.</value>
            public string ErrorInfo { get; set; }
            /// <summary>
            /// Gets the text.
            /// </summary>
            /// <value>The text.</value>
            public string Text { get { return Message + ErrorInfo; } }
        }

        /// <summary>
        /// Writes message in the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorInfo">The error info.</param>
        internal static void TraceWrite( string message, Exception errorInfo)
        {
            if (IsTraceEnabled)
            {
                HttpContext.Current.Trace.Write(ChartCategory, message, errorInfo);
                if (CurrentTraceInfo != null)
                {
                    CurrentTraceInfo.AddTraceInfo(message, errorInfo != null ? errorInfo.ToString() : String.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the current trace info.
        /// </summary>
        /// <value>The current trace info.</value>
        private static HandlerPageTraceInfo CurrentTraceInfo
        {
            get
            {
                lock (_lockObject)
                {
                    if (HttpContext.Current != null)
                    {
                        if (HttpContext.Current.Items[Diagnostics.ContextID] == null)
                        {
                            HandlerPageTraceInfo pageTrace = new HandlerPageTraceInfo();
                            _messages.Add(pageTrace);
                            if (_messages.Count > MessageLimit)
                            {
                                _messages.RemoveRange(0, _messages.Count - MessageLimit);
                            }
                            HttpContext.Current.Items[Diagnostics.ContextID] = pageTrace;
                        }
                        return (HandlerPageTraceInfo)HttpContext.Current.Items[Diagnostics.ContextID];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is trace enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is trace enabled; otherwise, <c>false</c>.
        /// </value>
        internal static bool IsTraceEnabled
        {
            get
            {
                return HttpContext.Current != null && HttpContext.Current.Trace.IsEnabled;
            }
        }

        /// <summary>
        /// Gets the messages collection.
        /// </summary>
        /// <value>The messages.</value>
        internal static ReadOnlyCollection<HandlerPageTraceInfo> Messages
        {
            get
            {
                List<HandlerPageTraceInfo> result;
                lock (_lockObject)
                {
                    result = new List<HandlerPageTraceInfo>(_messages);
                }
                return result.AsReadOnly();
            }
        }
    }

    #endregion //Diagnostics class
}
