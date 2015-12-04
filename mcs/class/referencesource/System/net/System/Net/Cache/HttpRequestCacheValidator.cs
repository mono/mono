/*++
Copyright (c) Microsoft Corporation

Module Name:

    HttpRequestCacheValidator.cs

Abstract:
    The class implements HTTP Caching validators as per RFC2616


Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

    Jan 25 2004 - Changed the visibility of the class from public to internal.

--*/
namespace System.Net.Cache {
    using System;
    using System.Net;
    using System.IO;
    using System.Collections;
    using System.Text;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Threading;


    /// <summary> The class represents an adavanced way for an application to control caching protocol </summary>
    internal class HttpRequestCacheValidator: RequestCacheValidator {
        internal const string Warning_110 = "110 Response is stale";
        internal const string Warning_111 = "111 Revalidation failed";
        internal const string Warning_112 = "112 Disconnected operation";
        internal const string Warning_113 = "113 Heuristic expiration";

        private struct RequestVars {
            internal HttpMethod   Method;
            internal bool         IsCacheRange;
            internal bool         IsUserRange;
            internal string       IfHeader1;
            internal string       Validator1;
            internal string       IfHeader2;
            internal string       Validator2;
        }

        private HttpRequestCachePolicy m_HttpPolicy;

        private HttpStatusCode  m_StatusCode;
        private string          m_StatusDescription;
        private Version         m_HttpVersion;
        private WebHeaderCollection m_Headers;
        private NameValueCollection m_SystemMeta;

        private bool            m_DontUpdateHeaders;
        private bool            m_HeuristicExpiration;

        private Vars            m_CacheVars;
        private Vars            m_ResponseVars;
        private RequestVars     m_RequestVars;

        private struct Vars {
            internal DateTime         Date;
            internal DateTime         Expires;
            internal DateTime         LastModified;
            internal long             EntityLength;
            internal TimeSpan         Age;
            internal TimeSpan         MaxAge;
            internal ResponseCacheControl CacheControl;
            internal long             RangeStart;
            internal long             RangeEnd;

            internal void Initialize() {
                EntityLength = RangeStart = RangeEnd = -1;
                Date = DateTime.MinValue;
                Expires = DateTime.MinValue;
                LastModified = DateTime.MinValue;
                Age = TimeSpan.MinValue;
                MaxAge = TimeSpan.MinValue;
            }
        }


        //public
        internal HttpStatusCode CacheStatusCode           {get{return m_StatusCode;}          set{m_StatusCode = value;}}
        //public
        internal string         CacheStatusDescription    {get{return m_StatusDescription;}   set{m_StatusDescription = value;}}
        //public
        internal Version        CacheHttpVersion          {get{return m_HttpVersion;}         set{m_HttpVersion = value;}}

        //public
        internal WebHeaderCollection CacheHeaders         {get{return m_Headers;}             set{m_Headers = value;}}

        //public
        internal new HttpRequestCachePolicy   Policy      {
                                                            get {
                                                                if(m_HttpPolicy != null) return m_HttpPolicy;
                                                                m_HttpPolicy = base.Policy as HttpRequestCachePolicy;
                                                                if(m_HttpPolicy != null) return m_HttpPolicy;
                                                                // promote base policy to Http one
                                                                m_HttpPolicy = new HttpRequestCachePolicy((HttpRequestCacheLevel)base.Policy.Level);
                                                                return m_HttpPolicy;
                                                            }
                                                        }

        internal NameValueCollection SystemMeta         {get{return m_SystemMeta;}                  set{m_SystemMeta = value;}}
        internal HttpMethod   RequestMethod             {get{return m_RequestVars.Method;}          set{m_RequestVars.Method = value;}}
        internal bool         RequestRangeCache         {get{return m_RequestVars.IsCacheRange;}    set{m_RequestVars.IsCacheRange = value;}}
        internal bool         RequestRangeUser          {get{return m_RequestVars.IsUserRange;}     set{m_RequestVars.IsUserRange = value;}}
        internal string       RequestIfHeader1          {get{return m_RequestVars.IfHeader1;}       set{m_RequestVars.IfHeader1 = value;}}
        internal string       RequestValidator1         {get{return m_RequestVars.Validator1;}      set{m_RequestVars.Validator1 = value;}}
        internal string       RequestIfHeader2          {get{return m_RequestVars.IfHeader2;}       set{m_RequestVars.IfHeader2 = value;}}
        internal string       RequestValidator2         {get{return m_RequestVars.Validator2;}      set{m_RequestVars.Validator2 = value;}}

        internal bool         CacheDontUpdateHeaders    {get{return m_DontUpdateHeaders;}           set{m_DontUpdateHeaders = value;}}

        internal DateTime      CacheDate               {get{return m_CacheVars.Date;}              set{m_CacheVars.Date = value;}}
        internal DateTime      CacheExpires            {get{return m_CacheVars.Expires;}           set{m_CacheVars.Expires = value;}}
        internal DateTime      CacheLastModified       {get{return m_CacheVars.LastModified;}      set{m_CacheVars.LastModified = value;}}
        internal long          CacheEntityLength       {get{return m_CacheVars.EntityLength ;}     set{m_CacheVars.EntityLength  = value;}}
        internal TimeSpan      CacheAge                {get{return m_CacheVars.Age;}               set{m_CacheVars.Age = value;}}
        internal TimeSpan      CacheMaxAge             {get{return m_CacheVars.MaxAge;}            set{m_CacheVars.MaxAge = value;}}
        internal bool          HeuristicExpiration     {get{return m_HeuristicExpiration;}         set{m_HeuristicExpiration = value;}}

        internal ResponseCacheControl     CacheCacheControl    {get{return m_CacheVars.CacheControl;}      set{m_CacheVars.CacheControl = value;}}

