//------------------------------------------------------------------------------
// <copyright file="SmtpMail.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Simple SMTP send mail utility
 *
 * Copyright (c) 2000, Microsoft Corporation
 */
namespace System.Web.Mail {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Util;
/*
 * Class that sends MailMessage using CDONTS/CDOSYS
 */

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[Obsolete("The recommended alternative is System.Net.Mail.SmtpClient. http://go.microsoft.com/fwlink/?linkid=14202")]
public class SmtpMail {

    private static object _lockObject = new object();

    private SmtpMail() {
    }

#if !FEATURE_PAL // FEATURE_PAL does not enable SmtpMail
    //
    // Late bound helper
    //

    internal class LateBoundAccessHelper {
        private String _progId;
        private Type _type;

        internal LateBoundAccessHelper(String progId) {
            _progId = progId;
        }

        private Type LateBoundType {
            get {
                if (_type == null) {
                    try {
                        _type = Type.GetTypeFromProgID(_progId);
                    }
                    catch {
                    }

                    if (_type == null)
                        throw new HttpException(SR.GetString(SR.SMTP_TypeCreationError, _progId));
                }

                return _type;
            }
        }

        internal Object CreateInstance() {
            return Activator.CreateInstance(LateBoundType);
        }

        internal Object CallMethod(Object obj, String methodName, Object[] args) {
            try {
                return CallMethod(LateBoundType, obj, methodName, args);
            }
            catch (Exception e) {
                throw new HttpException(GetInnerMostException(e).Message, e);
            }
        }

        internal static Object CallMethodStatic(Object obj, String methodName, Object[] args) {
            return CallMethod(obj.GetType(), obj, methodName, args);
        }

        private static Object CallMethod(Type type, Object obj, String methodName, Object[] args) {
            return type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, obj, args, CultureInfo.InvariantCulture);
        }

        private static Exception GetInnerMostException(Exception e) {
            if (e.InnerException == null)
                return e;
            else
                return GetInnerMostException(e.InnerException);
        }

        internal Object GetProp(Object obj, String propName) {
            try {
                return GetProp(LateBoundType, obj, propName);
            }
            catch (Exception e) {
                throw new HttpException(GetInnerMostException(e).Message, e);
            }
        }

        internal static Object GetPropStatic(Object obj, String propName) {
            return GetProp(obj.GetType(), obj, propName);
        }

        private static Object GetProp(Type type, Object obj, String propName) {
            return type.InvokeMember(propName, BindingFlags.GetProperty, null, obj,new Object[0], CultureInfo.InvariantCulture);
        }

        internal void SetProp(Object obj, String propName, Object propValue) {
            try {
                SetProp(LateBoundType, obj, propName, propValue);
            }
            catch (Exception e) {
                throw new HttpException(GetInnerMostException(e).Message, e);
            }
        }

        internal static void SetPropStatic(Object obj, String propName, Object propValue) {
            SetProp(obj.GetType(), obj, propName, propValue);
        }

        private static void SetProp(Type type, Object obj, String propName, Object propValue) {
            if (propValue != null && (propValue is string) && ((string)propValue).IndexOf('\0') >= 0)
                throw new ArgumentException();
            type.InvokeMember(propName, BindingFlags.SetProperty, null, obj, new Object[1] { propValue }, CultureInfo.InvariantCulture);
        }

        internal void SetProp(Object obj, String propName, Object propKey, Object propValue) {
            try {
                SetProp(LateBoundType, obj, propName, propKey, propValue);
            }
            catch (Exception e) {
                throw new HttpException(GetInnerMostException(e).Message, e);
            }
        }

        internal static void SetPropStatic(Object obj, String propName, Object propKey, Object propValue) {
            SetProp(obj.GetType(), obj, propName, propKey, propValue);
        }

        private static void SetProp(Type type, Object obj, String propName, Object propKey, Object propValue) {
            if (propValue != null && (propValue is string) && ((string)propValue).IndexOf('\0') >= 0)
                throw new ArgumentException();
            type.InvokeMember(propName, BindingFlags.SetProperty, null, obj,new Object[2] { propKey, propValue }, CultureInfo.InvariantCulture);
        }
    }

    //
    // Late bound access to CDONTS
    //

    internal class CdoNtsHelper {

        private static LateBoundAccessHelper _helper = new LateBoundAccessHelper("CDONTS.NewMail");

        private CdoNtsHelper() {
        }

