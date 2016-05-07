/*++
Copyright (c) Microsoft Corporation

Module Name:

    _RequestCacheProtocol.cs

Abstract:


    The class is a cache protocol engine.
    An application protocol such as HttpWebRequest or FtpWebRequest
    gets all cache-related answers by talking to this class

    Sometime in the future it will become public.

Author:

    Alexei Vopilov    21-Dec-2002

Revision History:
    Aug 25 2003 - moved into separate file and revised as per Whidbey-M3 spec.

--*/
namespace System.Net.Cache {
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using System.Collections.Specialized;
    using System.Threading;
    using System.Globalization;

    //
    //
    //
    internal class RequestCacheProtocol {

        private CacheValidationStatus _ProtocolStatus;
        private Exception            _ProtocolException;
        private Stream               _ResponseStream;
        private long                 _ResponseStreamLength;
        private RequestCacheValidator _Validator;
        private RequestCache         _RequestCache;
        private bool                 _IsCacheFresh;
        private bool                 _CanTakeNewRequest;

//      private string[]             _ResponseMetadata;
//      private string               _CacheRetrieveKey;
//      private string               _CacheStoreKey;

        //
        // Public properties
        //
        internal CacheValidationStatus    ProtocolStatus          {get {return _ProtocolStatus;}}
        internal Exception                ProtocolException       {get {return _ProtocolException;}}
        internal Stream                   ResponseStream          {get {return _ResponseStream;}}
        internal long                     ResponseStreamLength     {get {return _ResponseStreamLength;}}
        internal RequestCacheValidator    Validator               {get {return _Validator;}}
        internal bool                     IsCacheFresh            {get {return _Validator != null && _Validator.CacheFreshnessStatus == CacheFreshnessStatus.Fresh;}}

//      internal string[]                 ResponseMetadata        {get {return _ResponseMetadata;}}
//      internal string                   CacheRetrieveKey        {get {return _CacheRetrieveKey;}}
//      internal string                   CacheStoreKey           {get {return _CacheStoreKey;}}

