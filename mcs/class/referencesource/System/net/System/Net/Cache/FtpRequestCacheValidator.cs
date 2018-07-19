/*++
Copyright (c) Microsoft Corporation

Module Name:

    FtpRequestCacheValidator.cs

Abstract:
    The class implements FTP Caching validators

Author:

    Alexei Vopilov    3-Aug-2004

Revision History:

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


    // The class represents an adavanced way for an application to control caching protocol
    internal class FtpRequestCacheValidator: HttpRequestCacheValidator {

        DateTime       m_LastModified;
        bool           m_HttpProxyMode;

        private bool                    HttpProxyMode    {get{return m_HttpProxyMode;}}
        internal new RequestCachePolicy Policy           {get {return ((RequestCacheValidator)this).Policy;}}

        //
        private void ZeroPrivateVars()
        {
            m_LastModified      = DateTime.MinValue;
            m_HttpProxyMode     = false;
        }

        //public
        internal override RequestCacheValidator CreateValidator()
        {
            return new FtpRequestCacheValidator(StrictCacheErrors, UnspecifiedMaxAge);
        }

        //public
        internal FtpRequestCacheValidator(bool strictCacheErrors, TimeSpan unspecifiedMaxAge): base(strictCacheErrors, unspecifiedMaxAge)
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
        protected internal override CacheValidationStatus ValidateRequest()
        {
            // cleanup context after previous  request
            ZeroPrivateVars();

            if (Request is HttpWebRequest)
            {
                m_HttpProxyMode = true;
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_ftp_proxy_doesnt_support_partial));
                return base.ValidateRequest();
            }

            if (Policy.Level == RequestCacheLevel.BypassCache)
                return CacheValidationStatus.DoNotUseCache;

            string method = Request.Method.ToUpper(CultureInfo.InvariantCulture);
            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_ftp_method, method));

            switch (method) {
                case WebRequestMethods.Ftp.DownloadFile:  RequestMethod = HttpMethod.Get;   break;
                case WebRequestMethods.Ftp.UploadFile:    RequestMethod = HttpMethod.Put;break;
                case WebRequestMethods.Ftp.AppendFile:    RequestMethod = HttpMethod.Put;break;
                case WebRequestMethods.Ftp.Rename:        RequestMethod = HttpMethod.Put;break;
                case WebRequestMethods.Ftp.DeleteFile:    RequestMethod = HttpMethod.Delete;break;

                default:        RequestMethod = HttpMethod.Other;    break;
            }

            if ((RequestMethod != HttpMethod.Get || !((FtpWebRequest)Request).UseBinary) && Policy.Level == RequestCacheLevel.CacheOnly)
            {
                // Throw because the request must hit the wire and it's cache-only policy
                FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
            }

            if (method != WebRequestMethods.Ftp.DownloadFile)
                return CacheValidationStatus.DoNotTakeFromCache;

            if (!((FtpWebRequest)Request).UseBinary)
            {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_ftp_supports_bin_only));
                return CacheValidationStatus.DoNotUseCache;
            }

            if (Policy.Level >= RequestCacheLevel.Reload)
                return CacheValidationStatus.DoNotTakeFromCache;

            return CacheValidationStatus.Continue;
        }

        //
        // This validation method is called after caching protocol has retrieved the metadata of a cached entry.
        // Given the cached entry context, the request instance and the effective caching policy,
        // the handler has to decide whether a cached item can be considered as fresh.
        protected internal override CacheFreshnessStatus ValidateFreshness()
        {

            if (HttpProxyMode)
            {
                if (CacheStream != Stream.Null)
                {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_replacing_entry_with_HTTP_200));

                    // HTTP validator cannot parse FTP status code and other metadata
                    if (CacheEntry.EntryMetadata == null)
                        CacheEntry.EntryMetadata = new StringCollection();

                    CacheEntry.EntryMetadata.Clear();
                    CacheEntry.EntryMetadata.Add("HTTP/1.1 200 OK");
                }
                return base.ValidateFreshness();
            }

            DateTime nowDate = DateTime.UtcNow;

            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_now_time, nowDate.ToString("r", CultureInfo.InvariantCulture)));

            // If absolute Expires can be recovered
            if (CacheEntry.ExpiresUtc != DateTime.MinValue)
            {
                //Take absolute Expires value
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_age_absolute, CacheEntry.ExpiresUtc.ToString("r", CultureInfo.InvariantCulture)));
                if (CacheEntry.ExpiresUtc < nowDate) {
                    return CacheFreshnessStatus.Stale;
                }
                return CacheFreshnessStatus.Fresh;
            }

            TimeSpan age  = TimeSpan.MaxValue;

            if(CacheEntry.LastSynchronizedUtc != DateTime.MinValue)
            {
                age = nowDate - CacheEntry.LastSynchronizedUtc;
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_age1, ((int)age.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture)));
            }

            //
            // Heruistic expiration
            //
            if (CacheEntry.LastModifiedUtc != DateTime.MinValue)
            {
                TimeSpan span = (nowDate - CacheEntry.LastModifiedUtc);
                int maxAgeSeconds = (int)(span.TotalSeconds/10);
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_max_age_use_10_percent, maxAgeSeconds.ToString(NumberFormatInfo.InvariantInfo), CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture)));
                if (age.TotalSeconds < maxAgeSeconds) {
                    return CacheFreshnessStatus.Fresh;
                }
                return CacheFreshnessStatus.Stale;
            }

            // Else we can only rely on UnspecifiedMaxAge hint
            if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_max_age_use_default, ((int)(UnspecifiedMaxAge.TotalSeconds)).ToString(NumberFormatInfo.InvariantInfo)));
            if (UnspecifiedMaxAge >= age)
            {
                return CacheFreshnessStatus.Fresh;
            }

            return CacheFreshnessStatus.Stale;
            //return OnValidateFreshness(this);
        }

        // This method may add headers under the "Warning" header name
        protected internal override CacheValidationStatus ValidateCache()
        {
            if (HttpProxyMode)
                return base.ValidateCache();

            if (Policy.Level >= RequestCacheLevel.Reload)
            {
                // For those policies cache is never returned
                GlobalLog.Assert("OnValidateCache()", "This validator should not be called for policy = " + Policy.ToString());
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_validator_invalid_for_policy, Policy.ToString()));
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            // First check is do we have a cached entry at all?
            if (CacheStream == Stream.Null || CacheEntry.IsPartialEntry)
            {
                if (Policy.Level == RequestCacheLevel.CacheOnly)
                {
                    // Throw because entry was not found and it's cache-only policy
                    FailRequest(WebExceptionStatus.CacheEntryNotFound);
                }
                if (CacheStream == Stream.Null)
                {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                // Otherwise it's a partial entry and we can go on the wire
            }

            CacheStreamOffset = 0L;
            CacheStreamLength = CacheEntry.StreamSize;

            //
            // Before request submission validation
            //
            if (Policy.Level == RequestCacheLevel.Revalidate || CacheEntry.IsPartialEntry)
            {
                return TryConditionalRequest();
            }

            long contentOffset = Request is FtpWebRequest ? ((FtpWebRequest)Request).ContentOffset: 0L;

            if (CacheFreshnessStatus == CacheFreshnessStatus.Fresh || Policy.Level == RequestCacheLevel.CacheOnly || Policy.Level == RequestCacheLevel.CacheIfAvailable)
            {
                if (contentOffset != 0)
                {
                    if (contentOffset >= CacheStreamLength)
                    {
                        if (Policy.Level == RequestCacheLevel.CacheOnly)
                        {
                            // Throw because request is outside of cached size and it's cache-only policy
                            FailRequest(WebExceptionStatus.CacheEntryNotFound);
                        }
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    CacheStreamOffset = contentOffset;
                }
                return CacheValidationStatus.ReturnCachedResponse;
            }

            return CacheValidationStatus.DoNotTakeFromCache;
        }
        //
        // This is (optionally) called after receiveing a live response
        //
        protected internal override CacheValidationStatus RevalidateCache()
        {
            if (HttpProxyMode)
                return base.RevalidateCache();


            if (Policy.Level >= RequestCacheLevel.Reload)
            {
                // For those policies cache is never returned
                GlobalLog.Assert("RevalidateCache()", "This validator should not be called for policy = " + Policy.ToString());
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_validator_invalid_for_policy, Policy.ToString()));
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            // First check is do we still hold on a cached entry?
            if (CacheStream == Stream.Null)
            {
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            //
            // This is a second+ time validation after receiving at least one response
            //

            CacheValidationStatus result = CacheValidationStatus.DoNotTakeFromCache;

            FtpWebResponse resp = Response as FtpWebResponse;
            if (resp == null)
            {
                // This will result to an application error
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            if (resp.StatusCode == FtpStatusCode.FileStatus)
            {
                if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_response_last_modified, resp.LastModified.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture), resp.ContentLength));
                if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_cache_last_modified, CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture), CacheEntry.StreamSize));

                if (CacheStreamOffset != 0L && CacheEntry.IsPartialEntry)
                {
                    //should never happen
                    if(Logging.On) Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_partial_and_non_zero_content_offset, CacheStreamOffset.ToString(CultureInfo.InvariantCulture))); 
                    result = CacheValidationStatus.DoNotTakeFromCache;
                }

                if (resp.LastModified.ToUniversalTime() == CacheEntry.LastModifiedUtc)
                {
                    if (CacheEntry.IsPartialEntry)
                    {
                        // A caller will need to use Validator.CacheEntry.StreamSize to figure out what the restart point is

                        if (resp.ContentLength > 0)
                            this.CacheStreamLength = resp.ContentLength;
                        else
                            this.CacheStreamLength = -1;

                        result = CacheValidationStatus.CombineCachedAndServerResponse;
                    }
                    else if (resp.ContentLength == CacheEntry.StreamSize)
                    {
                        result = CacheValidationStatus.ReturnCachedResponse;
                    }
                    else
                        result = CacheValidationStatus.DoNotTakeFromCache;
                }
                else
                    result = CacheValidationStatus.DoNotTakeFromCache;
            }
            else
            {
                result =  CacheValidationStatus.DoNotTakeFromCache;
            }

            return result;
        }

        //
        // This validation method is responsible to answer whether the live response is sufficient to make
        // the final decision for caching protocol.
        // This is useful in case of possible failure or inconsistent results received from
        // the remote cache.
        //
        /// Invalid response from this method means the request was internally modified and should be retried </remarks>
        protected internal override CacheValidationStatus ValidateResponse()
        {
            if (HttpProxyMode)
                return base.ValidateResponse();

            if (Policy.Level != RequestCacheLevel.Default && Policy.Level != RequestCacheLevel.Revalidate)
            {
                // Those policy levels do not modify requests
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_response_valid_based_on_policy, Policy.ToString()));
                return CacheValidationStatus.Continue;
            }

            FtpWebResponse resp = Response as FtpWebResponse;

            if (resp == null) {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_null_response_failure));
                return CacheValidationStatus.Continue;
            }

            if(Logging.On) Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_ftp_response_status, ((int)resp.StatusCode).ToString(CultureInfo.InvariantCulture), resp.StatusCode.ToString()));

            // If there was a retry already, it should go with cache disabled so by default we won't retry it again
            if (ResponseCount > 1) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_valid_based_on_retry, ResponseCount));
                return CacheValidationStatus.Continue;
            }

            if (resp.StatusCode != FtpStatusCode.OpeningData && resp.StatusCode != FtpStatusCode.FileStatus)
            {
                return CacheValidationStatus.RetryResponseFromServer;
            }
            return CacheValidationStatus.Continue;
        }

        ///This action handler is responsible for making final decision on whether
        // a received response can be cached.
        // Invalid result from this method means the response must not be cached
        protected internal override CacheValidationStatus UpdateCache()
        {
            if (HttpProxyMode)
                return base.UpdateCache();

            // An combined cace+wire response is not supported if user has specified a restart offset.
            CacheStreamOffset = 0L;

            if (RequestMethod == HttpMethod.Other)
            {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_updated_based_on_policy, Request.Method));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            if (ValidationStatus == CacheValidationStatus.RemoveFromCache) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_removed_existing_invalid_entry));
                return CacheValidationStatus.RemoveFromCache;
            }

            if (Policy.Level == RequestCacheLevel.CacheOnly) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_updated_based_on_policy, Policy.ToString()));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            FtpWebResponse resp = Response as FtpWebResponse;

            if (resp == null)
            {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_updated_because_no_response));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            //
            // Check on cache removal based on the request method
            //
            if (RequestMethod == HttpMethod.Delete || RequestMethod == HttpMethod.Put)
            {
                if (RequestMethod == HttpMethod.Delete ||
                    resp.StatusCode == FtpStatusCode.OpeningData ||
                    resp.StatusCode == FtpStatusCode.DataAlreadyOpen ||
                    resp.StatusCode == FtpStatusCode.FileActionOK ||
                    resp.StatusCode == FtpStatusCode.ClosingData)
                {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_removed_existing_based_on_method, Request.Method));
                    return CacheValidationStatus.RemoveFromCache;
                }
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_existing_not_removed_because_unexpected_response_status, (int)resp.StatusCode, resp.StatusCode.ToString()));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            if (Policy.Level == RequestCacheLevel.NoCacheNoStore) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_removed_existing_based_on_policy, Policy.ToString()));
                return CacheValidationStatus.RemoveFromCache;
            }

            if (ValidationStatus == CacheValidationStatus.ReturnCachedResponse)
            {
                // have a response still returning from cache means just revalidated the entry.
                return UpdateCacheEntryOnRevalidate();
            }

            if (resp.StatusCode != FtpStatusCode.OpeningData 
                && resp.StatusCode != FtpStatusCode.DataAlreadyOpen
                && resp.StatusCode != FtpStatusCode.ClosingData)
            {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_updated_based_on_ftp_response_status, FtpStatusCode.OpeningData.ToString() + "|" + FtpStatusCode.DataAlreadyOpen.ToString() + "|" + FtpStatusCode.ClosingData.ToString(), resp.StatusCode.ToString()));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            // Check on no-update or cache removal if restart action has invalidated existing cache entry
            if (((FtpWebRequest)Request).ContentOffset != 0L)
            {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_update_not_supported_for_ftp_restart, ((FtpWebRequest)Request).ContentOffset.ToString(CultureInfo.InvariantCulture)));
                if (CacheEntry.LastModifiedUtc != DateTime.MinValue && resp.LastModified.ToUniversalTime() != CacheEntry.LastModifiedUtc)
                {
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_removed_entry_because_ftp_restart_response_changed, CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture), resp.LastModified.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture)));
                    return CacheValidationStatus.RemoveFromCache;
                }
                return CacheValidationStatus.DoNotUpdateCache;
            }

            return UpdateCacheEntryOnStore();
        }

        //
        //
        //
        private CacheValidationStatus UpdateCacheEntryOnStore()
        {
            CacheEntry.EntryMetadata  = null;
            CacheEntry.SystemMetadata = null;

            FtpWebResponse resp = Response as FtpWebResponse;
            if (resp.LastModified != DateTime.MinValue)
            {
                CacheEntry.LastModifiedUtc = resp.LastModified.ToUniversalTime();
            }

            ResponseEntityLength = Response.ContentLength;
            CacheEntry.StreamSize = ResponseEntityLength;       //This is passed down to cache on what size to expect
            CacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
            return CacheValidationStatus.CacheResponse;
        }
        //
        //
        private CacheValidationStatus UpdateCacheEntryOnRevalidate()
        {
            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_last_synchronized, CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture)));

            DateTime nowUtc = DateTime.UtcNow;
            if (CacheEntry.LastSynchronizedUtc + TimeSpan.FromMinutes(1) >= nowUtc)
            {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_suppress_update_because_synched_last_minute));
                return CacheValidationStatus.DoNotUpdateCache;
            }

            CacheEntry.EntryMetadata  = null;
            CacheEntry.SystemMetadata = null;

            CacheEntry.LastSynchronizedUtc = nowUtc;

            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_updating_last_synchronized, CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture)));

            return CacheValidationStatus.UpdateResponseInformation;
        }

        //
        private CacheValidationStatus TryConditionalRequest()
        {
            FtpWebRequest request = Request as FtpWebRequest;
            if (request == null || !request.UseBinary)
                return CacheValidationStatus.DoNotTakeFromCache;

            if (request.ContentOffset != 0L)
            {
                if (CacheEntry.IsPartialEntry || request.ContentOffset >= CacheStreamLength)
                    return CacheValidationStatus.DoNotTakeFromCache;
                CacheStreamOffset = request.ContentOffset;
            }
            return CacheValidationStatus.Continue;
        }

    }
}

