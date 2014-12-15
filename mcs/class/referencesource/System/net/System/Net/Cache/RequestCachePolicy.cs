/*++
Copyright (c) Microsoft Corporation

Module Name:

    RequestCachePolicy.cs

Abstract:
    The class implements caching policy paradigms that is used by all webrequest cache-aware clients

Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

    4  Dec 2003  - Reworked as per design review.

    30 Jul 2004  - Updated to accomodate FTP caching feature

--*/
namespace System.Net.Cache {
    using System.Globalization;


    // The enum describes cache settings applicable for any webrequest
    public enum RequestCacheLevel
    {
        // Default cache behavior determined by the protocol class
        Default             = 0,
        // Bypass the cache completely
        BypassCache         = 1,
        // Only serve requests from cache, an exception is thrown if not found
        CacheOnly           = 2,
        // Serve from the cache, but will [....] up with the server if not found
        CacheIfAvailable    = 3,
        // Attempt to revalidate cache with the server, reload if unable to
        Revalidate          = 4,
        // Reload the data from the origin server
        Reload              = 5,
        // Bypass the cache and removing existing entries in the cache
        NoCacheNoStore      = 6
    }


    //
    // Common request cache policy used for caching of Http, FTP, etc.
    //
    public class RequestCachePolicy
    {
        private RequestCacheLevel m_Level;

        //
        // Public stuff
        //
        public RequestCachePolicy(): this (RequestCacheLevel.Default)
        {
        }

        public RequestCachePolicy(RequestCacheLevel level)
        {
            if (level < RequestCacheLevel.Default || level > RequestCacheLevel.NoCacheNoStore)
                throw new ArgumentOutOfRangeException("level");

            m_Level = level;
        }
        //
        public RequestCacheLevel Level
        {
            get {
                return m_Level;
            }
        }
        //
        public override string ToString()
        {
            return "Level:" + m_Level.ToString();
        }
        //
        // Internal stuff
        //
        //
#if TRAVE
        /*
        // Consider removing.
        internal static string ToString(RequestCachePolicy Policy)
        {
            if (Policy == null) {
                return "null";
            }
            return Policy.ToString();
        }
        */
#endif

    }

    // The enum describes cache settings for http
    public enum HttpRequestCacheLevel
    {
        // Default cache behavior, server fresh response fomr cache, otherwise attempt
        // to revalidate with the server or reload
        Default             = 0,
        // Bypass the cache completely
        BypassCache         = 1,
        // Only serve requests from cache, an exception is thrown if not found
        CacheOnly           = 2,
        // Serve from the cache, but will [....] up with the server if not found
        CacheIfAvailable    = 3,
        // Validate cached data with the server even if it looks fresh
        Revalidate          = 4,
        // Reload the data from the origin server
        Reload              = 5,
        // Bypass the cache and removing existing entries in the cache
        NoCacheNoStore      = 6,
        // Serve from cache, or the next cache along the path
        CacheOrNextCacheOnly= 7,
        // Reload the data either from the origin server or from an uplevel cache
        //This is equvalent to Cache-Control:MaxAge=0 HTTP semantic
        Refresh             = 8,
    }
    //
    // CacheAgeControl is used to specify preferences with respect of cached item age and freshness.
    //
    public enum HttpCacheAgeControl {
            // Invalid value. Indicates the enum is not initialized
            None               = 0x0,
            // Cached item must be at least fresh for specified period since now
            MinFresh            = 0x1,
            // Cached item must be fresh and it's age must not exceed specified period
            MaxAge              = 0x2,
            // Cached item may be not fresh but it's expiration must not exceed specified period
            MaxStale            = 0x4,
            // Cached item must fresh for some period in future and it's age must be less than specified
            MaxAgeAndMinFresh   = 0x3, // MaxAge|MinFresh,
            // Cached item may be found as stale for some period but it's age must be less than specified
            MaxAgeAndMaxStale   = 0x6, // MaxAge|MaxStale,
    }

    //
    // HTTP cache policy that expresses RFC2616 HTTP caching semantic
    //
    public class HttpRequestCachePolicy: RequestCachePolicy {

        internal static readonly HttpRequestCachePolicy BypassCache = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);

        //Private members
        private HttpRequestCacheLevel m_Level = HttpRequestCacheLevel.Default;
        private DateTime        m_LastSyncDateUtc   = DateTime.MinValue;
        private TimeSpan        m_MaxAge     = TimeSpan.MaxValue;
        private TimeSpan        m_MinFresh   = TimeSpan.MinValue;
        private TimeSpan        m_MaxStale   = TimeSpan.MinValue;

