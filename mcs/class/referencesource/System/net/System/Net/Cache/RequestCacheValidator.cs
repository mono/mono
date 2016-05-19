/*++
Copyright (c) Microsoft Corporation

Module Name:

    RequestCacheValidator.cs

Abstract:

    The file specifies the contract for plugged cache validation logic.

Author:

    Alexei Vopilov    21-Dec-2002

Revision History:
    Aug 25 2003 - Moved into a separate file and implemented Whidbey M3 changes
    Jan 25 2004 - Changed the visibility of the class from public to internal.

--*/
namespace System.Net.Cache {
using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Threading;


    //
    // We need Undefined value because sometime a cache entry does not provide a clue when it should expire
    // not flags!
    internal enum CacheFreshnessStatus
    {
        Undefined   = 0,
        Fresh       = 1,
        Stale       = 2
    }

    //
    // These are valus that can be returned from validation methods.
    // Most validation methods can only return a subset of below values.
    //
    // not flags!
    internal enum CacheValidationStatus
    {
        DoNotUseCache               = 0,    //Cache is not used for this request and response is not cached.
        Fail                        = 1,    //Fail this request (allows a protocol to generate own exception)
        DoNotTakeFromCache          = 2,    //Don't used caches value for this request
        RetryResponseFromCache      = 3,    //Retry cache lookup using changed cache key
        RetryResponseFromServer     = 4,    //Retry this request as the result of invalid response received
        ReturnCachedResponse        = 5,    //Return cached response to the application
        CombineCachedAndServerResponse = 6, //Combine cached and live responses for this request
        CacheResponse               = 7,    //Replace cache entry with received live response
        UpdateResponseInformation   = 8,    //Update Metadata of cache entry using live response headers
        RemoveFromCache             = 9,    //Remove cache entry referenced to by a cache key.
        DoNotUpdateCache            = 10,   //Do nothing on cache update.
        Continue                    = 11    //Proceed to the next protocol stage.
    }

    /// <summary>
    /// <para>
    /// This class reserves a pattern for all WebRequest related cache validators.
    /// All exposed protected methods are virtual.
    /// If a derived class method does not call the base method implementation,
    /// then the base class context may not be updated so it's recommended suppressing the base
    /// methods for all subsequent calls on this class.
    /// </para>
    /// </summary>
    internal abstract class RequestCacheValidator {

        internal WebRequest              _Request;
        internal WebResponse             _Response;
        internal Stream                  _CacheStream;

        private RequestCachePolicy      _Policy;
        private Uri                     _Uri;
        private String                  _CacheKey;
        private RequestCacheEntry       _CacheEntry;
        private int                     _ResponseCount;
        private CacheValidationStatus   _ValidationStatus;
        private CacheFreshnessStatus    _CacheFreshnessStatus;
        private long                    _CacheStreamOffset;
        private long                    _CacheStreamLength;

        private bool            _StrictCacheErrors;
        private TimeSpan        _UnspecifiedMaxAge;

        /*-------------- public members -------------*/

        internal abstract RequestCacheValidator CreateValidator();

        /*
        // Consider removing.
        protected RequestCacheValidator(): this(false, TimeSpan.FromDays(1))
        {
        }
        */

        protected RequestCacheValidator(bool strictCacheErrors, TimeSpan unspecifiedMaxAge)
        {
            _StrictCacheErrors    = strictCacheErrors;
            _UnspecifiedMaxAge    = unspecifiedMaxAge;
            _ValidationStatus     = CacheValidationStatus.DoNotUseCache;
            _CacheFreshnessStatus = CacheFreshnessStatus.Undefined;
        }

        //public
        internal bool StrictCacheErrors
        {
            get {return _StrictCacheErrors;}
        }
        //
        // This would help cache validation when the entry does
        // not have any expiration mechanism defined.
        //public
        internal TimeSpan UnspecifiedMaxAge
        {
            get {return _UnspecifiedMaxAge;}
        }

