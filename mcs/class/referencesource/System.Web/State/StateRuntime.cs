//------------------------------------------------------------------------------
// <copyright file="StateRuntime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * StateWebRuntime
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.SessionState {
    using System.Configuration;
    using System.Globalization;
    using System.IO;    
    using System.Runtime.InteropServices;   
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Util;


    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    [ComImport, Guid("7297744b-e188-40bf-b7e9-56698d25cf44"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStateRuntime {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void StopProcessing();

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void ProcessRequest(
               [In, MarshalAs(UnmanagedType.SysInt)]
               IntPtr tracker,
               [In, MarshalAs(UnmanagedType.I4)]
               int verb,
               [In, MarshalAs(UnmanagedType.LPWStr)]
               string uri,
               [In, MarshalAs(UnmanagedType.I4)]
               int exclusive,
               [In, MarshalAs(UnmanagedType.I4)]
               int timeout,
               [In, MarshalAs(UnmanagedType.I4)]
               int lockCookieExists,
               [In, MarshalAs(UnmanagedType.I4)]
               int lockCookie,
               [In, MarshalAs(UnmanagedType.I4)]
               int contentLength,
               [In, MarshalAs(UnmanagedType.SysInt)]
               IntPtr content);

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void ProcessRequest(
               [In, MarshalAs(UnmanagedType.SysInt)]
               IntPtr tracker,
               [In, MarshalAs(UnmanagedType.I4)]
               int verb,
               [In, MarshalAs(UnmanagedType.LPWStr)]
               string uri,
               [In, MarshalAs(UnmanagedType.I4)]
               int exclusive,
               [In, MarshalAs(UnmanagedType.I4)]
               int extraFlags,
               [In, MarshalAs(UnmanagedType.I4)]
               int timeout,
               [In, MarshalAs(UnmanagedType.I4)]
               int lockCookieExists,
               [In, MarshalAs(UnmanagedType.I4)]
               int lockCookie,
               [In, MarshalAs(UnmanagedType.I4)]
               int contentLength,
               [In, MarshalAs(UnmanagedType.SysInt)]
               IntPtr content);

    }


    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
    public sealed class StateRuntime : IStateRuntime {
        static StateRuntime() {
            WebConfigurationFileMap webFileMap = new WebConfigurationFileMap();
            UserMapPath mapPath = new UserMapPath(webFileMap);
            HttpConfigurationSystem.EnsureInit(mapPath, false, true);

            StateApplication app = new StateApplication();

            HttpApplicationFactory.SetCustomApplication(app);

            PerfCounters.OpenStateCounters();
            ResetStateServerCounters();
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.State.StateRuntime'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        public StateRuntime() {
        }

        /*
         * Shutdown runtime
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void StopProcessing() {
            ResetStateServerCounters();
            HttpRuntime.Close();
        }

        static void ResetStateServerCounters() {
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL, 0);
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE, 0);
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT, 0);
            PerfCounters.SetStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED, 0);
        }

        public void ProcessRequest(
                  IntPtr tracker,
                  int verb,
                  string uri,
                  int exclusive,
                  int timeout,
                  int lockCookieExists,
                  int lockCookie,
                  int contentLength,
                  IntPtr content
                  ) {
            ProcessRequest(
                  tracker,
                  verb,
                  uri,
                  exclusive,
                  0,
                  timeout,
                  lockCookieExists,
                  lockCookie,
                  contentLength,
                  content);
        }

        /*
         * Process one ISAPI request
         *
         * @param ecb ECB
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void ProcessRequest(
                  IntPtr tracker,
                  int verb,
                  string uri,
                  int exclusive,
                  int extraFlags,
                  int timeout,
                  int lockCookieExists,
                  int lockCookie,
                  int contentLength,
                  IntPtr content
                  ) {

            StateHttpWorkerRequest  wr;

            wr = new StateHttpWorkerRequest(
                       tracker, (UnsafeNativeMethods.StateProtocolVerb) verb, uri, 
                       (UnsafeNativeMethods.StateProtocolExclusive) exclusive, extraFlags, timeout, 
                       lockCookieExists, lockCookie, contentLength, content);

            HttpRuntime.ProcessRequest(wr);
        }
    }

    internal static class StateHeaders {
        internal const String EXCLUSIVE_NAME = "Http_Exclusive";
        internal const String EXCLUSIVE_VALUE_ACQUIRE = "acquire";
        internal const String EXCLUSIVE_VALUE_RELEASE = "release";
        internal const String TIMEOUT_NAME = "Http_Timeout";
        internal const String TIMEOUT_NAME_RAW = "Timeout";
        internal const String LOCKCOOKIE_NAME = "Http_LockCookie";
        internal const String LOCKCOOKIE_NAME_RAW = "LockCookie";
        internal const String LOCKDATE_NAME = "Http_LockDate";
        internal const String LOCKDATE_NAME_RAW = "LockDate";
        internal const String LOCKAGE_NAME = "Http_LockAge";
        internal const String LOCKAGE_NAME_RAW = "LockAge";
        internal const String EXTRAFLAGS_NAME = "Http_ExtraFlags";
        internal const String EXTRAFLAGS_NAME_RAW = "ExtraFlags";
        internal const String ACTIONFLAGS_NAME = "Http_ActionFlags";
        internal const String ACTIONFLAGS_NAME_RAW = "ActionFlags";
    };

    internal sealed class CachedContent {
        internal byte[]             _content;
        internal IntPtr             _stateItem; // The pointer to the native memory that points to the psi
        internal bool               _locked;
        internal DateTime           _utcLockDate;
        internal int                _lockCookie;
        internal int                _extraFlags;
        #pragma warning disable 0649
        internal ReadWriteSpinLock  _spinLock;
        #pragma warning restore 0649

        internal CachedContent(
                byte []     content, 
                IntPtr      stateItem,
                bool        locked,
                DateTime    utcLockDate,
                int         lockCookie,
                int         extraFlags) {

            _content = content;
            _stateItem = stateItem;
            _locked = locked;
            _utcLockDate = utcLockDate;
            _lockCookie = lockCookie;
            _extraFlags = extraFlags;
        }
    }

    internal class StateApplication : IHttpHandler {
        CacheItemRemovedCallback _removedHandler;

        internal StateApplication() {
            if (!HttpRuntime.IsFullTrust) {
                // DevDiv #89021: This type passes user-supplied data to unmanaged code, so we need
                // to ensure that it can only be used from within a FullTrust environment.
                throw new InvalidOperationException(SR.GetString(SR.StateApplication_FullTrustOnly));
            }

            _removedHandler = new CacheItemRemovedCallback(this.OnCacheItemRemoved);
        }

        public void ProcessRequest(HttpContext context) {
            // Don't send content-type header.
            context.Response.ContentType = null;

            switch (context.Request.HttpVerb) {
                case HttpVerb.GET:
                    DoGet(context);
                    break;

                case HttpVerb.PUT:
                    DoPut(context);
                    break;

                case HttpVerb.HEAD:
                    DoHead(context);
                    break;

                case HttpVerb.DELETE:
                    DoDelete(context);
                    break;

                default:
                    DoUnknown(context);
                    break;
            }
        }

        public bool IsReusable {
            get { return true; }
        }

        private string CreateKey(HttpRequest request) {
            return CacheInternal.PrefixStateApplication + HttpUtility.UrlDecode(request.RawUrl);
        }

        private void ReportInvalidHeader(HttpContext context, String header) {
            HttpResponse    response;

            response = context.Response;
            response.StatusCode = 400;
            response.Write("<html><head><title>Bad Request</title></head>\r\n");
            response.Write("<body><h1>Http/1.1 400 Bad Request</h1>");
            response.Write("Invalid header <b>" + header + "</b></body></html>");
        }

        private void ReportLocked(HttpContext context, CachedContent content) {
            HttpResponse    response;
            DateTime        localLockDate;
            long            lockAge;

            // Note that due to a bug in the RTM state server client, 
            // we cannot add to body of the response when sending this
            // message, otherwise the client will leak memory.
            response = context.Response;
            response.StatusCode = 423;
            localLockDate = DateTimeUtil.ConvertToLocalTime(content._utcLockDate);
            lockAge = (DateTime.UtcNow - content._utcLockDate).Ticks / TimeSpan.TicksPerSecond;
            response.AppendHeader(StateHeaders.LOCKDATE_NAME_RAW, localLockDate.Ticks.ToString(CultureInfo.InvariantCulture));
            response.AppendHeader(StateHeaders.LOCKAGE_NAME_RAW, lockAge.ToString(CultureInfo.InvariantCulture));
            response.AppendHeader(StateHeaders.LOCKCOOKIE_NAME_RAW, content._lockCookie.ToString(CultureInfo.InvariantCulture));
        }

        private void ReportActionFlags(HttpContext context, int flags) {
            HttpResponse    response;

            // Note that due to a bug in the RTM state server client, 
            // we cannot add to body of the response when sending this
            // message, otherwise the client will leak memory.
            response = context.Response;
            response.AppendHeader(StateHeaders.ACTIONFLAGS_NAME_RAW, flags.ToString(CultureInfo.InvariantCulture));
        }

        private void ReportNotFound(HttpContext context) {
            context.Response.StatusCode = 404;
        }

        bool GetOptionalNonNegativeInt32HeaderValue(HttpContext context, string header, out int value)
        {
            bool headerValid;
            string valueAsString;

            value = -1;
            valueAsString = context.Request.Headers[header];
            if (valueAsString == null) {
                headerValid = true;
            }
            else {
                headerValid = false;
                try {
                    value = Int32.Parse(valueAsString, CultureInfo.InvariantCulture);
                    if (value >= 0) {
                        headerValid = true;
                    }
                }
                catch {
                }
            }

            if (!headerValid) {
                ReportInvalidHeader(context, header);
            }

            return headerValid;
        }

        bool GetRequiredNonNegativeInt32HeaderValue(HttpContext context, string header, out int value)
        {
            bool headerValid = GetOptionalNonNegativeInt32HeaderValue(context, header, out value);
            if (headerValid && value == -1) {
                headerValid = false;
                ReportInvalidHeader(context, header);
            }

            return headerValid;
        }

        bool GetOptionalInt32HeaderValue(HttpContext context, string header, out int value, out bool found)
        {
            bool headerValid;
            string valueAsString;

            found = false;

            value = 0;
            valueAsString = context.Request.Headers[header];
            if (valueAsString == null) {
                headerValid = true;
            }
            else {
                headerValid = false;
                try {
                    value = Int32.Parse(valueAsString, CultureInfo.InvariantCulture);
                    headerValid = true;
                    found = true;
                }
                catch {
                }
            }

            if (!headerValid) {
                ReportInvalidHeader(context, header);
            }

            return headerValid;
        }

        /*
         * Check Exclusive header for get, getexlusive, releaseexclusive
         * use the path as the id
         * Create the cache key
         * follow inproc.
         */
        internal /*public*/ void DoGet(HttpContext context) {
            HttpRequest     request = context.Request;
            HttpResponse    response = context.Response;
            Stream          responseStream;
            byte[]          buf;
            string          exclusiveAccess;
            string          key;
            CachedContent   content;
            CacheEntry      entry;
            int             lockCookie;
            int             timeout;

            key = CreateKey(request);
            entry = (CacheEntry) HttpRuntime.CacheInternal.Get(key, CacheGetOptions.ReturnCacheEntry);
            if (entry == null) {
                ReportNotFound(context);
                return;
            }

            exclusiveAccess = request.Headers[StateHeaders.EXCLUSIVE_NAME];
            content = (CachedContent) entry.Value;
            content._spinLock.AcquireWriterLock();
            try {
                if (content._content == null) {
                    ReportNotFound(context);
                    return;
                }
                
                int initialFlags;

                initialFlags = content._extraFlags;
                if ((initialFlags & (int)SessionStateItemFlags.Uninitialized) != 0) {
                    // It is an uninitialized item.  We have to remove that flag.
                    // We only allow one request to do that.
                    // For details, see inline doc for SessionStateItemFlags.Uninitialized flag.

                    // If initialFlags != return value of CompareExchange, it means another request has
                    // removed the flag.

                    if (initialFlags == Interlocked.CompareExchange(
                                            ref content._extraFlags, 
                                            initialFlags & (~((int)SessionStateItemFlags.Uninitialized)), 
                                            initialFlags)) {
                        ReportActionFlags(context, (int)SessionStateActions.InitializeItem);
                    }
                }

                if (exclusiveAccess == StateHeaders.EXCLUSIVE_VALUE_RELEASE) {
                    if (!GetRequiredNonNegativeInt32HeaderValue(context, StateHeaders.LOCKCOOKIE_NAME, out lockCookie))
                        return;
                     
                    if (content._locked) {
                        if (lockCookie == content._lockCookie) {
                            content._locked = false;
                        }
                        else {
                            ReportLocked(context, content);
                        }
                    }
                    else {
                        // should be locked but isn't.
                        context.Response.StatusCode = 200;
                    }
                } 
                else {
                    if (content._locked) {
                        ReportLocked(context, content);
                        return;
                    }

                    if (exclusiveAccess == StateHeaders.EXCLUSIVE_VALUE_ACQUIRE) {
                        content._locked = true;
                        content._utcLockDate = DateTime.UtcNow;
                        content._lockCookie++;

                        response.AppendHeader(StateHeaders.LOCKCOOKIE_NAME_RAW, (content._lockCookie).ToString(CultureInfo.InvariantCulture));
                    }

                    timeout = (int) (entry.SlidingExpiration.Ticks / TimeSpan.TicksPerMinute);
                    response.AppendHeader(StateHeaders.TIMEOUT_NAME_RAW, (timeout).ToString(CultureInfo.InvariantCulture));
                    responseStream = response.OutputStream;
                    buf = content._content;
                    responseStream.Write(buf, 0, buf.Length);
                    response.Flush();
                }
            }
            finally {
                content._spinLock.ReleaseWriterLock();
            }
        }


        internal /*public*/ void DoPut(HttpContext context) {
            IntPtr  stateItemDelete;

            stateItemDelete = FinishPut(context);
            if (stateItemDelete != IntPtr.Zero) {
                UnsafeNativeMethods.STWNDDeleteStateItem(stateItemDelete);
            }
        }

        unsafe IntPtr FinishPut(HttpContext context) {
            HttpRequest         request = context.Request;   
            HttpResponse        response = context.Response; 
            Stream              requestStream;               
            byte[]              buf;                         
            int                 timeoutMinutes;
            TimeSpan            timeout;
            int                 extraFlags;
            string              key;                         
            CachedContent       content;
            CachedContent       contentCurrent;
            int                 lockCookie;
            int                 lockCookieNew = 1;
            IntPtr              stateItem;
            CacheInternal       cacheInternal = HttpRuntime.CacheInternal;

            /* create the content */
            requestStream = request.InputStream;
            int bufferSize = (int)(requestStream.Length - requestStream.Position);
            buf = new byte[bufferSize];
            requestStream.Read(buf, 0 , buf.Length);

            fixed (byte * pBuf = buf) {
                // The ctor of StateHttpWorkerRequest convert the native pointer address
                // into an array of bytes, and in our we revert it back to an IntPtr
                stateItem = (IntPtr)(*((void **)pBuf));
            }

            /* get headers */
            if (!GetOptionalNonNegativeInt32HeaderValue(context, StateHeaders.TIMEOUT_NAME, out timeoutMinutes)) {
                return stateItem;
            }

            if (timeoutMinutes == -1) {
                timeoutMinutes = SessionStateModule.TIMEOUT_DEFAULT;
            }

            if (timeoutMinutes > SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES) {
                ReportInvalidHeader(context, StateHeaders.TIMEOUT_NAME);
                return stateItem;
            }

            timeout = new TimeSpan(0, timeoutMinutes, 0);

            bool found;
            if (!GetOptionalInt32HeaderValue(context, StateHeaders.EXTRAFLAGS_NAME, out extraFlags, out found)) {
                return stateItem;
            }

            if (!found) {
                extraFlags = 0;
            }

            /* lookup current value */
            key = CreateKey(request);
            CacheEntry entry = (CacheEntry) cacheInternal.Get(key, CacheGetOptions.ReturnCacheEntry);
            if (entry != null) {
                // DevDivBugs 146875: Expired Session State race condition
                // We make sure we do not overwrite an already existing item with an uninitialized item.
                if (((int)SessionStateItemFlags.Uninitialized & extraFlags) == 1) {
                    return stateItem;
                }
                
                if (!GetOptionalNonNegativeInt32HeaderValue(context, StateHeaders.LOCKCOOKIE_NAME, out lockCookie)) {
                    return stateItem;
                }

                contentCurrent = (CachedContent) entry.Value;
                contentCurrent._spinLock.AcquireWriterLock();
                try {
                    if (contentCurrent._content == null) {
                        ReportNotFound(context);
                        return stateItem;
                    }

                    /* Only set the item if we are the owner */
                    if (contentCurrent._locked && (lockCookie == -1 || lockCookie != contentCurrent._lockCookie)) {
                        ReportLocked(context, contentCurrent);
                        return stateItem;
                    }

                    if (entry.SlidingExpiration == timeout && contentCurrent._content != null) {
                        /* delete the old state item */
                        IntPtr stateItemOld = contentCurrent._stateItem;

                        /* change the item in place */
                        contentCurrent._content = buf;
                        contentCurrent._stateItem = stateItem;
                        contentCurrent._locked = false;
                        return stateItemOld;
                    }

                    /*
                        The timeout has changed.  In this case, we are removing the old item and
                        inserting a new one.
                        Update _extraFlags to ignore the cache item removed callback (this way,
                        we will not decrease the number of active sessions).
                     */
                    contentCurrent._extraFlags |= (int)SessionStateItemFlags.IgnoreCacheItemRemoved;

                    /*
                     * If not locked, keep it locked until it is completely replaced.
                     * Prevent overwriting when we drop the lock.
                     */
                    contentCurrent._locked = true;
                    contentCurrent._lockCookie = 0;
                    lockCookieNew = lockCookie;
                }
                finally {
                    contentCurrent._spinLock.ReleaseWriterLock();
                }
            }

            content = new CachedContent(buf, stateItem, false, DateTime.MinValue, lockCookieNew, extraFlags);
            cacheInternal.UtcInsert(
                    key, content, null, Cache.NoAbsoluteExpiration, timeout,
                    CacheItemPriority.NotRemovable, _removedHandler);

            if (entry == null) {
                IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL);
                IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE);
            }

            return IntPtr.Zero;
        }

        internal /*public*/ void DoDelete(HttpContext context) {
            string          key = CreateKey(context.Request);
            CacheInternal   cacheInternal = HttpRuntime.CacheInternal;
            CachedContent   content = (CachedContent) cacheInternal.Get(key);

            /* If the item isn't there, we probably took too long to run. */
            if (content == null) {
                ReportNotFound(context);
                return;
            }

            int lockCookie;
            if (!GetOptionalNonNegativeInt32HeaderValue(context, StateHeaders.LOCKCOOKIE_NAME, out lockCookie))
                return;

            content._spinLock.AcquireWriterLock();
            try {
                if (content._content == null) {
                    ReportNotFound(context);
                    return;
                }

                /* Only remove the item if we are the owner */
                if (content._locked && (lockCookie == -1 || content._lockCookie != lockCookie)) {
                    ReportLocked(context, content);
                    return;
                }

                /*
                 * If not locked, keep it locked until it is completely removed.
                 * Prevent overwriting when we drop the lock.
                 */
                content._locked = true;
                content._lockCookie = 0;
            }
            finally {
                content._spinLock.ReleaseWriterLock();
            }


            cacheInternal.Remove(key);
        }

        internal /*public*/ void DoHead(HttpContext context) {
            string  key;
            Object  item;

            key = CreateKey(context.Request);
            item = HttpRuntime.CacheInternal.Get(key);
            if (item == null) {
                ReportNotFound(context);
            }
        }

        /*
         * Unknown Http verb. Responds with "400 Bad Request".
         * Override this method to report different Http code.
         */
        internal /*public*/ void DoUnknown(HttpContext context) {
            context.Response.StatusCode = 400;
        }

        unsafe void OnCacheItemRemoved(String key, Object value, CacheItemRemovedReason reason) {
            CachedContent   content;
            IntPtr          stateItem;

            content = (CachedContent) value;

            content._spinLock.AcquireWriterLock();
            try {
                stateItem = content._stateItem;
                content._content = null;
                content._stateItem = IntPtr.Zero;
            }
            finally {
                content._spinLock.ReleaseWriterLock();
            }
           
            UnsafeNativeMethods.STWNDDeleteStateItem(stateItem);

            /* If _extraFlags have IgnoreCacheItemRemoved specified,
                don't update the counters.
             */
            if ((content._extraFlags & (int)SessionStateItemFlags.IgnoreCacheItemRemoved) != 0) {
                Debug.Trace("OnCacheItemRemoved", "OnCacheItemRemoved ignored (item removed, but counters not updated)");
                return;
            }

            switch (reason) {
                case CacheItemRemovedReason.Expired: 
                    IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT);
                    break;

                case CacheItemRemovedReason.Removed:
                    IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED);
                    break;

                default:
                    break;    
            }

            DecrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE);
        }

        private void DecrementStateServiceCounter(StateServicePerfCounter counter) {
            if (HttpRuntime.ShutdownInProgress) {
                return;
            }

            PerfCounters.DecrementStateServiceCounter(counter);
        }

        private void IncrementStateServiceCounter(StateServicePerfCounter counter) {
            if (HttpRuntime.ShutdownInProgress) {
                return;
            }

            PerfCounters.IncrementStateServiceCounter(counter);
        }

    }
}