        //
        // Public stuff
        //
        public HttpRequestCachePolicy():this(HttpRequestCacheLevel.Default)
        {
        }
        //
        public HttpRequestCachePolicy(HttpRequestCacheLevel level): base(MapLevel(level))
        {
            m_Level = level;
        }
        //
        // Creates an automatic cache policy that is bound to a simples age control
        //
        public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan ageOrFreshOrStale):this(HttpRequestCacheLevel.Default)
        {

            switch(cacheAgeControl) {
            case HttpCacheAgeControl.MinFresh:
                m_MinFresh = ageOrFreshOrStale;
                break;
            case HttpCacheAgeControl.MaxAge:
                m_MaxAge = ageOrFreshOrStale;
                break;
            case HttpCacheAgeControl.MaxStale:
                m_MaxStale = ageOrFreshOrStale;
                break;
            default:
                throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "HttpCacheAgeControl"), "cacheAgeControl");
            }
        }
        //
        // Creates an automatic cache policy that is bound to a complex age control
        //
        public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale):this(HttpRequestCacheLevel.Default)
        {

             switch(cacheAgeControl) {
             case HttpCacheAgeControl.MinFresh:
                 m_MinFresh = freshOrStale;
                 break;
             case HttpCacheAgeControl.MaxAge:
                 m_MaxAge = maxAge;
                 break;
             case HttpCacheAgeControl.MaxStale:
                 m_MaxStale = freshOrStale;
                 break;
             case HttpCacheAgeControl.MaxAgeAndMinFresh:
                 m_MaxAge = maxAge;
                 m_MinFresh = freshOrStale;
                 break;
             case HttpCacheAgeControl.MaxAgeAndMaxStale:
                 m_MaxAge = maxAge;
                 m_MaxStale = freshOrStale;
                 break;
             default:
                 throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "HttpCacheAgeControl"), "cacheAgeControl");
             }
        }
        //
        // Creates an automatic cache policy with the Date Synchronization requirement
        //
        public HttpRequestCachePolicy(DateTime cacheSyncDate):this(HttpRequestCacheLevel.Default)
        {
            m_LastSyncDateUtc = cacheSyncDate.ToUniversalTime();
        }
        //
        //
        //
        public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale, DateTime cacheSyncDate)
            :this(cacheAgeControl, maxAge, freshOrStale)
        {
            m_LastSyncDateUtc = cacheSyncDate.ToUniversalTime();
        }
        //
        // Properties
        //
        public new HttpRequestCacheLevel Level
        {
            get {
                return m_Level;
            }
        }
        //
        // Requires revalidation of items stored before lastSyncDate
        //
        public DateTime  CacheSyncDate {
            get {
                if (m_LastSyncDateUtc == DateTime.MinValue || m_LastSyncDateUtc == DateTime.MaxValue) {
                    return m_LastSyncDateUtc;
                }
                return m_LastSyncDateUtc.ToLocalTime();}
        }
        //
        internal DateTime  InternalCacheSyncDateUtc {
            get {return m_LastSyncDateUtc;}
        }
        //
        // Specifies age policy according to HTTP 1.1 RFC caching semantic
        //
        public TimeSpan MaxAge {
            get {return m_MaxAge;}
        }
        //
        // Specifies age policy according to HTTP 1.1 RFC caching semantic
        //
        public TimeSpan MinFresh {
            get {return m_MinFresh;}
        }
        //
        // Specifies age policy according to HTTP 1.1 RFC caching semantic
        //
        public TimeSpan MaxStale {
            get {return m_MaxStale;}
        }
        //
        //
        //
        public override string ToString()
        {
            return "Level:" + m_Level.ToString() +
                (m_MaxAge == TimeSpan.MaxValue? string.Empty: " MaxAge:" + m_MaxAge.ToString()) +
                (m_MinFresh == TimeSpan.MinValue? string.Empty: " MinFresh:" + m_MinFresh.ToString()) +
                (m_MaxStale == TimeSpan.MinValue? string.Empty: " MaxStale:" + m_MaxStale.ToString()) +
                (CacheSyncDate==DateTime.MinValue? string.Empty: " CacheSyncDate:" +  CacheSyncDate.ToString(CultureInfo.CurrentCulture));
        }
        //
        //
        //
        private static RequestCacheLevel MapLevel(HttpRequestCacheLevel level)
        {

            if (level <= HttpRequestCacheLevel.NoCacheNoStore)
                return (RequestCacheLevel) level;

            if (level == HttpRequestCacheLevel.CacheOrNextCacheOnly)
                return RequestCacheLevel.CacheOnly;

            if (level == HttpRequestCacheLevel.Refresh)
                return RequestCacheLevel.Reload;

            throw new ArgumentOutOfRangeException("level");
        }
    }
}
