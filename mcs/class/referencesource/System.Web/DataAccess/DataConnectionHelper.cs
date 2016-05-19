//------------------------------------------------------------------------------
// <copyright file="DataConnectionError.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
[assembly:WebResource("add_permissions_for_users.gif", "image/gif")]
[assembly:WebResource("properties_security_tab_w_user.gif", "image/gif")]
[assembly:WebResource("properties_security_tab.gif", "image/gif")]

namespace System.Web.DataAccess
{
    using System;
    using System.Web;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;
    using System.Threading;
    using System.Configuration;
    using System.Web.Util;
    using System.Security.Permissions;
    using System.Web.Hosting;
    using System.Security.Principal;
    using System.Web.UI;
    using System.Web.Handlers;
    using System.Web.Configuration;
    using System.Diagnostics;
    using System.Text;

    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    internal enum DataConnectionErrorEnum
    {
        CanNotCreateDataDir,
        CanNotWriteToDataDir,
        CanNotWriteToDBFile
    }

    internal static class DataConnectionHelper
    {
        internal static string GetCurrentName()
        {
            string userName = "NETWORK SERVICE";
            string domainName = "NT AUTHORITY";

            IntPtr pSid = IntPtr.Zero;

            try
            {
                if( UnsafeNativeMethods.ConvertStringSidToSid( "S-1-5-20", out pSid ) != 0 &&
                    pSid != IntPtr.Zero )
                {
                    int userNameLen = 256;
                    int domainNameLen = 256;
                    int sidNameUse = 0;
                    StringBuilder bufUserName = new StringBuilder( userNameLen );
                    StringBuilder bufDomainName = new StringBuilder( domainNameLen );
                    if( 0 != UnsafeNativeMethods.LookupAccountSid( null,
                                                                   pSid,
                                                                   bufUserName,
                                                                   ref userNameLen,
                                                                   bufDomainName,
                                                                   ref domainNameLen,
                                                                   ref sidNameUse ) )
                    {
                        userName = bufUserName.ToString();
                        domainName = bufDomainName.ToString();
                    }
                }

                WindowsIdentity id = WindowsIdentity.GetCurrent();
                if( id != null && id.Name != null )
                {
                    if ( string.Compare( id.Name,
                                         domainName + @"\" + userName,
                                         StringComparison.OrdinalIgnoreCase ) == 0 )
                    {
                        return userName;
                    }

                    return id.Name;
                }
            }
            catch {}
            finally
            {
                if( pSid != IntPtr.Zero )
                {
                    UnsafeNativeMethods.LocalFree( pSid );
                }
            }

            return String.Empty;
        }

    }

    internal class DataConnectionErrorFormatter : ErrorFormatter
    {
        protected static NameValueCollection s_errMessages = new NameValueCollection();
        protected static object s_Lock = new object ();
        protected string _UserName;
        protected DataConnectionErrorEnum _Error;

        protected override string ErrorTitle
        {
            get { return null; }
        }

        protected override string Description
        {
            get { return null; }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return SR.GetString(SR.DataAccessError_MiscSectionTitle) ;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                string url;
                int currentNumber = 1;
                string resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_1);
                string miscContent = "<ol>\n<li>" + resourceString + "</li>\n";

                switch (_Error)
                {
                    case DataConnectionErrorEnum.CanNotCreateDataDir:
                        resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_2_CanNotCreateDataDir);
                        miscContent += "<li>" + resourceString + "</li>\n";