        internal static void Send(MailMessage message) {
            // create mail object
            Object newMail = _helper.CreateInstance();

            // set properties

            if (message.From != null)
                _helper.SetProp(newMail, "From", message.From);

            if (message.To != null)
                _helper.SetProp(newMail, "To", message.To);

            if (message.Cc != null)
                _helper.SetProp(newMail, "Cc", message.Cc);

            if (message.Bcc != null)
                _helper.SetProp(newMail, "Bcc", message.Bcc);

            if (message.Subject != null)
                _helper.SetProp(newMail, "Subject", message.Subject);

            if (message.Priority != MailPriority.Normal) {
                int p = 0;
                switch (message.Priority) {
                case MailPriority.Low:      p = 0;  break;
                case MailPriority.Normal:   p = 1;  break;
                case MailPriority.High:     p = 2;  break;
                }
                _helper.SetProp(newMail, "Importance", p);
            }

            if (message.BodyEncoding != null)
                _helper.CallMethod(newMail, "SetLocaleIDs", new Object[1] { message.BodyEncoding.CodePage });

            if (message.UrlContentBase != null)
                _helper.SetProp(newMail, "ContentBase", message.UrlContentBase);

            if (message.UrlContentLocation != null)
                _helper.SetProp(newMail, "ContentLocation", message.UrlContentLocation);

            int numHeaders = message.Headers.Count;
            if (numHeaders > 0) {
                IDictionaryEnumerator e = message.Headers.GetEnumerator();
                while (e.MoveNext()) {
                    String k = (String)e.Key;
                    String v = (String)e.Value;
                    _helper.SetProp(newMail, "Value", k, v);
                }
            }

            if (message.BodyFormat == MailFormat.Html) {
                _helper.SetProp(newMail, "BodyFormat", 0);
                _helper.SetProp(newMail, "MailFormat", 0);
            }

            // always set Body (VSWhidbey 176284)
            _helper.SetProp(newMail, "Body", (message.Body != null) ? message.Body : String.Empty);

            for (IEnumerator e = message.Attachments.GetEnumerator(); e.MoveNext(); ) {
                MailAttachment a = (MailAttachment)e.Current;

                int c = 0;
                switch (a.Encoding) {
                case MailEncoding.UUEncode: c = 0;  break;
                case MailEncoding.Base64:   c = 1;  break;
                }

                _helper.CallMethod(newMail, "AttachFile", new Object[3] { a.Filename, null, (Object)c });
            }

            // send mail
            _helper.CallMethod(newMail, "Send", new Object[5] { null, null, null, null, null });

            // close unmanaged COM classic component
            Marshal.ReleaseComObject(newMail);
        }

        internal static void Send(String from, String to, String subject, String messageText) {
            MailMessage m = new MailMessage();
            m.From = from;
            m.To = to;
            m.Subject = subject;
            m.Body = messageText;
            Send(m);
        }
    }

    //
    // Late bound access to CDOSYS
    //

    internal class CdoSysHelper {

        private static LateBoundAccessHelper _helper = new LateBoundAccessHelper("CDO.Message");
        enum CdoSysLibraryStatus {
            NotChecked,
            Exists,
            DoesntExist
        }
        // Variable that shows if cdosys.dll exists
        private static CdoSysLibraryStatus cdoSysLibraryInfo = CdoSysLibraryStatus.NotChecked;

        private CdoSysHelper() {
        }

        private static void SetField(Object m, String name, String value) {
            _helper.SetProp(m, "Fields", "urn:schemas:mailheader:" + name, value);
            Object fields = _helper.GetProp(m, "Fields");
            LateBoundAccessHelper.CallMethodStatic(fields, "Update", new Object[0]);
            Marshal.ReleaseComObject(fields);
        }

        private static bool CdoSysExists() {
            // Check that the cdosys.dll exists
            if(cdoSysLibraryInfo == CdoSysLibraryStatus.NotChecked) {
                string fullDllPath = PathUtil.GetSystemDllFullPath("cdosys.dll");
                IntPtr cdoSysModule = UnsafeNativeMethods.LoadLibrary(fullDllPath);
                if(cdoSysModule != IntPtr.Zero) {
                    UnsafeNativeMethods.FreeLibrary(cdoSysModule);
                    cdoSysLibraryInfo = CdoSysLibraryStatus.Exists;
                    return true;
                }
                cdoSysLibraryInfo = CdoSysLibraryStatus.DoesntExist;
                return false;
            }
            // return cached value, found at a previous check
            return (cdoSysLibraryInfo == CdoSysLibraryStatus.Exists);
        }

        internal static bool OsSupportsCdoSys() {
            Version osVersion = Environment.OSVersion.Version;
            if ((osVersion.Major >= 7 || (osVersion.Major == 6 && osVersion.Minor >= 1))) {
                // for some OS versions higher that 6, CdoSys.dll doesn't exist
                return CdoSysExists();
            }
            return true;
        }