        internal DateTime      ResponseDate            {get{return m_ResponseVars.Date;}         set{m_ResponseVars.Date = value;}}
        internal DateTime      ResponseExpires         {get{return m_ResponseVars.Expires;}      set{m_ResponseVars.Expires = value;}}
        internal DateTime      ResponseLastModified    {get{return m_ResponseVars.LastModified;} set{m_ResponseVars.LastModified = value;}}
        internal long          ResponseEntityLength    {get{return m_ResponseVars.EntityLength ;}set{m_ResponseVars.EntityLength  = value;}}
        internal long          ResponseRangeStart      {get{return m_ResponseVars.RangeStart;}   set{m_ResponseVars.RangeStart = value;}}
        internal long          ResponseRangeEnd        {get{return m_ResponseVars.RangeEnd;}     set{m_ResponseVars.RangeEnd = value;}}
        internal TimeSpan      ResponseAge             {get{return m_ResponseVars.Age;}          set{m_ResponseVars.Age = value;}}
        internal ResponseCacheControl  ResponseCacheControl {get{return m_ResponseVars.CacheControl;} set{m_ResponseVars.CacheControl = value;}}

        //
        private void ZeroPrivateVars()
        {
            // Set default values for private members here
            m_RequestVars = new RequestVars();

            m_HttpPolicy        = null;
            m_StatusCode        = (HttpStatusCode)0;
            m_StatusDescription = null;
            m_HttpVersion       = null;
            m_Headers           = null;
            m_SystemMeta        = null;
            m_DontUpdateHeaders = false;
            m_HeuristicExpiration = false;

            m_CacheVars   = new Vars();
            m_CacheVars.Initialize();

            m_ResponseVars= new Vars();
            m_ResponseVars.Initialize();
        }

        //public
        internal override RequestCacheValidator CreateValidator()
        {
            return new HttpRequestCacheValidator(StrictCacheErrors, UnspecifiedMaxAge);
        }

        /*
        //public
        // Consider removing.
        internal HttpRequestCacheValidator(): base()
        {
        }
        */

        //public
        internal HttpRequestCacheValidator(bool strictCacheErrors, TimeSpan unspecifiedMaxAge): base(strictCacheErrors, unspecifiedMaxAge)
        {
        }

        //
        // This validation method is called first and before any Cache access is done.
        // Given the request instance the code has to decide whether the request is ever suitable for caching.
        //
        // Returns:
        // Continue           = Proceed to the next protocol stage.
        // DoNotTakeFromCache = Don't used caches value for this request
        // DoNotUseCache      = Cache is not used for this request and response is not cached.
        protected internal override CacheValidationStatus ValidateRequest() {

            // cleanup context after previous  request
            ZeroPrivateVars();

            string method = Request.Method.ToUpper(CultureInfo.InvariantCulture);
            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_request_method, method));

            switch (method) {
                case "GET" :    RequestMethod = HttpMethod.Get;      break;
                case "POST":    RequestMethod = HttpMethod.Post;     break;
                case "HEAD":    RequestMethod = HttpMethod.Head;     break;
                case "PUT" :    RequestMethod = HttpMethod.Put;      break;
                case "DELETE":  RequestMethod = HttpMethod.Delete;   break;
                case "OPTIONS": RequestMethod = HttpMethod.Options;  break;
                case "TRACE":   RequestMethod = HttpMethod.Trace;    break;
                case "CONNECT": RequestMethod = HttpMethod.Connect;  break;
                default:        RequestMethod = HttpMethod.Other;    break;
            }