                        resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_2);
                        miscContent += "<li>" + resourceString + "</li>\n";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDataDir:
                        resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_2);
                        miscContent += "<li>" + resourceString + "</li>\n";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDBFile:
                        resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_2_CanNotWriteToDBFile_a);
                        miscContent += "<li>" + resourceString + "</li>\n";

                        resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_2_CanNotWriteToDBFile_b);
                        miscContent += "<li>" + resourceString + "</li>\n";
                        break;
                }
                resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_3);
                miscContent += "<li>" + resourceString + "<br></li>\n";

                url = AssemblyResourceLoader.GetWebResourceUrl(typeof(Page), "properties_security_tab.gif", true);
                miscContent += "<br><br><IMG SRC=\"" + url + "\"><br><br><br>";

                resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_ClickAdd);
                miscContent += "<li>" + resourceString + "</li>\n";

                url = AssemblyResourceLoader.GetWebResourceUrl(typeof(Page), "add_permissions_for_users.gif", true);
                miscContent += "<br><br><IMG SRC=\"" + url + "\"><br><br>";

                string four;
                if (!String.IsNullOrEmpty(_UserName))
                    four = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_4, _UserName);
                else
                    four = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_4_2);
                miscContent += "<li>" + four + "</li>\n";

                resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_ClickOK);
                miscContent += "<li>" + resourceString + "</li>\n";

                resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_5);
                miscContent += "<li>" + resourceString + "</li>\n";

                url = AssemblyResourceLoader.GetWebResourceUrl(typeof(Page), "properties_security_tab_w_user.gif", true);
                miscContent += "<br><br><IMG SRC=\"" + url + "\"><br><br>";

                resourceString = GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, SR.DataAccessError_MiscSection_ClickOK);
                miscContent += "<li>" + resourceString + "</li>\n";
                return miscContent;
            }
        }

        protected override string ColoredSquareTitle
        {
            get { return null; }
        }

        protected override string ColoredSquareContent
        {
            get { return null; }
        }

        protected override bool ShowSourceFileInfo
        {
            get { return false; }
        }

        private string GetResourceStringAndSetAdaptiveNumberedText(ref int currentNumber, string resourceId) {
            string resourceString = SR.GetString(resourceId);
            SetAdaptiveNumberedText(ref currentNumber, resourceString);
            return resourceString;
        }

        private string GetResourceStringAndSetAdaptiveNumberedText(ref int currentNumber, string resourceId, string parameter1) {
            string resourceString = SR.GetString(resourceId, parameter1);
            SetAdaptiveNumberedText(ref currentNumber, resourceString);
            return resourceString;
        }

        private void SetAdaptiveNumberedText(ref int currentNumber, string resourceString) {
            string adaptiveText = currentNumber.ToString(CultureInfo.InvariantCulture) + " " + resourceString;
            AdaptiveMiscContent.Add(adaptiveText);
            currentNumber += 1;
        }
    }

    internal sealed class SqlExpressConnectionErrorFormatter : DataConnectionErrorFormatter
    {
        internal SqlExpressConnectionErrorFormatter(DataConnectionErrorEnum error)
        {
            _UserName = (HttpRuntime.HasUnmanagedPermission() ? DataConnectionHelper.GetCurrentName() : String.Empty);
            _Error = error;
        }

        internal SqlExpressConnectionErrorFormatter(string userName, DataConnectionErrorEnum error)
        {
            _UserName = userName;
            _Error = error;
        }

        protected override string ErrorTitle
        {
            get
            {
                string resourceKey = null;

                switch ( _Error )
                {
                    case DataConnectionErrorEnum.CanNotCreateDataDir:
                        resourceKey = SR.DataAccessError_CanNotCreateDataDir_Title;
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDataDir:
                        resourceKey = SR.SqlExpressError_CanNotWriteToDataDir_Title;
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDBFile:
                        resourceKey = SR.SqlExpressError_CanNotWriteToDbfFile_Title;
                        break;
                }
                return SR.GetString (resourceKey);
            }
        }

        protected override string Description
        {
            get
            {
                string resourceKey1 = null;
                string resourceKey2 = null;

                switch (_Error)
                {
                    case DataConnectionErrorEnum.CanNotCreateDataDir:
                        resourceKey1 = SR.DataAccessError_CanNotCreateDataDir_Description;
                        resourceKey2 = SR.DataAccessError_CanNotCreateDataDir_Description_2;
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDataDir:
                        resourceKey1 = SR.SqlExpressError_CanNotWriteToDataDir_Description;
                        resourceKey2 = SR.SqlExpressError_CanNotWriteToDataDir_Description_2;
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDBFile:
                        resourceKey1 = SR.SqlExpressError_CanNotWriteToDbfFile_Description;
                        resourceKey2 = SR.SqlExpressError_CanNotWriteToDbfFile_Description_2;
                        break;
                }
                string desc;
                if (!String.IsNullOrEmpty(_UserName))
                    desc = SR.GetString (resourceKey1, _UserName);
                else
                    desc = SR.GetString (resourceKey2);
                desc += " " + SR.GetString(SR.SqlExpressError_Description_1);
                return desc;
            }
        }
    }

    internal sealed class SqlExpressDBFileAutoCreationErrorFormatter : UnhandledErrorFormatter
    {
        static string s_errMessage = null;
        static object s_Lock = new object();

        internal SqlExpressDBFileAutoCreationErrorFormatter( Exception exception ) : base( exception )
        {
        }
        protected override string MiscSectionTitle
        {
            get
            {
                return SR.GetString(SR.SqlExpress_MDF_File_Auto_Creation_MiscSectionTitle) ;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                return CustomErrorMessage;
            }
        }

        internal static string CustomErrorMessage
        {
            get
            {
                if( s_errMessage == null )
                {
                    lock( s_Lock )
                    {
                        if( s_errMessage == null )
                        {
                            string resourceString;

                            resourceString = SR.GetString(SR.SqlExpress_MDF_File_Auto_Creation) ;
                            s_errMessage += "<br><br><p>" + resourceString + "<br></p>\n";

                            s_errMessage += "<ol>\n";

                            resourceString = SR.GetString(SR.SqlExpress_MDF_File_Auto_Creation_1) ;
                            s_errMessage += "<li>" + resourceString + "</li>\n";
                            resourceString = SR.GetString(SR.SqlExpress_MDF_File_Auto_Creation_2) ;
                            s_errMessage += "<li>" + resourceString + "</li>\n";
                            resourceString = SR.GetString(SR.SqlExpress_MDF_File_Auto_Creation_3) ;
                            s_errMessage += "<li>" + resourceString + "</li>\n";
                            resourceString = SR.GetString(SR.SqlExpress_MDF_File_Auto_Creation_4) ;
                            s_errMessage += "<li>" + resourceString + "</li>\n";
                            s_errMessage += "</ol>\n";
                        }
                    }
                }

                return s_errMessage;
            }
        }
    }
}