        internal static void Send(MailMessage message) {
            // create message object
            Object m = _helper.CreateInstance();

            // set properties

            if (message.From != null)
                _helper.SetProp(m, "From", message.From);

            if (message.To != null)
                _helper.SetProp(m, "To", message.To);

            if (message.Cc != null)
                _helper.SetProp(m, "Cc", message.Cc);

            if (message.Bcc != null)
                _helper.SetProp(m, "Bcc", message.Bcc);

            if (message.Subject != null)
                _helper.SetProp(m, "Subject", message.Subject);


            if (message.Priority != MailPriority.Normal) {
                String importance = null;
                switch (message.Priority) {
                case MailPriority.Low:      importance = "low";     break;
                case MailPriority.Normal:   importance = "normal";  break;
                case MailPriority.High:     importance = "high";    break;
                }

                if (importance != null)
                    SetField(m, "importance", importance);
            }

            if (message.BodyEncoding != null) {
                Object body = _helper.GetProp(m, "BodyPart");
                LateBoundAccessHelper.SetPropStatic(body, "Charset", message.BodyEncoding.BodyName);
                Marshal.ReleaseComObject(body);
            }

            if (message.UrlContentBase != null)
                SetField(m, "content-base", message.UrlContentBase);

            if (message.UrlContentLocation != null)
                SetField(m, "content-location", message.UrlContentLocation);

            int numHeaders = message.Headers.Count;
            if (numHeaders > 0) {
                IDictionaryEnumerator e = message.Headers.GetEnumerator();
                while (e.MoveNext()) {
                    SetField(m, (String)e.Key, (String)e.Value);
                }
            }

            if (message.Body != null) {
                if (message.BodyFormat == MailFormat.Html) {
                    _helper.SetProp(m, "HtmlBody", message.Body);
                }
                else {
                    _helper.SetProp(m, "TextBody", message.Body);
                }
            }
            else {
                _helper.SetProp(m, "TextBody", String.Empty);
            }

            for (IEnumerator e = message.Attachments.GetEnumerator(); e.MoveNext(); ) {
                MailAttachment a = (MailAttachment)e.Current;
                Object bodyPart = _helper.CallMethod(m, "AddAttachment", new Object[3] { a.Filename, null, null });

                if (a.Encoding == MailEncoding.UUEncode)
                    _helper.SetProp(m, "MimeFormatted", false);

                if (bodyPart != null)
                    Marshal.ReleaseComObject(bodyPart);
            }

            // optional SMTP server
            string server = SmtpMail.SmtpServer;
            if (!String.IsNullOrEmpty(server) || message.Fields.Count > 0) {
                Object config = LateBoundAccessHelper.GetPropStatic(m, "Configuration");

                if (config != null) {
                    LateBoundAccessHelper.SetPropStatic(config, "Fields", "http://schemas.microsoft.com/cdo/configuration/sendusing", (Object)2);
                    LateBoundAccessHelper.SetPropStatic(config, "Fields", "http://schemas.microsoft.com/cdo/configuration/smtpserverport", (Object)25);
                    if (!String.IsNullOrEmpty(server)) {
                        LateBoundAccessHelper.SetPropStatic(config, "Fields", "http://schemas.microsoft.com/cdo/configuration/smtpserver", server);
                    }

                    foreach (DictionaryEntry e in message.Fields) {
                        LateBoundAccessHelper.SetPropStatic(config, "Fields", (String)e.Key, e.Value);
                    }

                    Object fields = LateBoundAccessHelper.GetPropStatic(config, "Fields");
                    LateBoundAccessHelper.CallMethodStatic(fields, "Update", new Object[0]);
                    Marshal.ReleaseComObject(fields);

                    Marshal.ReleaseComObject(config);
                }
            }

            if (HostingEnvironment.IsHosted) {
                // revert to process identity while sending mail
                using (new ProcessImpersonationContext()) {
                    // send mail
                    _helper.CallMethod(m, "Send", new Object[0]);
                }
            }
            else {
                // send mail
                _helper.CallMethod(m, "Send", new Object[0]);
            }

            // close unmanaged COM classic component
            Marshal.ReleaseComObject(m);
        }