        /*------------- get-only protected properties -------------*/
        protected internal Uri          Uri                             {get {return _Uri;}}
        protected internal WebRequest   Request                         {get {return _Request; }}
        protected internal WebResponse  Response                        {get {return _Response; }}
        protected internal RequestCachePolicy Policy                    {get {return _Policy; }}
        protected internal int          ResponseCount                   {get {return _ResponseCount;}}
        protected internal CacheValidationStatus ValidationStatus       {get {return _ValidationStatus;}}
        protected internal CacheFreshnessStatus  CacheFreshnessStatus   {get {return _CacheFreshnessStatus;}}
        protected internal RequestCacheEntry     CacheEntry             {get {return _CacheEntry;}}

        /*------------- protected methods and settable protected properties ------------*/
        protected internal Stream CacheStream
        {
            get {return _CacheStream;}
            set {_CacheStream = value;}
        }
        //
        protected internal long CacheStreamOffset
        {
            get {return _CacheStreamOffset;}
            set {_CacheStreamOffset = value;}
        }
        //
        protected internal long CacheStreamLength
        {
            get {return _CacheStreamLength;}
            set {_CacheStreamLength = value;}
        }
        //
        protected internal string CacheKey
        {
            get {return _CacheKey;}
            /*
            // Consider removing.
            set
            {
                // Security: Setting a cache key would allow reading an arbitrary cache location
                //new RequestCachePermission(RequestCacheActions.CacheReadWrite, value).Demand();
                _CacheKey = value;
            }
            */
        }
        //
        /*-------------- protected virtual methods -------------*/
        //
        protected internal abstract CacheValidationStatus ValidateRequest();
        //
        protected internal abstract CacheFreshnessStatus  ValidateFreshness();
        //
        protected internal abstract CacheValidationStatus ValidateCache();
        //
        protected internal abstract CacheValidationStatus ValidateResponse();
        //
        protected internal abstract CacheValidationStatus RevalidateCache();
        //
        protected internal abstract CacheValidationStatus UpdateCache();
        //
        protected internal virtual void FailRequest(WebExceptionStatus webStatus)
        {
            if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_failing_request_with_exception, webStatus.ToString()));
            if (webStatus == WebExceptionStatus.CacheEntryNotFound)
                throw ExceptionHelper.CacheEntryNotFoundException;
            else if (webStatus == WebExceptionStatus.RequestProhibitedByCachePolicy)
                throw ExceptionHelper.RequestProhibitedByCachePolicyException;

            throw new WebException(NetRes.GetWebStatusString("net_requestaborted", webStatus), webStatus);
        }

        /*-------------- internal members -------------*/
        //
        internal void FetchRequest(Uri uri, WebRequest request)
        {
            _Request = request;
            _Policy  = request.CachePolicy;
            _Response = null;
            _ResponseCount = 0;
            _ValidationStatus     = CacheValidationStatus.DoNotUseCache;
            _CacheFreshnessStatus = CacheFreshnessStatus.Undefined;
            _CacheStream          = null;
            _CacheStreamOffset    = 0L;
            _CacheStreamLength    = 0L;

            if (!uri.Equals(_Uri))
            {
                // it's changed from previous call
                _CacheKey = uri.GetParts(UriComponents.AbsoluteUri, UriFormat.Unescaped);
            }
            _Uri = uri;
        }
        //
        internal void FetchCacheEntry(RequestCacheEntry fetchEntry)
        {
            _CacheEntry = fetchEntry;
        }

        internal void FetchResponse(WebResponse fetchResponse)
        {
            ++_ResponseCount;
            _Response = fetchResponse;
        }

        internal void SetFreshnessStatus(CacheFreshnessStatus status)
        {
            _CacheFreshnessStatus = status;
        }

        internal void SetValidationStatus(CacheValidationStatus status)
        {
            _ValidationStatus = status;
        }
    }

}