            // Apply our best knowledge of HTTP caching and return the result
            // that can be hooked up and revised by the upper level
            return Rfc2616.OnValidateRequest(this);
        }

        //
        // This validation method is called after caching protocol has retrieved the metadata of a cached entry.
        // Given the cached entry context, the request instance and the effective caching policy,
        // the handler has to decide whether a cached item can be considered as fresh.
        protected internal override CacheFreshnessStatus ValidateFreshness()
        {

            // Transfer cache entry metadata into status line and headers.
            string s = ParseStatusLine();

            if(Logging.On) {
                if ((int) CacheStatusCode == 0) {
                    Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_http_status_parse_failure, (s == null ? "null" : s)));
                }
                else {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_http_status_line, (CacheHttpVersion != null ? CacheHttpVersion.ToString() : "null"), (int)CacheStatusCode, CacheStatusDescription));
                }

            }

            CreateCacheHeaders((int)CacheStatusCode != 0);
            CreateSystemMeta();

            // We will need quick access to cache-control and other headers coming with the cached item
            FetchHeaderValues(true);

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_cache_control, CacheCacheControl.ToString()));

            // Now we try to apply our best knowledge of HTTP caching and return the result
            // that can be hooked up and revised on the upper level
            return Rfc2616.OnValidateFreshness(this);
        }

        /// <remarks>  This method may add headers under the "Warning" header name </remarks>
        protected internal override CacheValidationStatus ValidateCache() {

            if (this.Policy.Level != HttpRequestCacheLevel.Revalidate && base.Policy.Level >= RequestCacheLevel.Reload)
            {
                // For those policies cache is never returned
                GlobalLog.Assert("OnValidateCache()", "This validator should not be called for policy = " + Policy.ToString());
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_validator_invalid_for_policy, Policy.ToString()));
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            // First check is do we have a cached entry at all?
            // Also we include some very special case where cache got a 304 (NotModified) response somehow
            if (CacheStream == Stream.Null || (int)CacheStatusCode == 0 || CacheStatusCode == HttpStatusCode.NotModified)
            {
                if (this.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                {
                    // Throw because entry was not found and it's cache-only policy
                    FailRequest(WebExceptionStatus.CacheEntryNotFound);
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            if (RequestMethod == HttpMethod.Head)
            {
                // For a HEAD request we release the cache entry stream asap since we will have to suppress it anyway
                CacheStream.Close();
                CacheStream = new SyncMemoryStream(new byte[] {});
            }

            // Apply our best knowledge of HTTP caching and return the result
            // that can be hooked up and revised by the upper level

            CacheValidationStatus result = CacheValidationStatus.DoNotTakeFromCache;

            //
            // Before request submission validation
            //

            // If we return from cache we should remove existing 1xx warnings
            RemoveWarnings_1xx();

            // default values for a response from cache.
            CacheStreamOffset       = 0;
            CacheStreamLength       = CacheEntry.StreamSize;

            result = Rfc2616.OnValidateCache(this);
            if (result != CacheValidationStatus.ReturnCachedResponse && this.Policy.Level == HttpRequestCacheLevel.CacheOnly) {
                // Throw because entry was not found and it's cache-only policy
                FailRequest(WebExceptionStatus.CacheEntryNotFound);
            }

            if (result == CacheValidationStatus.ReturnCachedResponse)
            {
                if (CacheFreshnessStatus == CacheFreshnessStatus.Stale) {
                    CacheHeaders.Add(HttpKnownHeaderNames.Warning, HttpRequestCacheValidator.Warning_110);
                }
                if (base.Policy.Level == RequestCacheLevel.CacheOnly) {
                    CacheHeaders.Add(HttpKnownHeaderNames.Warning, HttpRequestCacheValidator.Warning_112);
                }
                if (HeuristicExpiration && (int)CacheAge.TotalSeconds >= 24*3600) {
                    CacheHeaders.Add(HttpKnownHeaderNames.Warning, HttpRequestCacheValidator.Warning_113);
                }
            }

            if (result == CacheValidationStatus.DoNotTakeFromCache) {
                // We signal that current cache entry can be only replaced and not updated
                CacheStatusCode = (HttpStatusCode) 0;
            }
            else if (result == CacheValidationStatus.ReturnCachedResponse) {
                CacheHeaders[HttpKnownHeaderNames.Age] = ((int)(CacheAge.TotalSeconds)).ToString(NumberFormatInfo.InvariantInfo);
            }
            return result;
        }
        //
        // This is (optionally) called after receiveing a live response
        //
        protected internal override CacheValidationStatus RevalidateCache()
        {
            if (this.Policy.Level != HttpRequestCacheLevel.Revalidate && base.Policy.Level >= RequestCacheLevel.Reload)
            {
                // For those policies cache is never returned
                GlobalLog.Assert("RevalidateCache()", "This validator should not be called for policy = " + Policy.ToString());
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_validator_invalid_for_policy, Policy.ToString()));
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            // First check is do we have a cached entry at all?
            // Also we include some very special case where cache got a 304 (NotModified) response somehow
            if (CacheStream == Stream.Null || (int)CacheStatusCode == 0 || CacheStatusCode == HttpStatusCode.NotModified)
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            //
            // This is a second+ time validation after receiving at least one response
            //

            // Apply our best knowledge of HTTP caching and return the result
            // that can be hooked up and revised by the upper level
            CacheValidationStatus result = CacheValidationStatus.DoNotTakeFromCache;

            HttpWebResponse resp = Response as HttpWebResponse;
            if (resp == null)
            {
                // This will result to an application error
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            if (resp.StatusCode >= HttpStatusCode.InternalServerError) {
                // If server returned a 5XX server error
                if (Rfc2616.Common.ValidateCacheOn5XXResponse(this) == CacheValidationStatus.ReturnCachedResponse) {
                    // We can substitute the response from cache
                    if (CacheFreshnessStatus == CacheFreshnessStatus.Stale) {
                        CacheHeaders.Add(HttpKnownHeaderNames.Warning, HttpRequestCacheValidator.Warning_110);
                    }
                    if (HeuristicExpiration && (int)CacheAge.TotalSeconds >= 24*3600) {
                        CacheHeaders.Add(HttpKnownHeaderNames.Warning, HttpRequestCacheValidator.Warning_113);
                    }
                    // We actually failed to reach the origin server hence we don't reset the current Cache Age
                }
            }
            else {

                // if there was already one retry, then cache should not be taken into account
                if (ResponseCount > 1) {
                    result =  CacheValidationStatus.DoNotTakeFromCache;
                }
                else {
                    /*
                    Section 13.2.3:
                    HTTP/1.1 uses the Age response-header to convey the estimated age
                    of the response message when obtained from a cache.
                    The Age field value is the cache's estimate of the amount of time
                    since the response was generated or >>revalidated<< by the origin server.
                    */
                    // Reset Cache Age to be 0 seconds
                    CacheAge = TimeSpan.Zero;
                    result = Rfc2616.Common.ValidateCacheAfterResponse(this, resp);
                }
            }

            if (result == CacheValidationStatus.ReturnCachedResponse)
            {
                CacheHeaders[HttpKnownHeaderNames.Age] = ((int)(CacheAge.TotalSeconds)).ToString(NumberFormatInfo.InvariantInfo);
            }
            return result;
        }

        /// <summary>
        /// <para>
        /// This validation method is responsible to answer whether the live response is sufficient to make
        /// the final decision for caching protocol.
        /// This is useful in case of possible failure or inconsistent results received from
        /// the remote cache.
        /// </para>
        /// </summary>
        /// <remarks>  Invalid response from this method means the request was internally modified and should be retried </remarks>
        protected internal override CacheValidationStatus ValidateResponse() {

            if (this.Policy.Level != HttpRequestCacheLevel.CacheOrNextCacheOnly &&
                this.Policy.Level != HttpRequestCacheLevel.Default &&
                this.Policy.Level != HttpRequestCacheLevel.Revalidate)
            {
                // Those policy levels do not modify requests
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_response_valid_based_on_policy, Policy.ToString()));
                return CacheValidationStatus.Continue;
            }

            // We will need quick access to cache controls coming with the live response
            HttpWebResponse resp = Response as HttpWebResponse;
            if (resp == null) {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_null_response_failure));
                return CacheValidationStatus.Continue;
            }

            FetchHeaderValues(false);
            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, "StatusCode=" + ((int)resp.StatusCode).ToString(CultureInfo.InvariantCulture) + ' ' +resp.StatusCode.ToString() +
                                                      (resp.StatusCode == HttpStatusCode.PartialContent
                                                       ?", Content-Range: " + resp.Headers[HttpKnownHeaderNames.ContentRange]
                                                       :string.Empty)
                                                      );


            // Apply our best knowledge of HTTP caching and return the result
            // that can be hooked up and revised by the upper level
            return Rfc2616.OnValidateResponse(this);
        }

        /// <summary>
        /// <para>
        /// This action handler is responsible for making final decision on whether
        /// a received response can be cached.
        /// </para>
        /// </summary>
        /// <remarks>  Invalid result from this method means the response must not be cached </remarks>
        protected internal override CacheValidationStatus UpdateCache() {

            if (this.Policy.Level == HttpRequestCacheLevel.NoCacheNoStore) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_removed_existing_based_on_policy, Policy.ToString()));
                return CacheValidationStatus.RemoveFromCache;
            }
            if (this.Policy.Level == HttpRequestCacheLevel.CacheOnly) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_updated_based_on_policy, Policy.ToString()));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            if (CacheHeaders == null)
                CacheHeaders = new WebHeaderCollection();

            if (SystemMeta == null)
                SystemMeta = new NameValueCollection(1, CaseInsensitiveAscii.StaticInstance);

            if (ResponseCacheControl == null) {
                //ValidateResponse was not invoked
                FetchHeaderValues(false);
            }

            // Apply our best knowledge of HTTP caching and return the result
            // that can be hooked up and revised by the upper level
            CacheValidationStatus result = Rfc2616.OnUpdateCache(this);

            if (result == CacheValidationStatus.UpdateResponseInformation || result == CacheValidationStatus.CacheResponse)
            {
                FinallyUpdateCacheEntry();
            }
            return result;
        }

        //
        //
        //
        private void FinallyUpdateCacheEntry() {
            // Transfer the context status line back to the metadata

            CacheEntry.EntryMetadata  = null;
            CacheEntry.SystemMetadata = null;

            if (CacheHeaders == null) {
                //must be an entry update without updating the headers
                return;
            }

            CacheEntry.EntryMetadata  = new StringCollection();
            CacheEntry.SystemMetadata = new StringCollection();

            if (CacheHttpVersion == null) {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_invalid_http_version));
                CacheHttpVersion = new Version(1, 0);
            }
            // HTTP/1.1 200 OK
            System.Text.StringBuilder sb = new System.Text.StringBuilder(CacheStatusDescription.Length + 20);
            sb.Append("HTTP/");
            sb.Append(CacheHttpVersion.ToString(2));
            sb.Append(' ');
            sb.Append(((int)CacheStatusCode).ToString(NumberFormatInfo.InvariantInfo));
            sb.Append(' ');
            sb.Append(CacheStatusDescription);

            // Fetch the status line into cache metadata
            CacheEntry.EntryMetadata.Add(sb.ToString());

            UpdateStringCollection(CacheEntry.EntryMetadata,  CacheHeaders, false);

            if (SystemMeta != null)
            {
                UpdateStringCollection(CacheEntry.SystemMetadata, SystemMeta, true);
            }

            // Update other entry values
            if (ResponseExpires != DateTime.MinValue) {
                CacheEntry.ExpiresUtc = ResponseExpires;
            }

            if (ResponseLastModified != DateTime.MinValue)
            {
                CacheEntry.LastModifiedUtc = ResponseLastModified;
            }

            if (this.Policy.Level == HttpRequestCacheLevel.Default)
            {
                    CacheEntry.MaxStale = this.Policy.MaxStale;
            }

            CacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
        }
        //
        //
        //
        private static void UpdateStringCollection(StringCollection result, NameValueCollection cc, bool winInetCompat)
        {
            StringBuilder sb;

            // Transfer headers
            for (int i=0; i < cc.Count; ++i)
            {
                    sb = new StringBuilder(40);
                    string key   = cc.GetKey(i) as string;
                    sb.Append(key).Append(':');

                    string[] val = cc.GetValues(i);
                    if (val.Length != 0)
                    {
                        if (winInetCompat)
                            {sb.Append(val[0]);}
                        else
                            {sb.Append(' ').Append(val[0]);}
                    }

                    for (int j = 1; j < val.Length; ++j)
                    {
                        sb.Append(key).Append(", ").Append(val[j]);
                    }
                    result.Add(sb.ToString());
            }
            // Transfer last \r\n
            result.Add(string.Empty);
        }

        // The format is
        // HTTP/X.Y SP NUMBER SP STRING
        // HTTP/1.1 200 OK
        //
        private string ParseStatusLine() {

            // This will indicate an invalid result
            CacheStatusCode = (HttpStatusCode)0;

            if (CacheEntry.EntryMetadata == null || CacheEntry.EntryMetadata.Count == 0)
            {
                return null;
            }

            string s = CacheEntry.EntryMetadata[0];

            if (s == null) {
                return null;
            }

            int  idx = 0;
            char ch = (char)0;
            while (++idx < s.Length && (ch=s[idx]) != '/') {
                ;
            }

            if (idx == s.Length) {return s;}

            int major = -1;
            int minor = -1;
            int status= -1;

            while (++idx < s.Length && (ch=s[idx]) >= '0' && ch <= '9') {
                major = (major<0? 0: major*10) +(ch - '0');
            }

            if (major < 0 || ch != '.') {return s;}

            while (++idx < s.Length && (ch=s[idx]) >= '0' && ch <= '9') {
                minor = (minor<0? 0: minor*10) + (ch - '0');
            }

            if (minor < 0 || (ch != ' ' && ch != '\t')) {return s;}

            while (++idx < s.Length && ((ch=s[idx]) == ' ' || ch == '\t'))
                ;

            if (idx >= s.Length) {return s;}

            while (ch >= '0' && ch <= '9')
            {
                status = (status<0? 0: status*10) +(ch - '0');
                if (++idx == s.Length)
                    break;
                ch=s[idx];
            }

            if (status < 0 || (idx <= s.Length && (ch != ' ' && ch != '\t'))) {return s;}

            while (idx < s.Length && (s[idx] == ' ' || s[idx] == '\t'))
                ++idx;

            CacheStatusDescription = s.Substring(idx);

            CacheHttpVersion = new Version(major, minor);
            CacheStatusCode = (HttpStatusCode)status;
            return s;
        }
        //
        private void CreateCacheHeaders(bool ignoreFirstString)
        {

            if (CacheHeaders == null)
                CacheHeaders = new WebHeaderCollection();

            if (CacheEntry.EntryMetadata == null || CacheEntry.EntryMetadata.Count == 0)
            {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_http_response_header));
                return;
            }

            string s = ParseNameValues(CacheHeaders, CacheEntry.EntryMetadata, ignoreFirstString?1:0);
            if (s != null)
            {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_http_header_parse_error, s));
                CacheHeaders.Clear();
            }
        }
        //
        private void CreateSystemMeta()
        {
            if (SystemMeta == null)
            {
                SystemMeta = new NameValueCollection((CacheEntry.EntryMetadata == null || CacheEntry.EntryMetadata.Count == 0? 2: CacheEntry.EntryMetadata.Count),
                                                     CaseInsensitiveAscii.StaticInstance);
            }
            if (CacheEntry.EntryMetadata == null || CacheEntry.EntryMetadata.Count == 0)
                {return;}

            string s = ParseNameValues(SystemMeta, CacheEntry.SystemMetadata, 0);
            if (s != null)
            {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_metadata_name_value_parse_error, s));
            }
        }
        //
        // Returns null on success, otherwise the offending string.
        //
        private string ParseNameValues(NameValueCollection cc, StringCollection sc, int start)
        {
            WebHeaderCollection wc = cc as WebHeaderCollection;

            string lastHeaderName = null;
            if (sc != null)
            {
                for (int i = start; i < sc.Count; ++i)
                {
                    string s = sc[i];
                    if (s == null || s.Length == 0)
                    {
                        //An empty string stands for \r\n
                        //Treat that as the end of headers and ignore the rest
                        return null;
                    }

                    if (s[0] == ' ' || s[0] == '\t')
                    {
                        if (lastHeaderName == null) {return s;}
                        if (wc != null)
                            wc.AddInternal(lastHeaderName, s);
                        else
                            cc.Add(lastHeaderName, s);
                    }

                    int colpos = s.IndexOf(':');
                    if (colpos < 0)
                        {return s;}
                    lastHeaderName = s.Substring(0, colpos);
                    while (++colpos < s.Length && (s[colpos] == ' ' || s[colpos] == '\t'))
                        {;}

                    try {
                        if (wc != null)
                            wc.AddInternal(lastHeaderName, s.Substring(colpos));
                        else
                            cc.Add(lastHeaderName, s.Substring(colpos));
                    }
                    catch(Exception e) {
                        if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                            throw;
                        // Otherwise the value of 's' will be used to log an error.
                        // The fact that we cannot parse headers may stand for corrupted metadata that we try to ignore
                        return s;
                    }
                }
            }
            return null;
        }
        //
        //
        //
        private void FetchHeaderValues(bool forCache) {

            WebHeaderCollection cc = forCache? CacheHeaders: Response.Headers;


            FetchCacheControl(cc.CacheControl, forCache);

            // Parse Date Header
            string s = cc.Date;

            DateTime date = DateTime.MinValue;
            if (s != null && HttpDateParse.ParseHttpDate(s, out date)) {
                date = date.ToUniversalTime();
            }
            if (forCache) {
                CacheDate = date;
            }
            else {
                ResponseDate = date;
            }

            // Parse Expires Header
            s = cc.Expires;

            date = DateTime.MinValue;
            if (s != null && HttpDateParse.ParseHttpDate(s, out date)) {
                date = date.ToUniversalTime();
            }
            if (forCache) {
                CacheExpires = date;
            }
            else {
                ResponseExpires = date;
            }

            // Parse LastModified Header
            s = cc.LastModified;

            date = DateTime.MinValue;
            if (s != null && HttpDateParse.ParseHttpDate(s, out date)) {
                date = date.ToUniversalTime();
            }
            if (forCache) {
                CacheLastModified = date;
            }
            else {
                ResponseLastModified = date;
            }

            long totalLength = -1;
            long startRange = -1;
            long end = -1;

            HttpWebResponse resp = Response as HttpWebResponse;
            if ((forCache? CacheStatusCode: resp.StatusCode) != HttpStatusCode.PartialContent) {

                // Parse Content-Length Header
                s = cc.ContentLength;
                if (s != null && s.Length != 0) {
                    int i = 0;
                    char ch = s[0];
                    while (i < s.Length && ch == ' ') {
                        ch = s[++i];
                    }
                    if (i != s.Length && ch >= '0' && ch <= '9') {
                        totalLength = ch-'0';
                        while(++i < s.Length && (ch = s[i]) >= '0' && ch <= '9') {
                            totalLength = totalLength*10+(ch-'0');
                        }
                    }
                }
            }
            else {
                //Parse Content-Range
                s = cc[HttpKnownHeaderNames.ContentRange];
                if(s == null || !Rfc2616.Common.GetBytesRange(s, ref startRange, ref end, ref totalLength, false)) {
                    if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_content_range_error, (s==null ? "<null>" : s)));
                    startRange=end=totalLength = -1;
                }
                else if (forCache && totalLength == CacheEntry.StreamSize)
                {
                    // This is a whole response, step back to 200
                    startRange = -1;
                    end = -1;
                    CacheStatusCode = HttpStatusCode.OK;
                    CacheStatusDescription = Rfc2616.Common.OkDescription;
                }
            }

            if (forCache) {
                CacheEntityLength  = totalLength;
                ResponseRangeStart = startRange;
                ResponseRangeEnd   = end;

            }
            else {
                ResponseEntityLength = totalLength;
                ResponseRangeStart = startRange;
                ResponseRangeEnd   = end;
            }

            //Parse Age Header
            TimeSpan span = TimeSpan.MinValue;
            s = cc[HttpKnownHeaderNames.Age];
            if (s != null) {
                int i = 0;
                int sec = 0;
                while(i < s.Length && s[i++] == ' ') {
                    ;
                }
                while(i < s.Length && s[i] >= '0' && s[i] <= '9') {
                    sec = sec*10 + (s[i++] - '0');
                }
                span = TimeSpan.FromSeconds(sec);
            }

            if (forCache) {
                CacheAge = span;
            }
            else {
                ResponseAge = span;
            }
        }

        const long LO = 0x0020002000200020L;
        const int  LOI = 0x00200020;
        const long _prox = 'p'|('r'<<16)|((long)'o'<<32)|((long)'x'<<48);
        const long _y_re = 'y'|('-'<<16)|((long)'r'<<32)|((long)'e'<<48);
        const long _vali = 'v'|('a'<<16)|((long)'l'<<32)|((long)'i'<<48);
        const long _date = 'd'|('a'<<16)|((long)'t'<<32)|((long)'e'<<48);

        const long _publ = 'p'|('u'<<16)|((long)'b'<<32)|((long)'l'<<48);
        const int  _ic   = 'i'|('c'<<16);

        const long _priv = 'p'|('r'<<16)|((long)'i'<<32)|((long)'v'<<48);
        const int  _at   = 'a'|('t'<<16);

        const long _no_c = 'n'|('o'<<16)|((long)'-'<<32)|((long)'c'<<48);
        const long _ache = 'a'|('c'<<16)|((long)'h'<<32)|((long)'e'<<48);

        const long _no_s = 'n'|('o'<<16)|((long)'-'<<32)|((long)'s'<<48);
        const long _tore = 't'|('o'<<16)|((long)'r'<<32)|((long)'e'<<48);

        const long _must = 'm'|('u'<<16)|((long)'s'<<32)|((long)'t'<<48);
        const long __rev = '-'|('r'<<16)|((long)'e'<<32)|((long)'v'<<48);
        const long _alid = 'a'|('l'<<16)|((long)'i'<<32)|((long)'d'<<48);

        const long _max_ = 'm'|('a'<<16)|((long)'x'<<32)|((long)'-'<<48);
        const int  _ag   = 'a'|('g'<<16);

        const long _s_ma = 's'|('-'<<16)|((long)'m'<<32)|((long)'a'<<48);
        const long _xage = 'x'|('a'<<16)|((long)'g'<<32)|((long)'e'<<48);
        //
        //
        //
        private unsafe void FetchCacheControl(string s, bool forCache) {
            //Initialize it
            ResponseCacheControl control = new ResponseCacheControl();
            if (forCache) {
                CacheCacheControl = control;
            }
            else {
                ResponseCacheControl = control;
            }

            if (s != null && s.Length != 0) {
                fixed (char *sp = s) {
                    int len = s.Length;
                    for (int i = 0; i < len-4; ++i) {
                        if (sp[i] < ' ' || sp[i] >= 0x7F) {
                            if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_cache_control_error, s));
                            //invalid format
                            return;
                        }
                        if (sp[i] == ' ' || sp[i] == ',') {
                            continue;
                        }

                        // These if-else are two logically identical blocks that differ only in the way of how text search is done.
                        // The text search is done differently for 32 and X-bits platforms.
                        // ATTN: You are responsible for keeping the rest of the logic in [....].
                        if (IntPtr.Size == 4) {
                            // We are on 32-bits platform

                            long *mask = (long*)&(sp[i]);
                            //making interested chars lowercase, others are ignored anyway
                            switch(*mask|LO) {

                                case _prox: if (i+16 > len) continue;
                                    if ((*(mask+1)|LO) != _y_re || (*(mask+2)|LO) != _vali || (*(mask+3)|LO) != _date) continue;
                                    control.ProxyRevalidate = true;
                                    i+=15;
                                    break;

                                case _publ: if (i+6 > len) return;
                                    if ((*((int*)(mask+1))|LOI) != _ic) continue;
                                    control.Public = true;
                                    i+=5;
                                    break;

                                case _priv: if (i+7 > len) return;
                                    if ((*((int*)(mask+1))|LOI) != _at || (sp[i+6]|0x20) != 'e') continue;
                                    control.Private = true;
                                    i+=6;
                                    // Check for a case: private = "name1,name2"
                                    while (i < len && sp[i] == ' ') {++i;}

                                    if (i >= len || sp[i] != '=') {--i;break;}

                                    while (i < len && sp[++i] == ' ') {;}

                                    if (i >= len || sp[i] != '\"') {--i;break;}

                                    System.Collections.ArrayList privateList = new System.Collections.ArrayList();
                                    ++i;
                                    while(i < len && sp[i] != '\"') {

                                        while (i < len && sp[i] == ' ') {++i;}
                                        int start = i;
                                        while (i < len && sp[i] != ' ' && sp[i] != ',' && sp[i] != '\"') {++i;}
                                        if (start != i) {
                                            privateList.Add(s.Substring(start, i-start));
                                        }
                                        while (i < len && sp[i] != ',' && sp[i] != '\"') {++i;}
                                    }
                                    if (privateList.Count != 0) {
                                        control.PrivateHeaders = (string[])privateList.ToArray(typeof(string));
                                    }
                                    break;

                                case _no_c: if (i+8 > len) return;
                                    if ((*(mask+1)|LOI) != _ache) continue;
                                    control.NoCache = true;
                                    i+=7;
                                    // Check for a case: no-cache = "name1,name2"
                                    while (i < len && sp[i] == ' ') {++i;}

                                    if (i >= len || sp[i] != '=') {--i;break;}

                                    while (i < len && sp[++i] == ' ') {;}

                                    if (i >= len || sp[i] != '\"') {--i;break;}

                                    System.Collections.ArrayList nocacheList = new System.Collections.ArrayList();
                                    ++i;
                                    while(i < len && sp[i] != '\"') {

                                        while (i < len && sp[i] == ' ') {++i;}
                                        int start = i;
                                        while (i < len && sp[i] != ' ' && sp[i] != ',' && sp[i] != '\"') {++i;}
                                        if (start != i) {
                                            nocacheList.Add(s.Substring(start, i-start));
                                        }
                                        while (i < len && sp[i] != ',' && sp[i] != '\"') {++i;}
                                    }
                                    if (nocacheList.Count != 0) {
                                        control.NoCacheHeaders = (string[])nocacheList.ToArray(typeof(string));
                                    }
                                    break;

                                case _no_s: if (i+8 > len) return;
                                    if ((*(mask+1)|LOI) != _tore) continue;
                                    control.NoStore = true;
                                    i+=7;
                                    break;

                                case _must: if (i+15 > len) continue;

                                    if ((*(mask+1)|LO) != __rev || (*(mask+2)|LO) != _alid || (*(int*)(mask+3)|LOI) != _at || (sp[i+14]|0x20) != 'e') continue;
                                    control.MustRevalidate = true;
                                    i+=14;
                                    break;

                                case _max_: if (i+7 > len) return;
                                    if ((*((int*)(mask+1))|LOI) != _ag || (sp[i+6]|0x20) != 'e') continue;
                                    i+=7;
                                    while (i < len && sp[i] == ' ') {
                                        ++i;
                                    }
                                    if (i == len || sp[i++] != '=') return;
                                    while (i < len && sp[i] == ' ') {
                                        ++i;
                                    }
                                    if (i == len) return;
                                    control.MaxAge = 0;
                                    while (i < len && sp[i] >= '0' && sp[i] <= '9') {
                                        control.MaxAge =control.MaxAge*10 + (sp[i++]-'0');
                                    }
                                    --i;
                                    break;

                                case _s_ma: if (i+8 > len) return;
                                    if ((*(mask+1)|LOI) != _xage) continue;
                                    i+=8;
                                    while (i < len && sp[i] == ' ') {
                                        ++i;
                                    }
                                    if (i == len || sp[i++] != '=') return;
                                    while (i < len && sp[i] == ' ') {
                                        ++i;
                                    }
                                    if (i == len) return;
                                    control.SMaxAge = 0;
                                    while (i < len && sp[i] >= '0' && sp[i] <= '9') {
                                        control.SMaxAge = control.SMaxAge*10 + (sp[i++]-'0');
                                    }
                                    --i;
                                    break;
                            }
                        }
                        else {
                            // We cannot use optimized code path due to IA-64 memory alligment problems see VSWhidbey 118967
                            if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "proxy-revalidate")) {
                                control.ProxyRevalidate = true;
                                i+=15;
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "public")) {
                                control.Public = true;
                                i+=5;
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "private")) {
                                control.Private = true;
                                i+=6;
                                // Check for a case: private = "name1,name2"
                                while (i < len && sp[i] == ' ') {++i;}

                                if (i >= len || sp[i] != '=') {--i;break;}

                                while (i < len && sp[++i] == ' ') {;}

                                if (i >= len || sp[i] != '\"') {--i;break;}

                                System.Collections.ArrayList privateList = new System.Collections.ArrayList();
                                ++i;
                                while(i < len && sp[i] != '\"') {

                                    while (i < len && sp[i] == ' ') {++i;}
                                    int start = i;
                                    while (i < len && sp[i] != ' ' && sp[i] != ',' && sp[i] != '\"') {++i;}
                                    if (start != i) {
                                        privateList.Add(s.Substring(start, i-start));
                                    }
                                    while (i < len && sp[i] != ',' && sp[i] != '\"') {++i;}
                                }
                                if (privateList.Count != 0) {
                                    control.PrivateHeaders = (string[])privateList.ToArray(typeof(string));
                                }
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "no-cache")) {
                                control.NoCache = true;
                                i+=7;
                                // Check for a case: no-cache = "name1,name2"
                                while (i < len && sp[i] == ' ') {++i;}

                                if (i >= len || sp[i] != '=') {--i;break;}

                                while (i < len && sp[++i] == ' ') {;}

                                if (i >= len || sp[i] != '\"') {--i;break;}

                                System.Collections.ArrayList nocacheList = new System.Collections.ArrayList();
                                ++i;
                                while(i < len && sp[i] != '\"') {

                                    while (i < len && sp[i] == ' ') {++i;}
                                    int start = i;
                                    while (i < len && sp[i] != ' ' && sp[i] != ',' && sp[i] != '\"') {++i;}
                                    if (start != i) {
                                        nocacheList.Add(s.Substring(start, i-start));
                                    }
                                    while (i < len && sp[i] != ',' && sp[i] != '\"') {++i;}
                                }
                                if (nocacheList.Count != 0) {
                                    control.NoCacheHeaders = (string[])nocacheList.ToArray(typeof(string));
                                }
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "no-store")) {
                                control.NoStore = true;
                                i+=7;
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "must-revalidate")) {
                                control.MustRevalidate = true;
                                i+=14;
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "max-age")) {
                                i+=7;
                                while (i < len && sp[i] == ' ') {
                                    ++i;
                                }
                                if (i == len || sp[i++] != '=') return;
                                while (i < len && sp[i] == ' ') {
                                    ++i;
                                }
                                if (i == len) return;
                                control.MaxAge = 0;
                                while (i < len && sp[i] >= '0' && sp[i] <= '9') {
                                    control.MaxAge =control.MaxAge*10 + (sp[i++]-'0');
                                }
                                --i;
                            }
                            else if (Rfc2616.Common.UnsafeAsciiLettersNoCaseEqual(sp, i, len, "smax-age")) {
                                i+=8;
                                while (i < len && sp[i] == ' ') {
                                    ++i;
                                }
                                if (i == len || sp[i++] != '=') return;
                                while (i < len && sp[i] == ' ') {
                                    ++i;
                                }
                                if (i == len) return;
                                control.SMaxAge = 0;
                                while (i < len && sp[i] >= '0' && sp[i] <= '9') {
                                    control.SMaxAge = control.SMaxAge*10 + (sp[i++]-'0');
                                }
                                --i;
                            }
                        }
                    }
                }
            }
        }

        /*
          - any stored Warning headers with warn-code 1xx (see section
        14.46) MUST be deleted from the cache entry and the forwarded
        response.

          - any stored Warning headers with warn-code 2xx MUST be retained
        in the cache entry and the forwarded response.
        */
        private void RemoveWarnings_1xx() {

            string[] warnings = CacheHeaders.GetValues(HttpKnownHeaderNames.Warning);
            if (warnings == null) {
                return;
            }
            ArrayList remainingWarnings = new ArrayList();
            ParseHeaderValues(warnings, ParseWarningsCallback, remainingWarnings);
            CacheHeaders.Remove(HttpKnownHeaderNames.Warning);
            for (int i=0; i < remainingWarnings.Count; ++i) {
                CacheHeaders.Add(HttpKnownHeaderNames.Warning, (string)remainingWarnings[i]);
            }
        }

        private static readonly ParseCallback ParseWarningsCallback = new ParseCallback(ParseWarningsCallbackMethod);
        private static void ParseWarningsCallbackMethod(string s, int start, int end, IList list) {
            if (end >= start && s[start] != '1') {
                ParseValuesCallbackMethod(s, start, end, list);
            }
        }
        //
        // This is used by other classes to get the list if values from a header string
        //
        internal delegate void ParseCallback(string s, int start, int end, IList list);
        internal static readonly ParseCallback ParseValuesCallback = new ParseCallback(ParseValuesCallbackMethod);
        private static void ParseValuesCallbackMethod(string s, int start, int end, IList list) {

            // Deal with the cases: '' ' ' 'value' 'value   '
            while (end >= start && s[end] == ' ') {
                --end;
            }
            if (end >= start) {
                list.Add(s.Substring(start, end-start+1));
            }
        }


        //
        // Parses header values calls a callback one value after other.
        // Note a single string can contain multiple values and any value may have a quoted string in.
        // The parser will not cut trailing spaces when invoking a callback
        //
        internal static void ParseHeaderValues(string[] values, ParseCallback calback, IList list) {

            if (values == null) {
                return;
            }
            for (int i = 0; i < values.Length; ++i) {
                string val = values[i];

                int end = 0;
                int start = 0;
                while (end < val.Length) {
                    //skip spaces
                    while (start < val.Length && val[start] == ' ') {
                        ++start;
                    }

                    if (start == val.Length ) {
                        //empty header value
                        break;
                    }

                    // find comma or quote
                    end = start;
                find_comma:
                    while (end < val.Length && val[end] != ',' && val[end] != '\"') {
                        ++end;
                    }

                    if (end == val.Length ) {
                        calback(val, start, end-1, list);
                        break;
                    }

                    if (val[end] == '\"') {
                        while (++end < val.Length && val[end] != '"') {
                            ;
                        }
                        if (end == val.Length ) {
                            //warning: no closing quote, accepting
                            calback(val, start, end-1, list);
                            break;
                        }
                        goto find_comma;
                    }
                    else {
                        //Comma
                        calback(val, start, end-1, list);
                        // skip leading spaces
                        while (++end < val.Length && val[end] == ' ') {
                            ;
                        }
                        if (end >= val.Length) {
                            break;
                        }
                        start = end;
                    }
                }
            }
        }
    }
    //
    //
    //
    //ATTN: The values order is importent
    internal enum HttpMethod {
        Other   = -1,
        Head    = 0,
        Get,
        Post,
        Put,
        Delete,
        Options,
        Trace,
        Connect
    }
    //
    //
    //
    internal class ResponseCacheControl {
        internal bool Public;
        internal bool Private;
        internal string[] PrivateHeaders;
        internal bool NoCache;
        internal string[] NoCacheHeaders;
        internal bool NoStore;
        internal bool MustRevalidate;
        internal bool ProxyRevalidate;
        internal int  MaxAge;
        internal int  SMaxAge;

        internal ResponseCacheControl() {
            MaxAge = SMaxAge = -1;
        }

        internal bool IsNotEmpty {
            get {
                return (Public || Private || NoCache || NoStore || MustRevalidate || ProxyRevalidate || MaxAge != -1 || SMaxAge != -1);
            }
        }

        public override string  ToString() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (Public) {
                sb.Append(" public");
            }
            if (Private) {
                sb.Append(" private");
                if (PrivateHeaders != null) {
                    sb.Append('=');
                    for (int i = 0; i < PrivateHeaders.Length-1; ++i) {
                        sb.Append(PrivateHeaders[i]).Append(',');
                    }
                    sb.Append(PrivateHeaders[PrivateHeaders.Length-1]);
                }
            }
            if (NoCache) {
                sb.Append(" no-cache");
                if (NoCacheHeaders != null) {
                    sb.Append('=');
                    for (int i = 0; i < NoCacheHeaders.Length-1; ++i) {
                        sb.Append(NoCacheHeaders[i]).Append(',');
                    }
                    sb.Append(NoCacheHeaders[NoCacheHeaders.Length-1]);
                }
            }
            if (NoStore) {
                sb.Append(" no-store");
            }
            if (MustRevalidate) {
                sb.Append(" must-revalidate");
            }
            if (ProxyRevalidate) {
                sb.Append(" proxy-revalidate");
            }
            if (MaxAge != -1) {
                sb.Append(" max-age=").Append(MaxAge);
            }
            if (SMaxAge != -1) {
                sb.Append(" s-maxage=").Append(SMaxAge);
            }
            return sb.ToString();
        }
    }

}