        internal static void Send(String from, String to, String subject, String messageText) {
            MailMessage m = new MailMessage();
            m.From = from;
            m.To = to;
            m.Subject = subject;
            m.Body = messageText;
            Send(m);
        }
    }

#endif // !FEATURE_PAL
    private static String _server;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static String SmtpServer {
        get {
            String s = _server;
            return (s != null) ? s : String.Empty;
        }

        set {
            _server = value;
        }
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
    [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
    public static void Send(String from, String to, String subject, String messageText) {
        lock (_lockObject) {
#if !FEATURE_PAL // FEATURE_PAL does not enable SmtpMail
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                throw new PlatformNotSupportedException(SR.GetString(SR.RequiresNT));
            }
            else if (!CdoSysHelper.OsSupportsCdoSys()) {
                throw new PlatformNotSupportedException(SR.GetString(SR.SmtpMail_not_supported_on_Win7_and_higher));
            }
            else if (Environment.OSVersion.Version.Major <= 4) {
                CdoNtsHelper.Send(from, to, subject, messageText);
            }
            else {
                CdoSysHelper.Send(from, to, subject, messageText);
            }
#else // !FEATURE_PAL
            throw new NotImplementedException("ROTORTODO");
#endif // !FEATURE_PAL
        }
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
    [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
    public static void Send(MailMessage message) {
        lock (_lockObject) {
#if !FEATURE_PAL // FEATURE_PAL does not enable SmtpMail
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                throw new PlatformNotSupportedException(SR.GetString(SR.RequiresNT));
            }
            else if (!CdoSysHelper.OsSupportsCdoSys()) {
                throw new PlatformNotSupportedException(SR.GetString(SR.SmtpMail_not_supported_on_Win7_and_higher));
            }
            else if (Environment.OSVersion.Version.Major <= 4) {
                CdoNtsHelper.Send(message);
            }
            else {
                CdoSysHelper.Send(message);
            }
#else // !FEATURE_PAL
            throw new NotImplementedException("ROTORTODO");
#endif // !FEATURE_PAL
        }
    }
}

//
// Enums for message elements
//


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[Obsolete("The recommended alternative is System.Net.Mail.MailMessage.IsBodyHtml. http://go.microsoft.com/fwlink/?linkid=14202")]
public enum MailFormat {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Text = 0,       // note - different from CDONTS.NewMail

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Html = 1
}


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[Obsolete("The recommended alternative is System.Net.Mail.MailPriority. http://go.microsoft.com/fwlink/?linkid=14202")]
public enum MailPriority {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Normal = 0,     // note - different from CDONTS.NewMail

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Low = 1,

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    High = 2
}


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[Obsolete("The recommended alternative is System.Net.Mime.TransferEncoding. http://go.microsoft.com/fwlink/?linkid=14202")]
public enum MailEncoding {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    UUEncode = 0,   // note - same as CDONTS.NewMail

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    Base64 = 1
}

/*
 * Immutable struct that holds a single attachment
 */

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[Obsolete("The recommended alternative is System.Net.Mail.Attachment. http://go.microsoft.com/fwlink/?linkid=14202")]
public class MailAttachment {
    private String _filename;
    private MailEncoding _encoding;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public String Filename { get { return _filename; } }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public MailEncoding Encoding { get { return _encoding; } }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public MailAttachment(String filename)
    {
        _filename = filename;
        _encoding = MailEncoding.Base64;
        VerifyFile();
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public MailAttachment(String filename, MailEncoding encoding)
    {
        _filename = filename;
        _encoding = encoding;
        VerifyFile();
    }

    private void VerifyFile() {
        try {
            File.Open(_filename, FileMode.Open, FileAccess.Read,  FileShare.Read).Close();
        }
        catch {
            throw new HttpException(SR.GetString(SR.Bad_attachment, _filename));
        }
    }
}

/*
 * Struct that holds a single message
 */

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[Obsolete("The recommended alternative is System.Net.Mail.MailMessage. http://go.microsoft.com/fwlink/?linkid=14202")]
public class MailMessage {
    Hashtable _headers = new Hashtable();
    Hashtable _fields = new Hashtable();
    ArrayList _attachments = new ArrayList();

    string from;
    string to;
    string cc;
    string bcc;
    string subject;
    MailPriority priority = MailPriority.Normal;
    string urlContentBase;
    string urlContentLocation;
    string body;
    MailFormat bodyFormat = MailFormat.Text;
    Encoding bodyEncoding = Encoding.Default;



    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string From {
        get {
            return from;
        }
        set {
            from = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string To {
        get {
            return to;
        }
        set {
            to = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string Cc {
        get {
            return cc;
        }
        set {
            cc = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string Bcc {
        get {
            return bcc;
        }
        set {
            bcc = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string Subject {
        get {
            return subject;
        }
        set {
            subject = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public MailPriority Priority {
        get {
            return priority;
        }
        set {
            priority = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string UrlContentBase {
        get {
            return urlContentBase;
        }
        set {
            urlContentBase = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string UrlContentLocation {
        get {
            return urlContentLocation;
        }
        set {
            urlContentLocation = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public string Body {
        get {
            return body;
        }
        set {
            body = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public MailFormat BodyFormat {
        get {
            return bodyFormat;
        }
        set {
            bodyFormat = value;
        }
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public Encoding BodyEncoding {
        get {
            return bodyEncoding;
        }
        set {
            bodyEncoding = value;
        }
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public IDictionary  Headers { get { return _headers; } }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public IDictionary  Fields { get { return _fields; } }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public IList        Attachments { get { return _attachments; } }

}


}
