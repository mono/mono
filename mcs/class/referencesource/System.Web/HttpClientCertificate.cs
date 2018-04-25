//------------------------------------------------------------------------------
// <copyright file="HttpClientCertificate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Client Certificate 
 * 
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Util;


    /// <devdoc>
    ///    <para>The HttpClientCertificate collection retrieves the certification fields 
    ///       (specified in the X.509 standard) from a request issued by the Web browser.</para>
    ///    <para>If a Web browser uses the SSL3.0/PCT1 protocol (in other words, it uses a URL 
    ///       starting with https:// instead of http://) to connect to a server and the server
    ///       requests certification, the browser sends the certification fields.</para>
    /// </devdoc>
    public class HttpClientCertificate  : NameValueCollection {
        /////////////////////////////////////////////////////////////////////////////
        // Properties

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    String    Cookie { get { return _Cookie;}}

        /// <devdoc>
        /// A string containing the binary stream of the entire certificate content in ASN.1 format.
        /// </devdoc>
        public    byte []   Certificate { get { return _Certificate;}}

        /// <devdoc>
        ///    <para>A set of flags that provide additional client certificate information. </para>
        /// </devdoc>
        public    int       Flags { get { return _Flags;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    int       KeySize { get { return _KeySize;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    int       SecretKeySize { get { return _SecretKeySize;}}

        /// <devdoc>
        ///    <para>A string that contains a list of subfield values containing information about 
        ///       the issuer of the certificate.</para>
        /// </devdoc>
        public    String    Issuer { get { return _Issuer;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    String    ServerIssuer { get { return _ServerIssuer;}}

        /// <devdoc>
        ///    <para>A string that contains a list of subfield values. The subfield values contain 
        ///       information about the subject of the certificate. If this value is specified
        ///       without a <paramref name="SubField"/>, the ClientCertificate collection returns a
        ///       comma-separated list of subfields. For example, C=US, O=Msft, and so on.</para>
        /// </devdoc>
        public    String    Subject { get { return _Subject;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    String    ServerSubject { get { return _ServerSubject;}}

        /// <devdoc>
        ///    <para>A string that contains the certification serial number as an ASCII 
        ///       representation of hexadecimal bytes separated by hyphens (-). For example,
        ///       04-67-F3-02.</para>
        /// </devdoc>
        public    String    SerialNumber { get { return _SerialNumber;}}

        /// <devdoc>
        ///    <para>A date specifying when the certificate becomes valid. This date varies with 
        ///       international settings. </para>
        /// </devdoc>
        public    DateTime  ValidFrom { get { return _ValidFrom;}}

        /// <devdoc>
        ///    <para>A date specifying when the certificate expires. The year value is displayed 
        ///       as a four-digit number.</para>
        /// </devdoc>
        public    DateTime  ValidUntil { get { return _ValidUntil;}}


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    int       CertEncoding    { get { return _CertEncoding;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    byte []   PublicKey       { get { return _PublicKey;}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    byte []   BinaryIssuer    { get { return _BinaryIssuer;}}


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    bool      IsPresent       { get { return((_Flags & 0x1) == 1);}}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public    bool      IsValid         { get { return((_Flags & 0x2) == 0);}}

        /////////////////////////////////////////////////////////////////////////////
        // Ctor
        internal HttpClientCertificate(HttpContext context) {
            String flags    = context.Request.ServerVariables["CERT_FLAGS"];
            if (!String.IsNullOrEmpty(flags))
                _Flags = Int32.Parse(flags, CultureInfo.InvariantCulture);
            else
                _Flags = 0;

            if (IsPresent == false)
                return;


            _Cookie         = context.Request.ServerVariables["CERT_COOKIE"];
            _Issuer         = context.Request.ServerVariables["CERT_ISSUER"];
            _ServerIssuer   = context.Request.ServerVariables["CERT_SERVER_ISSUER"];
            _Subject        = context.Request.ServerVariables["CERT_SUBJECT"];
            _ServerSubject  = context.Request.ServerVariables["CERT_SERVER_SUBJECT"];
            _SerialNumber   = context.Request.ServerVariables["CERT_SERIALNUMBER"];

            _Certificate    = context.WorkerRequest.GetClientCertificate();
            _ValidFrom      = context.WorkerRequest.GetClientCertificateValidFrom();
            _ValidUntil     = context.WorkerRequest.GetClientCertificateValidUntil();
            _BinaryIssuer   = context.WorkerRequest.GetClientCertificateBinaryIssuer();
            _PublicKey      = context.WorkerRequest.GetClientCertificatePublicKey();
            _CertEncoding   = context.WorkerRequest.GetClientCertificateEncoding();

            String keySize  = context.Request.ServerVariables["CERT_KEYSIZE"];
            String skeySize = context.Request.ServerVariables["CERT_SECRETKEYSIZE"];

            if (!String.IsNullOrEmpty(keySize))
                _KeySize = Int32.Parse(keySize, CultureInfo.InvariantCulture);
            if (!String.IsNullOrEmpty(skeySize))
                _SecretKeySize = Int32.Parse(skeySize, CultureInfo.InvariantCulture);

            base.Add("ISSUER",        null);
            base.Add("SUBJECTEMAIL", null);
            base.Add("BINARYISSUER", null);
            base.Add("FLAGS",         null);
            base.Add("ISSUERO",       null);
            base.Add("PUBLICKEY",     null);
            base.Add("ISSUEROU",      null);
            base.Add("ENCODING",      null);
            base.Add("ISSUERCN",      null);
            base.Add("SERIALNUMBER",  null);
            base.Add("SUBJECT",       null);
            base.Add("SUBJECTCN",     null);
            base.Add("CERTIFICATE",   null);
            base.Add("SUBJECTO",      null);
            base.Add("SUBJECTOU",     null);
            base.Add("VALIDUNTIL",    null);
            base.Add("VALIDFROM",     null);
        }


        /// <devdoc>
        ///    <para>Allows access to individual items in the collection by name.</para>
        /// </devdoc>
        public override String Get(String field)
        { 
            if (field == null)
                return String.Empty;

            field = field.ToLower(CultureInfo.InvariantCulture);

            switch (field) {
                case "cookie":
                    return Cookie;

                case "flags":
                    return Flags.ToString("G", CultureInfo.InvariantCulture);

                case "keysize":
                    return KeySize.ToString("G", CultureInfo.InvariantCulture);

                case "secretkeysize":
                    return SecretKeySize.ToString(CultureInfo.InvariantCulture);

                case "issuer":
                    return Issuer;

                case "serverissuer":
                    return ServerIssuer;

                case "subject":
                    return Subject;

                case "serversubject":
                    return ServerSubject;

                case "serialnumber":
                    return SerialNumber;

                case "certificate":
                    return System.Text.Encoding.Default.GetString(Certificate);

                case "binaryissuer":
                    return System.Text.Encoding.Default.GetString(BinaryIssuer);

                case "publickey":
                    return System.Text.Encoding.Default.GetString(PublicKey);

                case "encoding":
                    return CertEncoding.ToString("G", CultureInfo.InvariantCulture);

                case "validfrom":
                    return HttpUtility.FormatHttpDateTime(ValidFrom);

                case "validuntil":
                    return HttpUtility.FormatHttpDateTime(ValidUntil);
            }

            if (StringUtil.StringStartsWith(field, "issuer"))
                return ExtractString(Issuer, field.Substring(6));

            if (StringUtil.StringStartsWith(field, "subject")) {
                if (field.Equals("subjectemail"))
                    return ExtractString(Subject, "e");
                else
                    return ExtractString(Subject, field.Substring(7));
            }

            if (StringUtil.StringStartsWith(field, "serversubject"))
                return ExtractString(ServerSubject, field.Substring(13));

            if (StringUtil.StringStartsWith(field, "serverissuer"))
                return ExtractString(ServerIssuer, field.Substring(12));

            return String.Empty;
        }

        /////////////////////////////////////////////////////////////////////////////
        // Private data
        private    String    _Cookie              = String.Empty;
        private    byte []   _Certificate         = new byte[0];
        private    int       _Flags;
        private    int       _KeySize;
        private    int       _SecretKeySize;
        private    String    _Issuer              = String.Empty;
        private    String    _ServerIssuer        = String.Empty;
        private    String    _Subject             = String.Empty;
        private    String    _ServerSubject       = String.Empty;
        private    String    _SerialNumber        = String.Empty;
        private    DateTime  _ValidFrom           = DateTime.Now;
        private    DateTime  _ValidUntil          = DateTime.Now;
        private    int       _CertEncoding;
        private    byte []   _PublicKey           = new byte[0];
        private    byte []   _BinaryIssuer        = new byte[0];

        private String ExtractString(String strAll, String strSubject) {
            if (strAll == null || strSubject == null)
                return String.Empty;

            String strReturn = String.Empty;
            int    iStart    = 0;
            String strAllL   = strAll.ToLower(CultureInfo.InvariantCulture);

            while (iStart < strAllL.Length) {
                iStart = strAllL.IndexOf(strSubject + "=", iStart, StringComparison.Ordinal);
                if (iStart < 0)
                    return strReturn;
                if (strReturn.Length > 0)
                    strReturn += ";";

                iStart += strSubject.Length + 1;        
                int iEnd = 0;
                if (strAll[iStart]=='"') {
                    iStart++;
                    iEnd  = strAll.IndexOf('"' , iStart);
                }
                else
                    iEnd  = strAll.IndexOf(',' , iStart);

                if (iEnd < 0)
                    iEnd = strAll.Length;

                strReturn += strAll.Substring(iStart, iEnd - iStart);
                iStart = iEnd + 1;
            }

            return strReturn;
        }
    }
}