        //
        // Public methods
        //
        internal RequestCacheProtocol(RequestCache cache, RequestCacheValidator defaultValidator)
        {
            _RequestCache   = cache;
            _Validator      = defaultValidator;
            _CanTakeNewRequest = true;
        }
        //
        internal CacheValidationStatus  GetRetrieveStatus (Uri cacheUri, WebRequest request)
        {

            if (cacheUri == null)
                throw new ArgumentNullException("cacheUri");

            if (request == null)
               throw new ArgumentNullException("request");

            if (!_CanTakeNewRequest || _ProtocolStatus == CacheValidationStatus.RetryResponseFromServer)
                return CacheValidationStatus.Continue;
            _CanTakeNewRequest = false;


            // Reset protocol state
            _ResponseStream       = null;
            _ResponseStreamLength = 0L;
            _ProtocolStatus       = CacheValidationStatus.Continue;
            _ProtocolException    = null;

            if(Logging.On) Logging.Enter(Logging.RequestCache, this, "GetRetrieveStatus", request);
            try {
                if (request.CachePolicy == null || request.CachePolicy.Level == RequestCacheLevel.BypassCache)
                {
                    _ProtocolStatus = CacheValidationStatus.DoNotUseCache;
                    return _ProtocolStatus;
                }

                if (_RequestCache == null || _Validator == null)
                {
                    _ProtocolStatus = CacheValidationStatus.DoNotUseCache;
                    return _ProtocolStatus;
                }

                _Validator.FetchRequest(cacheUri, request);

                switch(_ProtocolStatus = ValidateRequest())
                {
                case CacheValidationStatus.Continue:            // This is a green light for cache protocol
                    break;

                case CacheValidationStatus.DoNotTakeFromCache:  // no cache but response can be cached
                case CacheValidationStatus.DoNotUseCache:       // ignore cache entirely
                    break;

                case CacheValidationStatus.Fail:
                    _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_fail, "ValidateRequest"));
                    break;

                default:
                    _ProtocolStatus = CacheValidationStatus.Fail;
                    _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_result, "ValidateRequest", _Validator.ValidationStatus.ToString()));
                    if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_unexpected_status, "ValidateRequest()", _Validator.ValidationStatus.ToString()));
                    break;
                }

                if (_ProtocolStatus != CacheValidationStatus.Continue)
                    return _ProtocolStatus;

                //
                // Proceed with validation
                //
                CheckRetrieveBeforeSubmit();
            }
            catch (Exception e) {
                _ProtocolException = e;
                _ProtocolStatus = CacheValidationStatus.Fail;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                    throw;

                if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_object_and_exception, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (e is WebException? e.Message: e.ToString())));
            }
            finally {
                if(Logging.On) Logging.Exit(Logging.RequestCache, this, "GetRetrieveStatus", "result = " + _ProtocolStatus.ToString());
            }
            return _ProtocolStatus;
        }
        //
        // This optional method is only for protocols supporting a revalidation concept
        // For a retried request this method must be called again.
        //
        internal CacheValidationStatus GetRevalidateStatus (WebResponse response, Stream responseStream)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (_ProtocolStatus == CacheValidationStatus.DoNotUseCache)
                return CacheValidationStatus.DoNotUseCache;

            // If we returned cached response, switch the state to not call cache anymore.
            if (_ProtocolStatus == CacheValidationStatus.ReturnCachedResponse)
            {
                _ProtocolStatus = CacheValidationStatus.DoNotUseCache;
                return _ProtocolStatus;
            }

            try {
                if(Logging.On) Logging.Enter(Logging.RequestCache, this, "GetRevalidateStatus", (_Validator == null? null: _Validator.Request));

                _Validator.FetchResponse(response);

                if (_ProtocolStatus != CacheValidationStatus.Continue && _ProtocolStatus != CacheValidationStatus.RetryResponseFromServer)
                {
                    if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_revalidation_not_needed, "GetRevalidateStatus()"));
                    return _ProtocolStatus;
                }
                CheckRetrieveOnResponse(responseStream);
            }
            finally {
                if(Logging.On) Logging.Exit(Logging.RequestCache, this, "GetRevalidateStatus", "result = " + _ProtocolStatus.ToString());
            }
            return _ProtocolStatus;
        }
        //
        // Returns UpdateResponseInformation if passed response stream has to be replaced (cache is updated in some way)
        // Returns Fail if request is to fail
        // Any other return value should be ignored
        //
        internal CacheValidationStatus GetUpdateStatus (WebResponse response, Stream responseStream)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (_ProtocolStatus == CacheValidationStatus.DoNotUseCache)
                return CacheValidationStatus.DoNotUseCache;

            try {
                if(Logging.On) Logging.Enter(Logging.RequestCache, this, "GetUpdateStatus", null);

                if (_Validator.Response == null)
                    _Validator.FetchResponse(response);

                if (_ProtocolStatus == CacheValidationStatus.RemoveFromCache)
                {
                    EnsureCacheRemoval(_Validator.CacheKey);
                    return _ProtocolStatus;
                }

                if (_ProtocolStatus != CacheValidationStatus.DoNotTakeFromCache &&
                    _ProtocolStatus != CacheValidationStatus.ReturnCachedResponse &&
                    _ProtocolStatus != CacheValidationStatus.CombineCachedAndServerResponse)
                {
                    if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_updated_based_on_cache_protocol_status, "GetUpdateStatus()", _ProtocolStatus.ToString()));
                    return _ProtocolStatus;
                }

                CheckUpdateOnResponse(responseStream);
            }
            catch (Exception e) {
                _ProtocolException = e;
                _ProtocolStatus = CacheValidationStatus.Fail;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_object_and_exception, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (e is WebException? e.Message: e.ToString())));
            }
            finally {
                if(Logging.On)Logging.Exit(Logging.RequestCache, this, "GetUpdateStatus", "result = " + _ProtocolStatus.ToString());
            }
            return _ProtocolStatus;
        }
        //
        // This must be the last call before starting a new request on this protocol instance
        //
        internal void Reset()
        {
            _CanTakeNewRequest = true;
        }

        //
        internal void Abort()
        {
            // if _CanTakeNewRequest==true we should not be holding any cache stream
            // Also we check on Abort() reentrancy this way.
            if (_CanTakeNewRequest)
                return;

            // in case of abnormal termination this will release cache entry sooner than does it's finalizer
            Stream stream = _ResponseStream;
            if (stream != null)
            {
                try {
                    if(Logging.On) Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_closing_cache_stream, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), "Abort()", stream.GetType().FullName, _Validator.CacheKey));
                    ICloseEx closeEx = stream as ICloseEx;
                    if (closeEx != null)
                        closeEx.CloseEx(CloseExState.Abort | CloseExState.Silent);
                    else
                        stream.Close();
                }
                catch(Exception e) {

                    if (NclUtilities.IsFatal(e)) throw;

                    if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_exception_ignored, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), "stream.Close()", e.ToString()));
                }
            }
            Reset();
        }

        //
        // Private methods
        //

        //
        // This method may be invoked as part of the request submission but before issuing a live request
        //
        private void CheckRetrieveBeforeSubmit() {

            GlobalLog.Assert(_ProtocolStatus == CacheValidationStatus.Continue, "CheckRetrieveBeforeSubmit()|Unexpected _ProtocolStatus = {0}", _ProtocolStatus);

            try {

                while (true)
                {
                    RequestCacheEntry cacheEntry;

                    if (_Validator.CacheStream != null && _Validator.CacheStream != Stream.Null)
                    {
                        // Reset to Initial state
                        _Validator.CacheStream.Close();
                        _Validator.CacheStream = Stream.Null;
                    }

                    if (_Validator.StrictCacheErrors)
                    {
                        _Validator.CacheStream = _RequestCache.Retrieve(_Validator.CacheKey, out cacheEntry);
                    }
                    else
                    {
                        Stream stream;
                        _RequestCache.TryRetrieve(_Validator.CacheKey, out cacheEntry, out stream);
                        _Validator.CacheStream = stream;
                    }

                    if (cacheEntry == null)
                    {
                        cacheEntry = new RequestCacheEntry();
                        cacheEntry.IsPrivateEntry = _RequestCache.IsPrivateCache;
                        _Validator.FetchCacheEntry(cacheEntry);
                    }

                    if (_Validator.CacheStream == null)
                    {
                        // If entry does not have a stream an empty stream wrapper must be returned.
                        // A null or Stream.Null value stands for non existent cache entry.
                        _Validator.CacheStream = Stream.Null;
                    }

                    ValidateFreshness(cacheEntry);

                    _ProtocolStatus = ValidateCache();

                    // This will tell us what to do next
                    switch (_ProtocolStatus) {

                    case CacheValidationStatus.ReturnCachedResponse:
                            if (_Validator.CacheStream == null || _Validator.CacheStream == Stream.Null)
                            {
                                if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_cache_entry, "ValidateCache()"));
                                _ProtocolStatus = CacheValidationStatus.Fail;
                                _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_no_stream, _Validator.CacheKey));
                                break;
                            }

                            // Final decision is made, check on a range response from cache
                            Stream stream = _Validator.CacheStream;
                            // The entry can now be replaced as we are not going for cache entry metadata-only  update
                            _RequestCache.UnlockEntry(_Validator.CacheStream);

                            if (_Validator.CacheStreamOffset != 0L || _Validator.CacheStreamLength != _Validator.CacheEntry.StreamSize)
                            {
                                stream =  new RangeStream(stream, _Validator.CacheStreamOffset, _Validator.CacheStreamLength);
                                if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_returned_range_cache, "ValidateCache()", _Validator.CacheStreamOffset, _Validator.CacheStreamLength));
                            }
                            _ResponseStream = stream;
                            _ResponseStreamLength = _Validator.CacheStreamLength;
                            break;

                    case CacheValidationStatus.Continue:
                            // copy a cache stream ref
                            _ResponseStream = _Validator.CacheStream;
                            break;

                    case CacheValidationStatus.RetryResponseFromCache:
                            // loop thought cache retrieve
                            continue;

                    case CacheValidationStatus.DoNotTakeFromCache:
                    case CacheValidationStatus.DoNotUseCache:
                            break;

                    case CacheValidationStatus.Fail:
                            _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_fail, "ValidateCache"));
                            break;

                    default:
                        _ProtocolStatus = CacheValidationStatus.Fail;
                        _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_result, "ValidateCache", _Validator.ValidationStatus.ToString()));
                            if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_unexpected_status, "ValidateCache()", _Validator.ValidationStatus.ToString()));
                            break;
                    }
                    break;
                }
            }
            catch (Exception e) {
                _ProtocolStatus = CacheValidationStatus.Fail;
                _ProtocolException = e;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_object_and_exception, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (e is WebException? e.Message: e.ToString())));
            }
            finally {
                // This is to release cache entry on error
                if (_ResponseStream == null && _Validator.CacheStream != null && _Validator.CacheStream != Stream.Null)
                {
                    _Validator.CacheStream.Close();
                    _Validator.CacheStream = Stream.Null;
                }
            }
        }
        //
        private void CheckRetrieveOnResponse(Stream responseStream) {

            GlobalLog.Assert(_ProtocolStatus == CacheValidationStatus.Continue || _ProtocolStatus == CacheValidationStatus.RetryResponseFromServer, "CheckRetrieveOnResponse()|Unexpected _ProtocolStatus = ", _ProtocolStatus);
            // if something goes wrong we release cache stream if any exists
            bool closeCacheStream = true;

            try {
                // This will inspect the live response on the correctness matter
                switch (_ProtocolStatus = ValidateResponse()) {

                case CacheValidationStatus.Continue:
                        closeCacheStream = false;
                        // The response looks good
                        break;

                case CacheValidationStatus.RetryResponseFromServer:
                        // The response is broken will need to retry or give up
                        closeCacheStream = false;
                        break;

                case CacheValidationStatus.Fail:
                        _ProtocolStatus = CacheValidationStatus.Fail;
                        _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_fail, "ValidateResponse"));
                        break;

                case CacheValidationStatus.DoNotUseCache:
                        break;

                default:
                    _ProtocolStatus = CacheValidationStatus.Fail;
                    _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_result, "ValidateResponse", _Validator.ValidationStatus.ToString()));
                    if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_unexpected_status, "ValidateResponse()", _Validator.ValidationStatus.ToString()));
                    break;
                }

            }
            catch (Exception e) {
                closeCacheStream = true;
                _ProtocolException = e;
                _ProtocolStatus = CacheValidationStatus.Fail;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_object_and_exception, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (e is WebException? e.Message: e.ToString())));
            }
            finally {
                // This is to release cache entry in case we are not interested in it
                if (closeCacheStream && _ResponseStream != null)
                {
                    _ResponseStream.Close();
                    _ResponseStream = null;
                    _Validator.CacheStream = Stream.Null;
                }
            }

            if (_ProtocolStatus != CacheValidationStatus.Continue) {
                return;
            }

            //
            // only CacheValidationStatus.Continue goes here with closeCacheStream == false
            //

            try {
                // This will tell us what to do next
                // Note this is a second time question to the caching protocol about a cached entry
                // Except that we now have live response to consider
                //
                // The validator can at any time replace the  cache stream and update cache Metadata (aka headers).
                //
                switch (_ProtocolStatus = RevalidateCache()) {

                case CacheValidationStatus.DoNotUseCache:
                case CacheValidationStatus.RemoveFromCache:
                case CacheValidationStatus.DoNotTakeFromCache:
                        closeCacheStream = true;
                        break;

                case CacheValidationStatus.ReturnCachedResponse:
                        if (_Validator.CacheStream == null || _Validator.CacheStream == Stream.Null)
                        {
                            _ProtocolStatus = CacheValidationStatus.Fail;
                            _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_no_stream, _Validator.CacheKey));
                            if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_null_cached_stream, "RevalidateCache()"));
                            break;
                        }

                        Stream stream = _Validator.CacheStream;

                        if (_Validator.CacheStreamOffset != 0L || _Validator.CacheStreamLength != _Validator.CacheEntry.StreamSize)
                        {
                            stream =  new RangeStream(stream, _Validator.CacheStreamOffset, _Validator.CacheStreamLength);
                            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_returned_range_cache, "RevalidateCache()", _Validator.CacheStreamOffset, _Validator.CacheStreamLength));
                        }
                        _ResponseStream = stream;
                        _ResponseStreamLength = _Validator.CacheStreamLength;
                        break;

                case CacheValidationStatus.CombineCachedAndServerResponse:

                        if (_Validator.CacheStream == null || _Validator.CacheStream == Stream.Null)
                        {
                            _ProtocolStatus = CacheValidationStatus.Fail;
                            _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_no_stream, _Validator.CacheKey));
                            if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_requested_combined_but_null_cached_stream, "RevalidateCache()"));
                            break;
                        }
                        //
                        // FTP cannot give the tail of the combined stream at that point
                        // Consider: Revisit the design of the CacheProtocol class
                        if (responseStream != null)
                        {
                            stream = new CombinedReadStream(_Validator.CacheStream, responseStream);
                        }
                        else
                        {
                            // So Abort can close the cache stream
                            stream = _Validator.CacheStream;
                        }
                        _ResponseStream = stream;
                        _ResponseStreamLength = _Validator.CacheStreamLength;
                        break;


                case CacheValidationStatus.Fail:
                        closeCacheStream = true;
                        _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_fail, "RevalidateCache"));
                        break;

                default:
                        closeCacheStream = true;
                        _ProtocolStatus = CacheValidationStatus.Fail;
                        _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_result, "RevalidateCache", _Validator.ValidationStatus.ToString()));
                        if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_unexpected_status, "RevalidateCache()", _Validator.ValidationStatus.ToString()));
                        break;
                }
            }
            catch (Exception e) {
                closeCacheStream = true;
                _ProtocolException = e;
                _ProtocolStatus = CacheValidationStatus.Fail;
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_object_and_exception, "CacheProtocol#" + this.GetHashCode().ToString(NumberFormatInfo.InvariantInfo), (e is WebException? e.Message: e.ToString())));
            }
            finally {
                // This is to release cache entry in case we are not interested in it
                if (closeCacheStream && _ResponseStream != null)
                {
                    _ResponseStream.Close();
                    _ResponseStream = null;
                    _Validator.CacheStream = Stream.Null;
                }
            }
        }
        //
        // This will decide on cache update and construct the effective response stream
        //
        private void CheckUpdateOnResponse(Stream responseStream)
        {

            if  (_Validator.CacheEntry == null)
            {
                // There was no chance to create an empty entry yet
                RequestCacheEntry cacheEntry = new RequestCacheEntry();
                cacheEntry.IsPrivateEntry = _RequestCache.IsPrivateCache;
                _Validator.FetchCacheEntry(cacheEntry);
            }

            // With NoCache we may end up storing whole response as a new entry in Cache.
            // Otherwise we may end up updating Context+Metadata or just Context
            //
            // In any case we may end up doing nothing.
            //
            string retrieveKey = _Validator.CacheKey;

            bool unlockEntry = true;
            try {
                switch (_ProtocolStatus=UpdateCache()) {

                case CacheValidationStatus.RemoveFromCache:
                        EnsureCacheRemoval(retrieveKey);
                        unlockEntry = false;
                        break;

                case CacheValidationStatus.UpdateResponseInformation:
                        // NB: Just invoked validator must have updated CacheEntry and transferred
                        //     ONLY allowed headers from the response to the Context.xxxMetadata member

                        _ResponseStream = new MetadataUpdateStream(
                                                                    responseStream,
                                                                    _RequestCache,
                                                                    _Validator.CacheKey,
                                                                    _Validator.CacheEntry.ExpiresUtc,
                                                                    _Validator.CacheEntry.LastModifiedUtc,
                                                                    _Validator.CacheEntry.LastSynchronizedUtc,
                                                                    _Validator.CacheEntry.MaxStale,
                                                                    _Validator.CacheEntry.EntryMetadata,
                                                                    _Validator.CacheEntry.SystemMetadata,
                                                                    _Validator.StrictCacheErrors);
                        //
                        // This can be looked as a design hole since we have to keep the entry
                        // locked for the case when we want to update that previously retrieved entry.
                        // I think RequestCache contract should allow to detect that a new physical cache entry
                        // does not match to the "entry being updated" and so to should ignore updates on replaced entries.
                        //
                        unlockEntry = false;
                        _ProtocolStatus = CacheValidationStatus.UpdateResponseInformation;
                        break;

                case CacheValidationStatus.CacheResponse:
                        // NB: Just invoked validator must have updated CacheEntry and transferred
                        //     ONLY allowed headers from the response to the Context.xxxMetadata member

                        Stream stream;
                        if (_Validator.StrictCacheErrors)
                            {stream = _RequestCache.Store(_Validator.CacheKey, _Validator.CacheEntry.StreamSize, _Validator.CacheEntry.ExpiresUtc, _Validator.CacheEntry.LastModifiedUtc, _Validator.CacheEntry.MaxStale, _Validator.CacheEntry.EntryMetadata, _Validator.CacheEntry.SystemMetadata);}
                        else
                            {_RequestCache.TryStore(_Validator.CacheKey, _Validator.CacheEntry.StreamSize, _Validator.CacheEntry.ExpiresUtc, _Validator.CacheEntry.LastModifiedUtc, _Validator.CacheEntry.MaxStale, _Validator.CacheEntry.EntryMetadata, _Validator.CacheEntry.SystemMetadata, out stream);}

                        // Wrap the response stream into forwarding one
                        if (stream == null) {
                            _ProtocolStatus = CacheValidationStatus.DoNotUpdateCache;
                        }
                        else {
                            _ResponseStream = new ForwardingReadStream(responseStream, stream, _Validator.CacheStreamOffset, _Validator.StrictCacheErrors);
                            _ProtocolStatus = CacheValidationStatus.UpdateResponseInformation;
                        }
                        break;

                case CacheValidationStatus.DoNotUseCache:
                case CacheValidationStatus.DoNotUpdateCache:
                        break;

                case CacheValidationStatus.Fail:
                        _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_fail, "UpdateCache"));
                        break;
                default:
                        _ProtocolStatus = CacheValidationStatus.Fail;
                        _ProtocolException = new InvalidOperationException(SR.GetString(SR.net_cache_validator_result, "UpdateCache", _Validator.ValidationStatus.ToString()));
                        if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_unexpected_status, "UpdateCache()", _Validator.ValidationStatus.ToString()));
                        break;
                }
            }
            finally {
                if (unlockEntry)
                {
                    // The entry can now be replaced as we are not going for cache entry metadata-only  update
                    _RequestCache.UnlockEntry(_Validator.CacheStream);
                }
            }
        }
        //
        private CacheValidationStatus ValidateRequest() {

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache,
                                        "Request#" + _Validator.Request.GetHashCode().ToString(NumberFormatInfo.InvariantInfo) +
                                        ", Policy = " + _Validator.Request.CachePolicy.ToString() +
                                        ", Cache Uri = " + _Validator.Uri);

            CacheValidationStatus result = _Validator.ValidateRequest();
            _Validator.SetValidationStatus(result);

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, "Selected cache Key = " + _Validator.CacheKey);
            return result;
        }
        //
        //
        //
        private void ValidateFreshness(RequestCacheEntry fetchEntry) {

            _Validator.FetchCacheEntry(fetchEntry);

            if (_Validator.CacheStream == null || _Validator.CacheStream == Stream.Null) {
                if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_entry_not_found_freshness_undefined, "ValidateFreshness()"));
                _Validator.SetFreshnessStatus(CacheFreshnessStatus.Undefined);
                return;
            }

            if(Logging.On) {
                if (Logging.IsVerbose(Logging.RequestCache)) {
                    Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_dumping_cache_context));

                    if (fetchEntry == null) {
                        Logging.PrintInfo(Logging.RequestCache, "<null>");
                    }
                    else {
                        string[] context = fetchEntry.ToString(Logging.IsVerbose(Logging.RequestCache)).Split(RequestCache.LineSplits);

                        for (int i = 0; i< context.Length; ++i) {
                            if (context[i].Length != 0) {
                                Logging.PrintInfo(Logging.RequestCache, context[i]);
                            }
                        }
                    }
                }
            }

            CacheFreshnessStatus result = _Validator.ValidateFreshness();
            _Validator.SetFreshnessStatus(result);
            _IsCacheFresh = result == CacheFreshnessStatus.Fresh;

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_result, "ValidateFreshness()", result.ToString()));
        }

        //
        //
        //
        private CacheValidationStatus ValidateCache() {
            CacheValidationStatus result = _Validator.ValidateCache();
            _Validator.SetValidationStatus(result);

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_result, "ValidateCache()", result.ToString()));
            return result;
        }
        //
        private CacheValidationStatus RevalidateCache() {

            CacheValidationStatus result = _Validator.RevalidateCache();
            _Validator.SetValidationStatus(result);

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_result, "RevalidateCache()", result.ToString()));
            return result;
        }
        //
        private CacheValidationStatus ValidateResponse()
        {

            CacheValidationStatus result = _Validator.ValidateResponse();
            _Validator.SetValidationStatus(result);

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_result, "ValidateResponse()", result.ToString()));
            return result;
        }
        //
        private CacheValidationStatus UpdateCache()
        {
            CacheValidationStatus result = _Validator.UpdateCache();
            _Validator.SetValidationStatus(result);

            return result;
        }
        //
        private void EnsureCacheRemoval(string retrieveKey)
        {
            // The entry can now be replaced as we are not going for cache entry metadata-only  update
            _RequestCache.UnlockEntry(_Validator.CacheStream);

            if (_Validator.StrictCacheErrors)
                {_RequestCache.Remove(retrieveKey);}
            else
                {_RequestCache.TryRemove(retrieveKey);}

            // We may need to remove yet another reference from the cache
            if (retrieveKey != _Validator.CacheKey)
            {
                if (_Validator.StrictCacheErrors)
                    {_RequestCache.Remove(_Validator.CacheKey);}
                else
                    {_RequestCache.TryRemove(_Validator.CacheKey);}
            }
        }

    }
}
