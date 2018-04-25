//------------------------------------------------------------------------------
// <copyright file="HttpBrowserCapabilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Built-in browser caps object
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web {
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Web.Configuration;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para> Enables the server to compile
    ///       information on the capabilities of the browser running on the client.</para>
    /// </devdoc>
    public class HttpBrowserCapabilities : HttpCapabilitiesBase {

        // Lazy computation
        // NOTE: The methods below are designed to work on multiple threads
        // without a need for synchronization. Do NOT do something like replace
        // all the _haveX booleans with bitfields or something similar, because
        // the methods depend on the fact that "_haveX = true" is atomic.


        /// <devdoc>
        ///    <para>Returns the .NET Common Language Runtime version running
        ///         on the client.  If no CLR version is specified on the
        ///         User-Agent returns new Version(), which is 0,0.</para>
        /// </devdoc>
/*        public Version ClrVersion {
            get {
                ClrVersionEnsureInit();
                return _clrVersion;
            }
        }


        /// <devdoc>
        ///    <para>Returns all versions of the .NET CLR running on the
        ///         client.  If no CLR version is specified on the User-Agent
        ///         returns an array containing a single empty Version object,
        ///         which is 0,0.</para>
        /// </devdoc>
        public Version [] GetClrVersions() {
            ClrVersionEnsureInit();
            return _clrVersions;
        }


        private void ClrVersionEnsureInit() {
            if (!_haveClrVersion) {
                Regex regex = new Regex("\\.NET CLR (?'clrVersion'[0-9\\.]*)");
                MatchCollection matches = regex.Matches(this[String.Empty]);

                if (matches.Count == 0) {
                    Version version = new Version();
                    Version [] clrVersions = new Version [1] {version};
                    _clrVersions = clrVersions;
                    _clrVersion = version;
                }
                else {
                    ArrayList versionList = new ArrayList();

                    foreach (Match match in matches) {
                        Version version = new Version(match.Groups["clrVersion"].Value);
                        versionList.Add(version);
                    }

                    versionList.Sort();

                    Version [] versions = (Version []) versionList.ToArray(typeof(Version));

                    _clrVersions = versions;
                    _clrVersion = versions[versions.Length - 1];
                }

                _haveClrVersion = true;
            }
        }



        /// <devdoc>
        ///    <para>Returns the name of the client browser and its major version number. For example, "Microsoft Internet Explorer version
        ///       5".</para>
        /// </devdoc>
        public string  Type {
            get {
                if (!_havetype) {
                    _type = this["type"];
                    _havetype = true;
                }
                return _type;
            }
        }

        /// <devdoc>
        ///    <para>Browser string in User Agent (for example: "IE").</para>
        /// </devdoc>
        public string  Browser {
            get {
                if (!_havebrowser) {
                    _browser = this["browser"];
                    _havebrowser = true;
                }
                return _browser;
            }
        }

        /// <devdoc>
        ///    <para>Returns the major version number + minor version number
        ///       of the client browser; for example: "5.5".</para>
        /// </devdoc>
        public string  Version {
            get {
                if (!_haveversion) {
                    _version =  this["version"];
                    _haveversion = true;
                }
                return _version;
            }
        }

        /// <devdoc>
        ///    <para>Returns the major version number of the client browser; for example: 3.</para>
        /// </devdoc>
        public int MajorVersion {
            get {
                if (!_havemajorversion) {
                    try {
                        _majorversion = int.Parse(this["majorversion"], CultureInfo.InvariantCulture);
                        _havemajorversion = true;
                    }
                    catch (FormatException e) {
                        throw BuildParseError(e, "majorversion");
                    }
                }
                return _majorversion;
            }
        }

        Exception BuildParseError(Exception e, string capsKey) {
            string message = SR.GetString(SR.Invalid_string_from_browser_caps, e.Message, capsKey, this[capsKey]);

            // to show ConfigurationException in stack trace
            ConfigurationException configEx = new ConfigurationErrorsException(message, e);

            // I want it to look like an unhandled exception
            HttpUnhandledException httpUnhandledEx = new HttpUnhandledException(null, null);

            // but show message from outer exception (it normally shows the inner-most)
            httpUnhandledEx.SetFormatter(new UseLastUnhandledErrorFormatter(configEx));

            return httpUnhandledEx;
        }

        bool CapsParseBool(string capsKey) {
            try {
                return bool.Parse(this[capsKey]);
            }
            catch (FormatException e) {
                throw BuildParseError(e, capsKey);
            }
        }


        /// <devdoc>
        ///    <para>Returns the minor version number of the client browser; for example: .01.</para>
        /// </devdoc>
        public double MinorVersion {
            get {
                if (!_haveminorversion) {
                    try {
                        // see ASURT 11176
                        _minorversion = double.Parse(
                            this["minorversion"],
                            NumberStyles.Float | NumberStyles.AllowDecimalPoint,
                            NumberFormatInfo.InvariantInfo);
                        _haveminorversion = true;
                    }
                    catch (FormatException e) {
                        throw BuildParseError(e, "majorversion");
                    }
                }
                return _minorversion;
            }
        }

        /// <devdoc>
        ///    <para>Returns the platform's name; for example, "Win32".</para>
        /// </devdoc>
        public string  Platform {
            get {
                if (!_haveplatform) {
                    _platform = this["platform"];
                    _haveplatform = true;
                }
                return _platform;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type TagWriter {
            get {
                try {
                    if (!_havetagwriter) {
                        string tagWriter = this["tagwriter"];
                        if (String.IsNullOrEmpty(tagWriter)) {
                            _tagwriter = null;
                        }
                        else if (string.Compare(tagWriter, typeof(System.Web.UI.HtmlTextWriter).FullName, false, CultureInfo.InvariantCulture) == 0) {
                            _tagwriter=  typeof(System.Web.UI.HtmlTextWriter);
                        }
                        else {
                            _tagwriter = System.Type.GetType(tagWriter, true /*throwOnError*///);
/*                        }
                        _havetagwriter = true;
                    }
                }
                catch (Exception e) {
                    throw BuildParseError(e, "tagwriter");
                }

                return _tagwriter;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Version EcmaScriptVersion {
            get {
                if (!_haveecmascriptversion) {
                    _ecmascriptversion = new Version(this["ecmascriptversion"]);
                    _haveecmascriptversion = true;
                }
                return _ecmascriptversion;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Version MSDomVersion {
            get {
                if (!_havemsdomversion) {
                    _msdomversion = new Version(this["msdomversion"]);
                    _havemsdomversion = true;
                }
                return _msdomversion;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Version W3CDomVersion {
            get {
                if (!_havew3cdomversion) {
                    _w3cdomversion = new Version(this["w3cdomversion"]);
                    _havew3cdomversion = true;
                }
                return _w3cdomversion;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the browser client is in beta.</para>
        /// </devdoc>
        public bool Beta {
            get {
                if (!_havebeta) {
                    _beta = CapsParseBool("beta");
                    _havebeta = true;
                }
                return _beta;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser is a Web-crawling search engine.</para>
        /// </devdoc>
        public bool Crawler {
            get {
                if (!_havecrawler) {
                    _crawler = CapsParseBool("crawler");
                    _havecrawler = true;
                }
                return _crawler;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client is an AOL branded browser.</para>
        /// </devdoc>
        public bool AOL {
            get {
                if (!_haveaol) {
                    _aol = CapsParseBool("aol");
                    _haveaol = true;
                }
                return _aol;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client is a Win16-based machine.</para>
        /// </devdoc>
        public bool Win16 {
            get {
                if (!_havewin16) {
                    _win16 = CapsParseBool("win16");
                    _havewin16 = true;
                }
                return _win16;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client is a Win32-based machine.</para>
        /// </devdoc>
        public bool Win32 {
            get {
                if (!_havewin32) {
                    _win32 = CapsParseBool("win32");
                    _havewin32 = true;
                }
                return _win32;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports HTML frames.</para>
        /// </devdoc>
        public bool Frames {
            get {
                if (!_haveframes) {
                    _frames = CapsParseBool("frames");
                    _haveframes = true;
                }
                return _frames;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports the bold tag.</para>
        /// </devdoc>
        public bool SupportsBold {
            get {
                if (!_havesupportsbold) {
                    _supportsbold = CapsParseBool("supportsBold");
                    _havesupportsbold = true;
                }
                return _supportsbold;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports the italic tag.</para>
        /// </devdoc>
        public bool SupportsItalic {
            get {
                if (!_havesupportsitalic) {
                    _supportsitalic = CapsParseBool("supportsItalic");
                    _havesupportsitalic = true;
                }
                return _supportsitalic;
            }
        }



        /// <devdoc>
        ///    <para>Indicates whether the client browser supports tables.</para>
        /// </devdoc>
        public bool Tables {
            get {
                if (!_havetables) {
                    _tables = CapsParseBool("tables");
                    _havetables = true;
                }
                return _tables;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports cookies.</para>
        /// </devdoc>
        public bool Cookies {
            get {
                if (!_havecookies) {
                    _cookies = CapsParseBool("cookies");
                    _havecookies = true;
                }
                return _cookies;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports VBScript.</para>
        /// </devdoc>
        public bool VBScript {
            get {
                if (!_havevbscript) {
                    _vbscript = CapsParseBool("vbscript");
                    _havevbscript = true;
                }
                return _vbscript;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports JavaScript.</para>
        /// </devdoc>
        [Obsolete("Use EcmaScriptVersion instead of Javascript.  Major versions greater than or equal to one imply javascript support.")]
        public bool JavaScript {
            get {
                if (!_havejavascript) {
                    _javascript=CapsParseBool("javascript");
                    _havejavascript = true;
                }
                return _javascript;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports Java Applets.</para>
        /// </devdoc>
        public bool JavaApplets {
            get {
                if (!_havejavaapplets) {
                    _javaapplets=CapsParseBool("javaapplets");
                    _havejavaapplets = true;
                }
                return _javaapplets;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports ActiveX Controls.</para>
        /// </devdoc>
        public bool ActiveXControls {
            get {
                if (!_haveactivexcontrols) {
                    _activexcontrols=CapsParseBool("activexcontrols");
                    _haveactivexcontrols = true;
                }
                return _activexcontrols;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports background sounds.</para>
        /// </devdoc>
        public bool BackgroundSounds {
            get {
                if (!_havebackgroundsounds) {
                    _backgroundsounds=CapsParseBool("backgroundsounds");
                    _havebackgroundsounds = true;
                }
                return _backgroundsounds;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the client browser supports Channel Definition Format (CDF) for webcasting.</para>
        /// </devdoc>
        public bool CDF {
            get {
                if (!_havecdf) {
                    _cdf = CapsParseBool("cdf");
                    _havecdf = true;
                }
                return _cdf;
            }
        }


        private string  _type;
        private string  _browser;
        private string  _version;
        private int     _majorversion;
        private double  _minorversion;
        private string  _platform;
        private Type    _tagwriter;
        private Version _ecmascriptversion;
        private Version _msdomversion;
        private Version _w3cdomversion;
        private Version _clrVersion;
        private Version [] _clrVersions;

        private bool _beta;
        private bool _crawler;
        private bool _aol;
        private bool _win16;
        private bool _win32;

        private bool _frames;
        private bool _supportsbold;
        private bool _supportsitalic;
        private bool _tables;
        private bool _cookies;
        private bool _vbscript;
        private bool _javascript;
        private bool _javaapplets;
        private bool _activexcontrols;
        private bool _backgroundsounds;
        private bool _cdf;

        private bool _havetype;
        private bool _havebrowser;
        private bool _haveversion;
        private bool _havemajorversion;
        private bool _haveminorversion;
        private bool _haveplatform;
        private bool _havetagwriter;
        private bool _haveecmascriptversion;
        private bool _havemsdomversion;
        private bool _havew3cdomversion;
        private bool _haveClrVersion;

        private bool _havebeta;
        private bool _havecrawler;
        private bool _haveaol;
        private bool _havewin16;
        private bool _havewin32;

        private bool _haveframes;
        private bool _havesupportsbold;
        private bool _havesupportsitalic;
        private bool _havetables;
        private bool _havecookies;
        private bool _havevbscript;
        private bool _havejavascript;
        private bool _havejavaapplets;
        private bool _haveactivexcontrols;
        private bool _havebackgroundsounds;
        private bool _havecdf;
*/
    }
}
