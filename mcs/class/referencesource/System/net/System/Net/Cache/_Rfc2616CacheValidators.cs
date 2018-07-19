/*++
Copyright (c) Microsoft Corporation

Module Name:

    _Rfc2616CacheValidators.cs

Abstract:
    The class implements set of HTTP validators as per RFC2616

Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

--*/
namespace System.Net.Cache {
using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;


    //
    // Caching RFC
    //
    internal class Rfc2616 {

        private Rfc2616() {
        }

        internal enum TriState {
            Unknown,
            Valid,
            Invalid
        }

        /*----------*/
        // Continue           = Proceed to the next protocol stage.
        // DoNotTakeFromCache = Don't used caches value for this request
        // DoNotUseCache      = Cache is not used for this request and response is not cached.
        public static CacheValidationStatus OnValidateRequest(HttpRequestCacheValidator ctx)
        {

            CacheValidationStatus  result = Common.OnValidateRequest(ctx);

            if (result == CacheValidationStatus.DoNotUseCache)
            {
                return result;
            }

            /*
               HTTP/1.1 caches SHOULD treat "Pragma: no-cache" as if the client had
               sent "Cache-Control: no-cache". No new Pragma directives will be
               defined in HTTP.

               we use above information to remove pragma header (we control it itself)
            */
            ctx.Request.Headers.RemoveInternal(HttpKnownHeaderNames.Pragma);

            /*
                we want to control cache-control header as well, any specifi extensions should be done
                using a derived validator class and custom policy
            */
            ctx.Request.Headers.RemoveInternal(HttpKnownHeaderNames.CacheControl);

            if (ctx.Policy.Level == HttpRequestCacheLevel.NoCacheNoStore)
            {
                //adjust request headers since retrieval validators will be suppressed upon return.
                ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "no-store");
                ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "no-cache");
                ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.Pragma, "no-cache");
                result = CacheValidationStatus.DoNotTakeFromCache;
            }
            else if (result == CacheValidationStatus.Continue)
            {
                if (ctx.Policy.Level == HttpRequestCacheLevel.Reload || ctx.Policy.Level == HttpRequestCacheLevel.NoCacheNoStore)
                {
                //adjust request headers since retrieval validators will be suppressed upon return.
                ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "no-cache");
                ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.Pragma, "no-cache");
                result = CacheValidationStatus.DoNotTakeFromCache;
                }
                else if (ctx.Policy.Level == HttpRequestCacheLevel.Refresh)
                {
                    //adjust request headers since retrieval validators will be suppressed upon return.
                    ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "max-age=0");
                    ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.Pragma, "no-cache");
                    result = CacheValidationStatus.DoNotTakeFromCache;
                }
                else if (ctx.Policy.Level == HttpRequestCacheLevel.Default)
                {
                    //Transfer Policy into CacheControl directives
                    if (ctx.Policy.MinFresh > TimeSpan.Zero) {
                        ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "min-fresh=" + (int)ctx.Policy.MinFresh.TotalSeconds);
                    }
                    if (ctx.Policy.MaxAge != TimeSpan.MaxValue) {
                        ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "max-age=" + (int)ctx.Policy.MaxAge.TotalSeconds);
                    }
                    if (ctx.Policy.MaxStale > TimeSpan.Zero) {
                        ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "max-stale=" + (int)ctx.Policy.MaxStale.TotalSeconds);
                    }
                }
                else if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly || ctx.Policy.Level == HttpRequestCacheLevel.CacheOrNextCacheOnly)
                {
                    // In case other validators will not be called
                    ctx.Request.Headers.AddInternal(HttpKnownHeaderNames.CacheControl, "only-if-cached");
                }
            }
            return result;
        }
        /*----------*/
        public static CacheFreshnessStatus OnValidateFreshness(HttpRequestCacheValidator  ctx)
        {
            // This will figure out ctx.CacheAge and ctx.CacheMaxAge memebers
            CacheFreshnessStatus result = Common.ComputeFreshness(ctx);

            /*
               We note one exception to this rule: since some applications have
               traditionally used GETs and HEADs with query URLs (those containing a
               "?" in the rel_path part) to perform operations with significant side
               effects, caches MUST NOT treat responses to such URIs as fresh unless
               the server provides an explicit expiration time. This specifically
               means that responses from HTTP/1.0 servers for such URIs SHOULD NOT
               be taken from a cache. See section 9.1.1 for related information.
            */
            if (ctx.Uri.Query.Length != 0) {
                if (ctx.CacheHeaders.Expires == null && (ctx.CacheEntry.IsPrivateEntry?ctx.CacheCacheControl.MaxAge == -1:ctx.CacheCacheControl.SMaxAge == -1)) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_uri_with_query_has_no_expiration));
                    return CacheFreshnessStatus.Stale;
                }
                if (ctx.CacheHttpVersion.Major <= 1 && ctx.CacheHttpVersion.Minor < 1) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_uri_with_query_and_cached_resp_from_http_10));
                    return CacheFreshnessStatus.Stale;
                }
            }

            return result;

        }

        /*----------*/
        // ReturnCachedResponse        =  Return cached response to the application
        // DoNotTakeFromCache          =  Don't used caches value for this request
        // Continue                    =  Proceed to the next protocol stage.
        public static CacheValidationStatus OnValidateCache(HttpRequestCacheValidator ctx)
        {

            if (Common.ValidateCacheByVaryHeader(ctx) == TriState.Invalid) {
                // RFC 2616 is tricky on this. In theory we could make a conditional request.
                // However we rather will not.
                // And the reason can be deducted from the RFC definitoin of the response Vary Header.
                return CacheValidationStatus.DoNotTakeFromCache;
            }


            // For Revalidate option we perform a wire request anyway
            if (ctx.Policy.Level == HttpRequestCacheLevel.Revalidate) {
                return Common.TryConditionalRequest(ctx);
            }

            if (Common.ValidateCacheBySpecialCases(ctx) == TriState.Invalid)
            {
                // This takes over the cache policy since the cache content may be sematically incorrect
                if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly) {
                    // Cannot do a wire request
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                return Common.TryConditionalRequest(ctx);
            }

            // So now we have either fresh or stale entry that might be used in place of the response
            // At this point it's safe to consider cache freshness and effective Policy as the core decision rules
            // Reminder: This method should not be executed with Level >= CacheLevel.Refresh

            bool enoughFresh = Common.ValidateCacheByClientPolicy(ctx);

            if (enoughFresh || ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly || ctx.Policy.Level == HttpRequestCacheLevel.CacheIfAvailable || ctx.Policy.Level == HttpRequestCacheLevel.CacheOrNextCacheOnly)
            {
                // The freshness does not matter, check does user requested Range fits into cached entry
                CacheValidationStatus result = Common.TryResponseFromCache(ctx);

                if (result != CacheValidationStatus.ReturnCachedResponse) {
                    if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly) {
                        // Cannot do a wire request
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }
                    return result;
                }

                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_valid_as_fresh_or_because_policy, ctx.Policy.ToString()));
                return CacheValidationStatus.ReturnCachedResponse;
            }
            // This will return either Continue=conditional request or DoNotTakeFromCache==Unconditional request
            return Common.TryConditionalRequest(ctx);
        }

        /*----------*/
        // Returns
        // RetryResponseFromServer     =  Retry this request as the result of invalid response received
        // Continue                    =  The response can be accepted
        public static CacheValidationStatus OnValidateResponse(HttpRequestCacheValidator  ctx)
        {
            //
            // At this point we assume that policy >= CacheOrNextCacheOnly && policy < Refresh
            //


            // If there was a retry already, it should go with cache disabled so by default we won't retry it again
            if (ctx.ResponseCount > 1) {
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_accept_based_on_retry_count, ctx.ResponseCount));
                return CacheValidationStatus.Continue;
            }

            // We don't convert user-range request to a conditional one
            if (ctx.RequestRangeUser) {
                // was a user range request, we did not touch it.
                return CacheValidationStatus.Continue;
            }

            //If a live response has older Date, then request should be retried
            if (ctx.CacheDate != DateTime.MinValue &&
                ctx.ResponseDate != DateTime.MinValue &&
                ctx.CacheDate > ctx.ResponseDate) {
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_date_header_older_than_cache_entry));
                Common.ConstructUnconditionalRefreshRequest(ctx);
                return CacheValidationStatus.RetryResponseFromServer;
            }

            HttpWebResponse resp = ctx.Response as HttpWebResponse;
            if (ctx.RequestRangeCache && resp.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable) {

                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_server_didnt_satisfy_range, ctx.Request.Headers[HttpKnownHeaderNames.Range]));
                Common.ConstructUnconditionalRefreshRequest(ctx);
                return CacheValidationStatus.RetryResponseFromServer;
            }


            if (resp.StatusCode == HttpStatusCode.NotModified)
            {
                if (ctx.RequestIfHeader1 == null)
                {
                    // something is really broken on the wire
                    if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_304_received_on_unconditional_request));
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
                else if (ctx.RequestRangeCache)
                {
                    // The way _we_ create range requests shoyuld never result in 304
                    if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_304_received_on_unconditional_request_expected_200_206));
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
            }

            if (ctx.CacheHttpVersion.Major <= 1 && resp.ProtocolVersion.Major <=1 &&
                ctx.CacheHttpVersion.Minor < 1  && resp.ProtocolVersion.Minor <1 &&
                ctx.CacheLastModified > ctx.ResponseLastModified)
            {
                if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_last_modified_header_older_than_cache_entry));
                // On http <= 1.0 cache LastModified > resp LastModified
                Common.ConstructUnconditionalRefreshRequest(ctx);
                return CacheValidationStatus.RetryResponseFromServer;
            }

            if (ctx.Policy.Level == HttpRequestCacheLevel.Default && ctx.ResponseAge != TimeSpan.MinValue) {
                // If the client has requested MaxAge/MinFresh/MaxStale
                // check does the response meet the requirements
                if ( (ctx.ResponseAge > ctx.Policy.MaxAge) ||
                     (ctx.ResponseExpires != DateTime.MinValue &&
                     (ctx.Policy.MinFresh > TimeSpan.Zero &&  (ctx.ResponseExpires - DateTime.UtcNow) <  ctx.Policy.MinFresh) ||
                     (ctx.Policy.MaxStale > TimeSpan.Zero &&  (DateTime.UtcNow - ctx.ResponseExpires) >  ctx.Policy.MaxStale)))
                {
                    if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_freshness_outside_policy_limits));
                    Common.ConstructUnconditionalRefreshRequest(ctx);
                    return CacheValidationStatus.RetryResponseFromServer;
                }
            }

            //Cleanup what we've done to this request since protcol can resubmit for auth or redirect.
            if (ctx.RequestIfHeader1 != null) {
                ctx.Request.Headers.RemoveInternal(ctx.RequestIfHeader1);
                ctx.RequestIfHeader1 = null;
            }
            if (ctx.RequestIfHeader2 != null) {
                ctx.Request.Headers.RemoveInternal(ctx.RequestIfHeader2);
                ctx.RequestIfHeader2 = null;
            }
            if (ctx.RequestRangeCache) {
                ctx.Request.Headers.RemoveInternal(HttpKnownHeaderNames.Range);
                ctx.RequestRangeCache = false;
            }
            return CacheValidationStatus.Continue;
        }

        /*----------*/
        // Returns:
        // CacheResponse               = Replace cache entry with received live response
        // UpdateResponseInformation   = Update Metadata of cache entry using live response headers
        // RemoveFromCache             = Remove cache entry referenced to by a cache key.
        // Continue                    = Simply do not update cache.
        //
        public static CacheValidationStatus OnUpdateCache(HttpRequestCacheValidator ctx) {

            // Below condition is to get rid of a broken cache entry, we cannot update cache in that case
            if (ctx.CacheStatusCode == HttpStatusCode.NotModified) {
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_need_to_remove_invalid_cache_entry_304));
                return CacheValidationStatus.RemoveFromCache;
            }

            HttpWebResponse resp = ctx.Response as HttpWebResponse;
            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_status, resp.StatusCode));


            /*********
                Vs Whidbey#127214
                It was decided not to play with ResponseContentLocation in our implementation.
                A derived class may still want to play.

            // Compute new Cache Update Key if Content-Location is present on the response
            if (ctx.ResponseContentLocation != null) {
                if (!Uri.TryParse(ctx.ResponseContentLocation, true, true, out cacheUri)) {
                    if(Logging.On)Logging.PrintError(Logging.RequestCache, "Cannot parse Uri from Response Content-Location: " + ctx.ResponseContentLocation);
                    return CacheValidationStatus.RemoveFromCache;
                }
                if (!cacheUri.IsAbsoluteUri) {
                    try {
                        ctx.CacheKey = new Uri(ctx.RequestUri, cacheUri);
                    }
                    catch {
                        return CacheValidationStatus.RemoveFromCache;
                    }
                }
            }
            *********/

            if (ctx.ValidationStatus == CacheValidationStatus.RemoveFromCache) {
                return CacheValidationStatus.RemoveFromCache;
            }

            CacheValidationStatus noUpdateResult =
                            (ctx.RequestMethod >= HttpMethod.Post && ctx.RequestMethod <= HttpMethod.Delete || ctx.RequestMethod == HttpMethod.Other)
                                ?CacheValidationStatus.RemoveFromCache
                                :CacheValidationStatus.DoNotUpdateCache;

            if (Common.OnUpdateCache(ctx, resp) != TriState.Valid) {
                return noUpdateResult;
            }

            CacheValidationStatus result = CacheValidationStatus.CacheResponse;
            ctx.CacheEntry.IsPartialEntry = false;

            if (resp.StatusCode == HttpStatusCode.NotModified || ctx.RequestMethod == HttpMethod.Head)
            {
                result = CacheValidationStatus.UpdateResponseInformation;

                // This may take a shorter path when updating the entry
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_304_or_request_head));
                if (ctx.CacheDontUpdateHeaders) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_dont_update_cached_headers));
                    ctx.CacheHeaders = null;
                    ctx.CacheEntry.ExpiresUtc = ctx.ResponseExpires;
                    ctx.CacheEntry.LastModifiedUtc = ctx.ResponseLastModified;
                    if (ctx.Policy.Level == HttpRequestCacheLevel.Default) {
                        ctx.CacheEntry.MaxStale = ctx.Policy.MaxStale;
                    }
                    else {
                        ctx.CacheEntry.MaxStale = TimeSpan.MinValue;
                    }
                    ctx.CacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
                }
                else {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_update_cached_headers));
                }
            }
            else if (resp.StatusCode == HttpStatusCode.PartialContent)
            {
                // Check on whether the user requested range can be appended to the cache entry
                // We only support combining of non-overlapped increasing bytes ranges
                if (ctx.CacheEntry.StreamSize != ctx.ResponseRangeStart && ctx.ResponseRangeStart != 0)
                {
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_partial_resp_not_combined_with_existing_entry, ctx.CacheEntry.StreamSize, ctx.ResponseRangeStart));
                    return noUpdateResult;
                }

                // We might be appending a live stream to cache BUT user has asked for a specific range.
                // Hence don't reset CacheStreamOffset here so the protocol will create a cache forwarding stream that will hide first bytes from the user
                if (!ctx.RequestRangeUser) {
                    ctx.CacheStreamOffset = 0;
                }

                // Below code assumes that a combined response has been given to the user,

                Common.ReplaceOrUpdateCacheHeaders(ctx, resp);

                ctx.CacheHttpVersion  = resp.ProtocolVersion;
                ctx.CacheEntityLength = ctx.ResponseEntityLength;
                ctx.CacheStreamLength = ctx.CacheEntry.StreamSize = ctx.ResponseRangeEnd+1;
                if (ctx.CacheEntityLength > 0 && ctx.CacheEntityLength == ctx.CacheEntry.StreamSize)
                {
                    //eventually cache is about to store a complete response
                    Common.Construct200ok(ctx);
                }
                else
                    Common.Construct206PartialContent(ctx, 0);
            }
            else
            {
                Common.ReplaceOrUpdateCacheHeaders(ctx, resp);

                ctx.CacheHttpVersion        = resp.ProtocolVersion;
                ctx.CacheStatusCode         = resp.StatusCode;
                ctx.CacheStatusDescription  = resp.StatusDescription;
                ctx.CacheEntry.StreamSize   = resp.ContentLength;
            }

            return result;
        }


        //
        // Implements various cache validation helper methods
        //
        internal static class Common {
            public const string PartialContentDescription = "Partial Content";
            public const string OkDescription = "OK";
            //
            // Implements logic as of the Request caching suitability.
            //
            // Returns:
            // Continue           = Proceed to the next protocol stage.
            // DoNotTakeFromCache = Don't use cached response for this request
            // DoNotUseCache      = Cache is not used for this request and response is not cached.
            public static CacheValidationStatus OnValidateRequest(HttpRequestCacheValidator ctx) {

                /*
                   Some HTTP methods MUST cause a cache to invalidate an entity. This is
                   either the entity referred to by the Request-URI, or by the Location
                   or Content-Location headers (if present). These methods are:
                   PUT, DELETE, POST.

                   A cache that passes through requests for methods it does not
                   understand SHOULD invalidate any entities referred to by the
                   Request-URI
                */
                if (ctx.RequestMethod >= HttpMethod.Post && ctx.RequestMethod <= HttpMethod.Delete)
                {
                    if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                    {
                        // Throw because the request must hit the wire and it's cache-only policy
                        ctx.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
                    }
                    // here we could return a hint on removing existing entry, but UpdateCache should handle this case correctly
                    return CacheValidationStatus.DoNotTakeFromCache;
                }
                //
                // Additionally to said above we can only cache GET or HEAD, for any other methods we request bypassing cache.
                //
                if (ctx.RequestMethod < HttpMethod.Head || ctx.RequestMethod > HttpMethod.Get )
                {
                    if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                    {
                        // Throw because the request must hit the wire and it's cache-only policy
                        ctx.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
                    }
                    return CacheValidationStatus.DoNotUseCache;
                }


                if (ctx.Request.Headers[HttpKnownHeaderNames.IfModifiedSince]   != null ||
                    ctx.Request.Headers[HttpKnownHeaderNames.IfNoneMatch]       != null ||
                    ctx.Request.Headers[HttpKnownHeaderNames.IfRange]           != null ||
                    ctx.Request.Headers[HttpKnownHeaderNames.IfMatch]           != null ||
                    ctx.Request.Headers[HttpKnownHeaderNames.IfUnmodifiedSince] != null )
                {
                    // The _user_ request contains conditonal cache directives
                    // Those will conflict with the caching engine => do not lookup a cached item.
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_request_contains_conditional_header));

                    if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly)
                    {
                        // Throw because the request must hit the wire and it's cache-only policy
                        ctx.FailRequest(WebExceptionStatus.RequestProhibitedByCachePolicy);
                    }

                    return CacheValidationStatus.DoNotTakeFromCache;

                }
                return CacheValidationStatus.Continue;
            }
            //
            // Implements logic as to compute cache freshness.
            // Client Policy is not considered
            //
            public static CacheFreshnessStatus ComputeFreshness(HttpRequestCacheValidator ctx) {

                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_now_time, DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture)));

                /*
                     apparent_age = max(0, response_time - date_value);
                */

                DateTime nowDate = DateTime.UtcNow;

                TimeSpan age  = TimeSpan.MaxValue;
                DateTime date = ctx.CacheDate;

                if (date != DateTime.MinValue) {
                    age = (nowDate - date);
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_age1_date_header, ((int)age.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), ctx.CacheDate.ToString("r", CultureInfo.InvariantCulture)));
                }
                else if (ctx.CacheEntry.LastSynchronizedUtc != DateTime.MinValue) {
                    /*
                        Another way to compute cache age but only if Date header is absent.
                    */
                    age = nowDate - ctx.CacheEntry.LastSynchronizedUtc;
                    if (ctx.CacheAge != TimeSpan.MinValue) {
                        age += ctx.CacheAge;
                    }
                    if(Logging.On) {
                        if (ctx.CacheAge != TimeSpan.MinValue)
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_age1_last_synchronized_age_header, ((int)age.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), ctx.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture), ((int)ctx.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                        else
                            Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_age1_last_synchronized, ((int)age.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo), ctx.CacheEntry.LastSynchronizedUtc.ToString("r", CultureInfo.InvariantCulture))); 
                    }
                }

                /*
                    corrected_received_age = max(apparent_age, age_value);
                */
                if (ctx.CacheAge != TimeSpan.MinValue) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_age2, ((int)ctx.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                    if (ctx.CacheAge > age || age == TimeSpan.MaxValue) {
                        age = ctx.CacheAge;
                    }
                }

                // Updating CacheAge ...
                // Note we don't account on response "transit" delay
                // Also undefined cache entry Age is reported as TimeSpan.MaxValue (which is impossble to get from HTTP)
                // Also a negative age is reset to 0 as per RFC
                ctx.CacheAge = (age < TimeSpan.Zero? TimeSpan.Zero: age);

                // Now we start checking the server specified requirements

                /*
                The calculation to determine if a response has expired is quite simple:
                response_is_fresh = (freshness_lifetime > current_age)
                */

                // If we managed to compute the Cache Age
                if (ctx.CacheAge != TimeSpan.MinValue) {

                    /*
                        s-maxage
                        If a response includes an s-maxage directive, then for a shared
                        cache (but not for a private cache), the maximum age specified by
                        this directive overrides the maximum age specified by either the
                        max-age directive or the Expires header.
                    */
                    if (!ctx.CacheEntry.IsPrivateEntry && ctx.CacheCacheControl.SMaxAge != -1) {
                        ctx.CacheMaxAge = TimeSpan.FromSeconds(ctx.CacheCacheControl.SMaxAge);
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_age_cache_s_max_age, ((int)ctx.CacheMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                        if (ctx.CacheAge < ctx.CacheMaxAge) {
                            return CacheFreshnessStatus.Fresh;
                        }
                        return CacheFreshnessStatus.Stale;
                    }

                    /*
                    The max-age directive takes priority over Expires, so if max-age is
                    present in a response, the calculation is simply:
                            freshness_lifetime = max_age_value
                    */
                    if (ctx.CacheCacheControl.MaxAge != -1) {
                        ctx.CacheMaxAge = TimeSpan.FromSeconds(ctx.CacheCacheControl.MaxAge);
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_age_cache_max_age, ((int)ctx.CacheMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                        if (ctx.CacheAge < ctx.CacheMaxAge) {
                            return CacheFreshnessStatus.Fresh;
                        }
                        return CacheFreshnessStatus.Stale;
                    }
                }

                /*
                 Otherwise, if Expires is present in the response, the calculation is:
                        freshness_lifetime = expires_value - date_value
                */
                if (date == DateTime.MinValue) {
                    date = ctx.CacheEntry.LastSynchronizedUtc;
                }

                DateTime expiresDate = ctx.CacheEntry.ExpiresUtc;
                if (ctx.CacheExpires != DateTime.MinValue && ctx.CacheExpires < expiresDate) {
                    expiresDate = ctx.CacheExpires;
                }

                // If absolute Expires and Response Date and Cache Age can be recovered
                if (expiresDate != DateTime.MinValue && date != DateTime.MinValue && ctx.CacheAge != TimeSpan.MinValue) {
                    ctx.CacheMaxAge = expiresDate - date;
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_age_expires_date, ((int)((expiresDate - date).TotalSeconds)).ToString(NumberFormatInfo.InvariantInfo), expiresDate.ToString("r", CultureInfo.InvariantCulture)));
                    if (ctx.CacheAge < ctx.CacheMaxAge) {
                        return CacheFreshnessStatus.Fresh;
                    }
                    return CacheFreshnessStatus.Stale;
                }

                // If absolute Expires can be recovered
                if (expiresDate != DateTime.MinValue) {
                    ctx.CacheMaxAge = expiresDate - DateTime.UtcNow;
                    //Take absolute Expires value
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_max_age_absolute, expiresDate.ToString("r", CultureInfo.InvariantCulture)));
                    if (expiresDate < DateTime.UtcNow) {
                        return CacheFreshnessStatus.Fresh;
                    }
                    return CacheFreshnessStatus.Stale;
                }

                /*
                   If none of Expires, Cache-Control: max-age, or Cache-Control: s-
                   maxage (see section 14.9.3) appears in the response, and the response
                   does not include other restrictions on caching, the cache MAY compute
                   a freshness lifetime using a heuristic. The cache MUST attach Warning
                   113 to any response whose age is more than 24 hours if such warning
                   has not already been added.

                   Also, if the response does have a Last-Modified time, the heuristic
                   expiration value SHOULD be no more than some fraction of the interval
                   since that time. A typical setting of this fraction might be 10%.

                        response_is_fresh = (freshness_lifetime > current_age)
               */

                ctx.HeuristicExpiration = true;

                DateTime lastModifiedDate = ctx.CacheEntry.LastModifiedUtc;
                if (ctx.CacheLastModified > lastModifiedDate) {
                    lastModifiedDate = ctx.CacheLastModified;
                }                   ctx.CacheMaxAge = ctx.UnspecifiedMaxAge;

                if (lastModifiedDate != DateTime.MinValue) {
                    TimeSpan span = (nowDate - lastModifiedDate);
                    int maxAgeSeconds = (int)(span.TotalSeconds/10);
                    ctx.CacheMaxAge = TimeSpan.FromSeconds(maxAgeSeconds);
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_max_age_use_10_percent, maxAgeSeconds.ToString(NumberFormatInfo.InvariantInfo), lastModifiedDate.ToString("r", CultureInfo.InvariantCulture)));
                    if (ctx.CacheAge.TotalSeconds < maxAgeSeconds) {
                        return CacheFreshnessStatus.Fresh;
                    }
                    return CacheFreshnessStatus.Stale;
                }

                // Else we can only rely on UnspecifiedMaxAge hint
                ctx.CacheMaxAge = ctx.UnspecifiedMaxAge;
                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_max_age_use_default, ((int)(ctx.UnspecifiedMaxAge.TotalSeconds)).ToString(NumberFormatInfo.InvariantInfo)));
                if (ctx.CacheMaxAge >= ctx.CacheAge) {
                    return CacheFreshnessStatus.Fresh;
                }
                return CacheFreshnessStatus.Stale;
            }

            /*
                Returns:
                - Valid     : The cache can be updated with the response
                - Unknown   : The response should not go into cache
            */
            internal static TriState OnUpdateCache(HttpRequestCacheValidator ctx, HttpWebResponse resp) {
                /*
                    Quick check on supported methods.
                */
                if (ctx.RequestMethod != HttpMethod.Head &&
                    ctx.RequestMethod != HttpMethod.Get  &&
                    ctx.RequestMethod != HttpMethod.Post) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_not_a_get_head_post));
                    return TriState.Unknown;
                }

                //If the entry did not exist ...
                if (ctx.CacheStream == Stream.Null || (int)ctx.CacheStatusCode == 0) {
                    if(resp.StatusCode == HttpStatusCode.NotModified) {
                        // Protection from some weird case when user has changed things
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_cannot_update_cache_if_304));
                        return TriState.Unknown;
                    }
                    if (ctx.RequestMethod == HttpMethod.Head) {
                        // Protection from some caching Head response when entry does not exist.
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_cannot_update_cache_with_head_resp));
                        return TriState.Unknown;
                    }
                }


                if (resp == null) {
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_http_resp_is_null));
                    return TriState.Unknown;
                }

                //
                // We assume that ctx.ResponseCacheControl is already updated based on a live response
                //

                /*
                no-store
                      ... If sent in a response,
                      a cache MUST NOT store any part of either this response or the
                      request that elicited it.
                */
                if (ctx.ResponseCacheControl.NoStore) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_cache_control_is_no_store));
                    return TriState.Unknown;
                }


                /*
                If there is neither a cache validator nor an explicit expiration time
                   associated with a response, we do not expect it to be cached, but
                   certain caches MAY violate this expectation (for example, when little
                   or no network connectivity is available). A client can usually detect
                   that such a response was taken from a cache by comparing the Date
                   header to the current time.
                */

                // NOTE: If no Expire and no Validator peresnt we choose to CACHE
                //===============================================================


                /*
                    Note: a new response that has an older Date header value than
                    existing cached responses is not cacheable.
                */
                if (ctx.ResponseDate != DateTime.MinValue && ctx.CacheDate != DateTime.MinValue) {
                    if (ctx.ResponseDate < ctx.CacheDate) {
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_older_than_cache));
                        return TriState.Unknown;
                    }
                }

                /*
                public
                      Indicates that the response MAY be cached by any cache, even if it
                      would normally be non-cacheable or cacheable only within a non-
                      shared cache. (See also Authorization, section 14.8, for
                      additional details.)
                */
                if (ctx.ResponseCacheControl.Public) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_cache_control_is_public));
                    return TriState.Valid;
                }

                // sometiem public cache can cache a response with "private" directive, subject to other restrictions
                TriState result = TriState.Unknown;

                /*
                private
                      Indicates that all or part of the response message is intended for
                      a single user and MUST NOT be cached by a shared cache. This
                      allows an origin server to state that the specified parts of the

                      response are intended for only one user and are not a valid
                      response for requests by other users. A private (non-shared) cache
                      MAY cache the response.
                */
                if (ctx.ResponseCacheControl.Private) {
                    if (!ctx.CacheEntry.IsPrivateEntry) {
                        if (ctx.ResponseCacheControl.PrivateHeaders == null) {
                            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_cache_control_is_private));
                            return TriState.Unknown;
                        }
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_cache_control_is_private_plus_headers));
                        for (int i = 0; i < ctx.ResponseCacheControl.PrivateHeaders.Length; ++i) {
                            ctx.CacheHeaders.Remove(ctx.ResponseCacheControl.PrivateHeaders[i]);
                            result = TriState.Valid;
                        }
                    }
                    else {
                        result = TriState.Valid;
                    }
                }


                /*
                    The RFC is funky on no-cache directive.
                    But the bottom line is sometime you CAN cache no-cache responses.

                */
                if (ctx.ResponseCacheControl.NoCache)
                {
                        if (ctx.ResponseLastModified == DateTime.MinValue && ctx.Response.Headers.ETag == null) {
                            if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_revalidation_required));
                            return TriState.Unknown;
                        }
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_needs_revalidation));
                        return TriState.Valid;
                }

                if (ctx.ResponseCacheControl.SMaxAge != -1 || ctx.ResponseCacheControl.MaxAge != -1) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_allows_caching, ctx.ResponseCacheControl.ToString()));
                    return TriState.Valid;
                }

                /*
                  When a shared cache (see section 13.7) receives a request
                  containing an Authorization field, it MUST NOT return the
                  corresponding response as a reply to any other request, unless one
                  of the following specific exceptions holds:

                  1. If the response includes the "s-maxage" cache-control

                  2. If the response includes the "must-revalidate" cache-control

                  3. If the response includes the "public" cache-control directive,
                */
                if (!ctx.CacheEntry.IsPrivateEntry && ctx.Request.Headers[HttpKnownHeaderNames.Authorization] != null) {
                    // we've already passed an opportunity to cache.
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_auth_header_and_no_s_max_age));
                    return TriState.Unknown;
                }

                /*
                    POST
                    Responses to this method are not cacheable, unless the response
                    includes appropriate Cache-Control or Expires header fields.
                */
                if (ctx.RequestMethod == HttpMethod.Post && resp.Headers.Expires == null) {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_post_resp_without_cache_control_or_expires));
                    return TriState.Unknown;
                }

                /*
                 A response received with a status code of 200, 203, 206, 300, 301 or
                   410 MAY be stored by a cache and used in reply to a subsequent
                   request, subject to the expiration mechanism, unless a cache-control
                   directive prohibits caching. However, a cache that does not support
                   the Range and Content-Range headers MUST NOT cache 206 (Partial
                   Content) responses.

                   NOTE: We added 304 here which is correct
                */
                if (resp.StatusCode == HttpStatusCode.NotModified ||
                    resp.StatusCode == HttpStatusCode.OK ||
                    resp.StatusCode == HttpStatusCode.NonAuthoritativeInformation ||
                    resp.StatusCode == HttpStatusCode.PartialContent ||
                    resp.StatusCode == HttpStatusCode.MultipleChoices ||
                    resp.StatusCode == HttpStatusCode.MovedPermanently ||
                    resp.StatusCode == HttpStatusCode.Gone)
                {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_valid_based_on_status_code, (int)resp.StatusCode));
                    return TriState.Valid;
                }

                /*
                   A response received with any other status code (e.g. status codes 302
                   and 307) MUST NOT be returned in a reply to a subsequent request
                   unless there are cache-control directives or another header(s) that
                   explicitly allow it. For example, these include the following: an
                   Expires header (section 14.21); a "max-age", "s-maxage",  "must-
                   revalidate", "proxy-revalidate", "public" or "private" cache-control
                   directive (section 14.9).
                 */
                if (result != TriState.Valid) {
                    // otheriwse there was a "safe" private directive that allows caching
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_no_cache_control, (int)resp.StatusCode));
                }
                return result;
            }

            /*----------*/
            //
            // This method checks sutability of cached entry based on the client policy.
            //
            /*
                Returns:
                - true      : The cache is still good
                - false     : The cache age does not fit into client policy
            */
            public static bool ValidateCacheByClientPolicy(HttpRequestCacheValidator ctx) {

                if (ctx.Policy.Level == HttpRequestCacheLevel.Default)
                {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_age, (ctx.CacheAge != TimeSpan.MinValue ? ((int)ctx.CacheAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo) : SR.GetString(SR.net_log_unknown)), (ctx.CacheMaxAge != TimeSpan.MinValue? ((int)ctx.CacheMaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo): SR.GetString(SR.net_log_unknown))));

                    if (ctx.Policy.MinFresh > TimeSpan.Zero)
                    {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_policy_min_fresh, ((int)ctx.Policy.MinFresh.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                        if (ctx.CacheAge + ctx.Policy.MinFresh >= ctx.CacheMaxAge) {return false;}
                    }

                    if (ctx.Policy.MaxAge != TimeSpan.MaxValue)
                    {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_policy_max_age, ((int)ctx.Policy.MaxAge.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                        if (ctx.CacheAge >= ctx.Policy.MaxAge) {return false;}
                    }

                    if (ctx.Policy.InternalCacheSyncDateUtc != DateTime.MinValue)
                    {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_policy_cache_sync_date, ctx.Policy.InternalCacheSyncDateUtc.ToString("r", CultureInfo.CurrentCulture), ctx.CacheEntry.LastSynchronizedUtc.ToString(CultureInfo.CurrentCulture)));
                        if (ctx.CacheEntry.LastSynchronizedUtc < ctx.Policy.InternalCacheSyncDateUtc) {
                            return false;
                        }
                    }

                    TimeSpan adjustedMaxAge = ctx.CacheMaxAge;
                    if (ctx.Policy.MaxStale > TimeSpan.Zero)
                    {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_policy_max_stale, ((int)ctx.Policy.MaxStale.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo)));
                        if (adjustedMaxAge < TimeSpan.MaxValue - ctx.Policy.MaxStale)
                        {
                            adjustedMaxAge = adjustedMaxAge + ctx.Policy.MaxStale;
                        }
                        else
                        {
                            adjustedMaxAge = TimeSpan.MaxValue;
                        }

                        if (ctx.CacheAge >= adjustedMaxAge)
                            return false;
                        else
                            return true;
                    }

                }
                // not stale means "fresh enough"
                return ctx.CacheFreshnessStatus == CacheFreshnessStatus.Fresh;
            }

            /*
                This Validator should be called ONLY before submitting any response
            */
            /*
                Returns:
                - Valid     : Cache can be returned to the app subject to effective policy
                - Invalid   : A Conditional request MUST be made (unconditional request is also fine)
            */
            internal static TriState ValidateCacheBySpecialCases(HttpRequestCacheValidator  ctx) {

                /*
                   no-cache
                       If the no-cache directive does not specify a field-name, then a
                      cache MUST NOT use the response to satisfy a subsequent request
                      without successful revalidation with the origin server. This
                      allows an origin server to prevent caching even by caches that
                      have been configured to return stale responses to client requests.
                */
                if (ctx.CacheCacheControl.NoCache) {
                    if (ctx.CacheCacheControl.NoCacheHeaders == null)
                    {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_control_no_cache));
                        return TriState.Invalid;
                    }
                    /*
                        If the no-cache directive does specify one or more field-names, then a cache MAY
                        use the response to satisfy a subsequent request, subject to any other restrictions
                        on caching.
                        However, the specified field-name(s) MUST NOT be sent in the response to
                        a subsequent request without successful revalidation with the origin server.
                        This allows an origin server to prevent the re-use of certain header fields
                        in a response, while still allowing caching of the rest of the response.
                    */
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_control_no_cache_removing_some_headers));
                    for (int i = 0; i < ctx.CacheCacheControl.NoCacheHeaders.Length; ++i) {
                        ctx.CacheHeaders.Remove(ctx.CacheCacheControl.NoCacheHeaders[i]);
                    }
                }

                /*
                 must-revalidate

                    When the must-revalidate
                    directive is present in a response received by a cache, that cache
                    MUST NOT use the entry after it becomes stale to respond to a
                    subsequent request without first revalidating it with the origin
                    server. (I.e., the cache MUST do an end-to-end revalidation every
                    time, if, based solely on the origin server's Expires or max-age
                    value, the cached response is stale.)

                 proxy-revalidate
                    The proxy-revalidate directive has the same meaning as the must-
                    revalidate directive, except that it does not apply to non-shared
                    user agent caches.
                */
                if (ctx.CacheCacheControl.MustRevalidate ||
                    (!ctx.CacheEntry.IsPrivateEntry && ctx.CacheCacheControl.ProxyRevalidate))
                {
                    if (ctx.CacheFreshnessStatus != CacheFreshnessStatus.Fresh) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_control_must_revalidate));
                        return TriState.Invalid;
                    }
                }
                /*
                  When a shared cache (see section 13.7) receives a request
                  containing an Authorization field, it MUST NOT return the
                  corresponding response as a reply to any other request, unless one
                  of the following specific exceptions holds:

                  1. If the response includes the "s-maxage" cache-control
                     directive, the cache MAY use that response in replying to a
                     subsequent request. But (if the specified maximum age has
                     passed) a proxy cache MUST first revalidate it with the origin
                     server, using the request-headers from the new request to allow
                     the origin server to authenticate the new request. (This is the
                     defined behavior for s-maxage.) If the response includes "s-
                     maxage=0", the proxy MUST always revalidate it before re-using
                     it.

                  2. If the response includes the "must-revalidate" cache-control
                     directive, the cache MAY use that response in replying to a
                     subsequent request. But if the response is stale, all caches
                     MUST first revalidate it with the origin server, using the
                     request-headers from the new request to allow the origin server
                     to authenticate the new request.

                  3. If the response includes the "public" cache-control directive,
                     it MAY be returned in reply to any subsequent request.
                */
                if (ctx.Request.Headers[HttpKnownHeaderNames.Authorization] != null) {
                    if (ctx.CacheFreshnessStatus != CacheFreshnessStatus.Fresh) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_cached_auth_header));
                        return TriState.Invalid;
                    }

                    if (!ctx.CacheEntry.IsPrivateEntry && ctx.CacheCacheControl.SMaxAge == -1 && !ctx.CacheCacheControl.MustRevalidate && !ctx.CacheCacheControl.Public) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_cached_auth_header_no_control_directive));
                        return TriState.Invalid;
                    }
                }
                return TriState.Valid;
            }


            //
            // Second Time (after response) cache validation always goes through this method.
            //
            // Returns
            // - ReturnCachedResponse   = Take from cache, cache stream may be replaced and response stream is closed
            // - DoNotTakeFromCache     = Disregard the cache
            // - RemoveFromCache        = Disregard and remove cache entry
            // - CombineCachedAndServerResponse  = The combined cache+live stream has been constructed.
            //
            public static CacheValidationStatus ValidateCacheAfterResponse(HttpRequestCacheValidator ctx, HttpWebResponse resp) {

                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_after_validation));

                if ((ctx.CacheStream == Stream.Null || (int)ctx.CacheStatusCode == 0) && resp.StatusCode == HttpStatusCode.NotModified) {
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_resp_status_304));
                    return CacheValidationStatus.DoNotTakeFromCache;
                }

                if (ctx.RequestMethod == HttpMethod.Head) {
                    /*
                           The response to a HEAD request MAY be cacheable in the sense that the
                           information contained in the response MAY be used to update a
                           previously cached entity from that resource. If the new field values
                           indicate that the cached entity differs from the current entity (as
                           would be indicated by a change in Content-Length, Content-MD5, ETag
                           or Last-Modified), then the cache MUST treat the cache entry as
                           stale.
                    */
                    bool invalidate = false;

                    if (ctx.ResponseEntityLength != -1 && ctx.ResponseEntityLength != ctx.CacheEntityLength) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_head_resp_has_different_content_length));
                        invalidate = true;
                    }
                    if (resp.Headers[HttpKnownHeaderNames.ContentMD5] != ctx.CacheHeaders[HttpKnownHeaderNames.ContentMD5]) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_head_resp_has_different_content_md5));
                        invalidate = true;
                    }
                    if (resp.Headers.ETag != ctx.CacheHeaders.ETag) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_head_resp_has_different_etag));
                        invalidate = true;
                    }
                    if (resp.StatusCode != HttpStatusCode.NotModified && resp.Headers.LastModified != ctx.CacheHeaders.LastModified)
                    {
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_304_head_resp_has_different_last_modified));
                        invalidate = true;
                    }
                    if (invalidate) {
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_existing_entry_has_to_be_discarded));
                        return CacheValidationStatus.RemoveFromCache;
                    }
                }

                // If server has returned 206 partial content
                if (resp.StatusCode == HttpStatusCode.PartialContent) {
                    /*
                           A cache MUST NOT combine a 206 response with other previously cached
                           content if the ETag or Last-Modified headers do not match exactly,
                           see 13.5.4.
                    */

                    // Sometime if ETag has been used the server won't include Last-Modified, which seems to be OK
                    if (ctx.CacheHeaders.ETag != ctx.Response.Headers.ETag ||
                        (ctx.CacheHeaders.LastModified != ctx.Response.Headers.LastModified
                         && (ctx.Response.Headers.LastModified != null || ctx.Response.Headers.ETag == null)))
                    {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_206_resp_non_matching_entry));
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_existing_entry_should_be_discarded));
                        return CacheValidationStatus.RemoveFromCache;
                    }


                    // check does the live stream fit exactly into our cache tail
                    if (ctx.CacheEntry.StreamSize != ctx.ResponseRangeStart) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_206_resp_starting_position_not_adjusted));
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }

                    Common.ReplaceOrUpdateCacheHeaders(ctx, resp);
                    if (ctx.RequestRangeUser) {
                        // This happens when a response is being downloaded page by page

                        // We request combining the streams
                        // A user will see data starting CacheStreamOffset of a combined stream
                        ctx.CacheStreamOffset       = ctx.CacheEntry.StreamSize;
                        // This is a user response content length
                        ctx.CacheStreamLength       = ctx.ResponseRangeEnd - ctx.ResponseRangeStart + 1;
                        // This is a new cache stream size
                        ctx.CacheEntityLength       = ctx.ResponseEntityLength;

                        ctx.CacheStatusCode         = resp.StatusCode;
                        ctx.CacheStatusDescription  = resp.StatusDescription;
                        ctx.CacheHttpVersion        = resp.ProtocolVersion;
                    }
                    else {
                        // This happens when previous response was downloaded partly

                        ctx.CacheStreamOffset       = 0;
                        ctx.CacheStreamLength       = ctx.ResponseEntityLength;
                        ctx.CacheEntityLength       = ctx.ResponseEntityLength;

                        ctx.CacheStatusCode         = HttpStatusCode.OK;
                        ctx.CacheStatusDescription  = Common.OkDescription;
                        ctx.CacheHttpVersion        = resp.ProtocolVersion;
                        ctx.CacheHeaders.Remove(HttpKnownHeaderNames.ContentRange);

                        if (ctx.CacheStreamLength == -1)
                            {ctx.CacheHeaders.Remove(HttpKnownHeaderNames.ContentLength);}
                        else
                            {ctx.CacheHeaders[HttpKnownHeaderNames.ContentLength] = ctx.CacheStreamLength.ToString(NumberFormatInfo.InvariantInfo);}

                    }
                    // At this point the protocol should create a combined stream made up of the cached and live streams
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_combined_resp_requested));
                    return CacheValidationStatus.CombineCachedAndServerResponse;
                }

                /*
                304 Not Modified
                    The response MUST include the following header fields:

                      - Date, unless its omission is required by section 14.18.1

                   If a clockless origin server obeys these rules, and proxies and
                   clients add their own Date to any response received without one (as
                   already specified by [RFC 2068], section 14.19), caches will operate
                   correctly.

                      - ETag and/or Content-Location, if the header would have been sent
                        in a 200 response to the same request

                      - Expires, Cache-Control, and/or Vary, if the field-value might
                        differ from that sent in any previous response for the same
                        variant
                */

                if (resp.StatusCode == HttpStatusCode.NotModified) {
                    // We will return the response from cache.

                    // We try to avoid to update Cache update in case the server has
                    // sent only headers that are "safe" to ignore
                    // It's not the best way but WinInet does not work well with headers update.

                    WebHeaderCollection cc = resp.Headers;

                    string  location = null;
                    string  etag = null;

                    if ((ctx.CacheExpires != ctx.ResponseExpires) ||
                        (ctx.CacheLastModified != ctx.ResponseLastModified) ||
                        (ctx.CacheDate != ctx.ResponseDate) ||
                        (ctx.ResponseCacheControl.IsNotEmpty) ||
                        ((location=cc[HttpKnownHeaderNames.ContentLocation]) != null && location != ctx.CacheHeaders[HttpKnownHeaderNames.ContentLocation]) ||
                        ((etag=cc.ETag) != null && etag != ctx.CacheHeaders.ETag)) {
                        // Headers have to be updated
                        // Note that would allow a new E-Tag header to come in without changing the content.
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_updating_headers_on_304));
                        Common.ReplaceOrUpdateCacheHeaders(ctx, resp);
                        return CacheValidationStatus.ReturnCachedResponse;
                    }

                    //Try to not update headers if they are invariant or the same
                    int ignoredHeaders = 0;
                    if (etag != null) {
                        ++ignoredHeaders;
                    }
                    if (location != null) {
                        ++ignoredHeaders;
                    }
                    if (ctx.ResponseAge != TimeSpan.MinValue) {
                        ++ignoredHeaders;
                    }
                    if (ctx.ResponseLastModified != DateTime.MinValue) {
                        ++ignoredHeaders;
                    }
                    if (ctx.ResponseExpires != DateTime.MinValue) {
                        ++ignoredHeaders;
                    }
                    if (ctx.ResponseDate != DateTime.MinValue) {
                        ++ignoredHeaders;
                    }
                    if (cc.Via != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.Connection] != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.KeepAlive] != null) {
                        ++ignoredHeaders;
                    }
                    if (cc.ProxyAuthenticate != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.ProxyAuthorization] != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.TE] != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.TransferEncoding] != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.Trailer] != null) {
                        ++ignoredHeaders;
                    }
                    if (cc[HttpKnownHeaderNames.Upgrade] != null) {
                        ++ignoredHeaders;
                    }

                    if (resp.Headers.Count <= ignoredHeaders) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_suppressing_headers_update_on_304));
                        ctx.CacheDontUpdateHeaders = true;
                    }
                    else {
                        Common.ReplaceOrUpdateCacheHeaders(ctx, resp);
                    }
                    return CacheValidationStatus.ReturnCachedResponse;
                }

                // Any other response
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_status_code_not_304_206));
                return CacheValidationStatus.DoNotTakeFromCache;
            }

            /*
                Returns:
                - ReturnCachedResponse  : Cache may be returned to the app
                - DoNotTakeFromCache    : Cache must not be returned to the app
            */
            public static CacheValidationStatus ValidateCacheOn5XXResponse(HttpRequestCacheValidator ctx) {
                /*
                   If a cache receives a 5xx response while attempting to revalidate an
                   entry, it MAY either forward this response to the requesting client,
                   or act as if the server failed to respond. In the latter case, it MAY
                   return a previously received response unless the cached entry
                   includes the "must-revalidate" cache-control directive

                */
                // Do we have cached item?
                if (ctx.CacheStream == Stream.Null || ctx.CacheStatusCode == (HttpStatusCode)0) {
                    return CacheValidationStatus.DoNotTakeFromCache;
                }

                if (ctx.CacheEntityLength != ctx.CacheEntry.StreamSize || ctx.CacheStatusCode == HttpStatusCode.PartialContent) {
                    // Partial cache remains partial, user will not know that.
                    // This is because user either did not provide a Range Header or
                    // the user range was just forwarded to the server bypassing cache
                    return CacheValidationStatus.DoNotTakeFromCache;
                }

                if (ValidateCacheBySpecialCases(ctx) != TriState.Valid) {
                    // This response cannot be used without _successful_ revalidation
                    return CacheValidationStatus.DoNotTakeFromCache;
                }

                if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly || ctx.Policy.Level == HttpRequestCacheLevel.CacheIfAvailable || ctx.Policy.Level == HttpRequestCacheLevel.CacheOrNextCacheOnly)
                {
                    // that was a cache only request
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_sxx_resp_cache_only));
                    return CacheValidationStatus.ReturnCachedResponse;
                }

                if (ctx.Policy.Level == HttpRequestCacheLevel.Default || ctx.Policy.Level == HttpRequestCacheLevel.Revalidate)
                {
                    if (ValidateCacheByClientPolicy(ctx)) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_sxx_resp_can_be_replaced));
                        ctx.CacheHeaders.Add(HttpKnownHeaderNames.Warning, HttpRequestCacheValidator.Warning_111);
                        return CacheValidationStatus.ReturnCachedResponse;
                    }
                }
                return CacheValidationStatus.DoNotTakeFromCache;
            }


            /*
               When the cache receives a subsequent request whose Request-URI
               specifies one or more cache entries including a Vary header field,
               the cache MUST NOT use such a cache entry to construct a response to
               the new request unless all of the selecting request-headers present
               in the new request match the corresponding stored request-headers in
               the original request.

               The selecting request-headers from two requests are defined to match
               if and only if the selecting request-headers in the first request can
               be transformed to the selecting request-headers in the second request
               by adding or removing linear white space (LWS) at places where this
               is allowed by the corresponding BNF, and/or combining multiple
               message-header fields with the same field name following the rules
               about message headers in section 4.2.

               A Vary header field-value of "*" always fails to match and subsequent
               requests on that resource can only be properly interpreted by the
               origin server.
            */
            /*
                Returns:
                - Valid     : Vary header values match in both request and cache
                - Invalid   : Vary header values do not match
                - Unknown   : Vary header is not present in cache
            */
            internal static TriState ValidateCacheByVaryHeader(HttpRequestCacheValidator  ctx) {
                string[] cacheVary = ctx.CacheHeaders.GetValues(HttpKnownHeaderNames.Vary);
                if (cacheVary == null) {
                    return TriState.Unknown;
                }

                ArrayList varyValues = new ArrayList();
                HttpRequestCacheValidator.ParseHeaderValues(cacheVary,
                                                            HttpRequestCacheValidator.ParseValuesCallback,
                                                            varyValues);
                if (varyValues.Count == 0) {
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_vary_header_empty));
                    return TriState.Invalid;
                }

                if (((string)(varyValues[0]))[0] == '*') {
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_vary_header_contains_asterisks));
                    return TriState.Invalid;
                }

                if (ctx.SystemMeta == null || ctx.SystemMeta.Count == 0) {
                    // We keep there previous request headers
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_no_headers_in_metadata));
                    return TriState.Invalid;
                }

                /*
                   A Vary field value consisting of a list of field-names signals that
                   the representation selected for the response is based on a selection
                   algorithm which considers ONLY the listed request-header field values
                   in selecting the most appropriate representation. A cache MAY assume
                   that the same selection will be made for future requests with the
                   same values for the listed field names, for the duration of time for
                   which the response is fresh.
                */

                for (int i = 0; i < varyValues.Count; ++i) {

                    string[] requestValues  = ctx.Request.Headers.GetValues((string)varyValues[i]);
                    ArrayList requestFields = new ArrayList();
                    if (requestValues != null) {
                        HttpRequestCacheValidator.ParseHeaderValues(requestValues,
                                                                    HttpRequestCacheValidator.ParseValuesCallback,
                                                                    requestFields);
                    }

                    string[] cacheValues    =  ctx.SystemMeta.GetValues((string)varyValues[i]);
                    ArrayList cacheFields = new ArrayList();
                    if (cacheValues != null) {
                        HttpRequestCacheValidator.ParseHeaderValues(cacheValues,
                                                                HttpRequestCacheValidator.ParseValuesCallback,
                                                                cacheFields);
                    }

                    if (requestFields.Count != cacheFields.Count) {
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_vary_header_mismatched_count, (string)varyValues[i]));
                        return TriState.Invalid;
                    }

                    // NB: fields order is significant as per RFC.
                    for (int j = 0; j < cacheFields.Count; ++j) {
                        if (!AsciiLettersNoCaseEqual((string)cacheFields[j], (string)requestFields[j])) {
                            if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_vary_header_mismatched_field, (string)varyValues[i], (string)cacheFields[j], (string)requestFields[j]));
                            return TriState.Invalid;
                        }
                    }
                }
                if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_vary_header_match));
                // The Vary header is in cache and all headers values referenced to are equal to those in the Request.
                return TriState.Valid;
            }

            // Returns
            // - DoNotTakeFromCache = A request shall go as is and the cache will be dropped
            // - Continue           = Cache should be preserved and after-response validator should be called
            public static CacheValidationStatus TryConditionalRequest(HttpRequestCacheValidator ctx) {

                string ranges;
                TriState isPartial = CheckForRangeRequest(ctx, out ranges);

                if (isPartial == TriState.Invalid) {
                    // This is a user requested range, pass it as is
                    return CacheValidationStatus.Continue;
                }

                if(isPartial == TriState.Valid) {
                    // Not all proxy servers, support requesting a range on an FTP
                    // command, so to be safe, never try to mix the cache with a range
                    // response. Always get the whole thing fresh in the case of FTP
                    // over proxy.
                    if (ctx is FtpRequestCacheValidator)
                        return CacheValidationStatus.DoNotTakeFromCache;
                    // We only have a partial response, need to complete it
                    if (TryConditionalRangeRequest(ctx)){
                        // We can do a conditional range request
                        ctx.RequestRangeCache = true;
                        ((HttpWebRequest)ctx.Request).AddRange((int)ctx.CacheEntry.StreamSize);
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_range, ctx.Request.Headers[HttpKnownHeaderNames.Range]));
                        return CacheValidationStatus.Continue;
                    }
                    return CacheValidationStatus.DoNotTakeFromCache;
                }

                //This is not a range request
                return  ConstructConditionalRequest(ctx);
            }


            // Returns:
            // ReturnFromCache   = Take it from cache
            // DoNotTakeFromCache= Reload from server and disregard current cache
            // Continue          = Send a request that may have added a conditional header
            public static CacheValidationStatus TryResponseFromCache(HttpRequestCacheValidator ctx) {

                string ranges;
                TriState isRange = CheckForRangeRequest(ctx, out ranges);

                if (isRange == TriState.Unknown) {
                    return  CacheValidationStatus.ReturnCachedResponse;
                }

                if (isRange == TriState.Invalid) {
                    // user range request
                    long start = 0;
                    long end   = 0;
                    long total = 0;

                    if (!GetBytesRange(ranges, ref start, ref end, ref total, true)) {
                        if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_range_invalid_format, ranges));
                        return CacheValidationStatus.DoNotTakeFromCache;
                    }

                    if (start >= ctx.CacheEntry.StreamSize 
                        || end > ctx.CacheEntry.StreamSize 
                        || (end == -1 && ctx.CacheEntityLength == -1) 
                        || (end == -1 && ctx.CacheEntityLength > ctx.CacheEntry.StreamSize)
                        || (start == -1 && (end == -1 
                                            || ctx.CacheEntityLength == -1 
                                            || (ctx.CacheEntityLength - end >= ctx.CacheEntry.StreamSize))))
                    {
                        // we don't have such a range in cache
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_range_not_in_cache, ranges));
                        return  CacheValidationStatus.Continue;
                    }

                    if (start == -1) {
                        start = ctx.CacheEntityLength - end;
                    }

                    if (end <= 0) {
                        end = ctx.CacheEntry.StreamSize - 1;
                    }

                    ctx.CacheStreamOffset = start;
                    ctx.CacheStreamLength = end-start+1;
                    Construct206PartialContent(ctx, (int) start);

                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_range_in_cache, ctx.CacheHeaders[HttpKnownHeaderNames.ContentRange]));

                    return CacheValidationStatus.ReturnCachedResponse;
                }
                //
                // Here we got a partially cached response and the user wants a whole response
                //
                if (ctx.Policy.Level == HttpRequestCacheLevel.CacheOnly &&
                    ((object)ctx.Uri.Scheme == (object)Uri.UriSchemeHttp ||
                     (object)ctx.Uri.Scheme == (object)Uri.UriSchemeHttps))
                {
                    // Here we should strictly report a failure
                    // Only for HTTP and HTTPS we choose to return a partial content even user did not ask for it
                    ctx.CacheStreamOffset = 0;
                    ctx.CacheStreamLength = ctx.CacheEntry.StreamSize;
                    Construct206PartialContent(ctx, 0);

                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_partial_resp, ctx.CacheHeaders[HttpKnownHeaderNames.ContentRange]));
                    return CacheValidationStatus.ReturnCachedResponse;
                }

                if (ctx.CacheEntry.StreamSize >= Int32.MaxValue) {
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_entry_size_too_big, ctx.CacheEntry.StreamSize));
                    return CacheValidationStatus.DoNotTakeFromCache;
                }

                if (TryConditionalRangeRequest(ctx)) {
                    ctx.RequestRangeCache = true;
                    ((HttpWebRequest)ctx.Request).AddRange((int)ctx.CacheEntry.StreamSize);
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_range, ctx.Request.Headers[HttpKnownHeaderNames.Range]));
                    return CacheValidationStatus.Continue;
                }
                // This will let an unconditional request go
                return CacheValidationStatus.Continue;
            }

            /*
                Discovers the fact that cached response is a partial one.
                Returns:
                - Invalid   : It's a user range request
                - Valid     : It's a partial cached response
                - Unknown   : It's neither a range request nor the cache does have a partial response
            */
            private static TriState CheckForRangeRequest(HttpRequestCacheValidator ctx, out string ranges) {

                if ((ranges = ctx.Request.Headers[HttpKnownHeaderNames.Range]) != null) {
                    // A request already contains range.
                    // The caller will either return it from cache or pass as is to the server
                    ctx.RequestRangeUser = true;
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_range_request_range, ctx.Request.Headers[HttpKnownHeaderNames.Range]));
                    return TriState.Invalid;
                }

                if (ctx.CacheStatusCode == HttpStatusCode.PartialContent && ctx.CacheEntityLength == ctx.CacheEntry.StreamSize)
                {
                    // this is a whole resposne
                    ctx.CacheStatusCode = HttpStatusCode.OK;
                    ctx.CacheStatusDescription = Common.OkDescription;
                    return TriState.Unknown;
                }
                if (ctx.CacheEntry.IsPartialEntry || (ctx.CacheEntityLength != -1 && ctx.CacheEntityLength != ctx.CacheEntry.StreamSize) || ctx.CacheStatusCode == HttpStatusCode.PartialContent)
                {                    //The cache may contain a partial response
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_could_be_partial, ctx.CacheEntry.StreamSize, ctx.CacheEntityLength));
                    return TriState.Valid;
                }

                return TriState.Unknown;
            }

            /*
                HTTP/1.1 clients:

                - If an entity tag has been provided by the origin server, MUST
                use that entity tag in any cache-conditional request (using If-
                Match or If-None-Match).

                - If only a Last-Modified value has been provided by the origin
                server, SHOULD use that value in non-subrange cache-conditional
                requests (using If-Modified-Since).

                - If only a Last-Modified value has been provided by an HTTP/1.0
                origin server, MAY use that value in subrange cache-conditional
                requests (using If-Unmodified-Since:). The user agent SHOULD
                provide a way to disable this, in case of difficulty.

                - If both an entity tag and a Last-Modified value have been
                provided by the origin server, SHOULD use both validators in
                cache-conditional requests. This allows both HTTP/1.0 and
                HTTP/1.1 caches to respond appropriately.

            */
            /*
                Returns:
                - Continue            : Conditional request has been constructed
                - DoNotTakeFromCache  : Conditional request cannot be constructed
            */
            public static CacheValidationStatus ConstructConditionalRequest(HttpRequestCacheValidator  ctx) {

                CacheValidationStatus result = CacheValidationStatus.DoNotTakeFromCache;

                // The assumption is that a _user_ conditional request was already filtered out

                bool validator2 = false;
                string str = ctx.CacheHeaders.ETag;
                if (str != null) {
                    result = CacheValidationStatus.Continue;
                    ctx.Request.Headers[HttpKnownHeaderNames.IfNoneMatch] = str;
                    ctx.RequestIfHeader1 = HttpKnownHeaderNames.IfNoneMatch;
                    ctx.RequestValidator1 = str;
                    validator2 = true;
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_condition_if_none_match, ctx.Request.Headers[HttpKnownHeaderNames.IfNoneMatch]));
                }

                if (ctx.CacheEntry.LastModifiedUtc != DateTime.MinValue) {
                    result = CacheValidationStatus.Continue;
                    str = ctx.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture);
                    ctx.Request.Headers.ChangeInternal(HttpKnownHeaderNames.IfModifiedSince, str);
                    if (validator2) {
                        ctx.RequestIfHeader2  = HttpKnownHeaderNames.IfModifiedSince;
                        ctx.RequestValidator2 = str;
                    }
                    else {
                        ctx.RequestIfHeader1  = HttpKnownHeaderNames.IfModifiedSince;
                        ctx.RequestValidator1 = str;
                    }
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_condition_if_modified_since, ctx.Request.Headers[HttpKnownHeaderNames.IfModifiedSince]));
                }

                if(Logging.On) {
                    if (result == CacheValidationStatus.DoNotTakeFromCache) {
                        Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_cannot_construct_conditional_request));
                    }
                }
                return result;
            }


            /*
                Returns:
                - true: Conditional Partial request has been constructed
                - false: Conditional Partial request cannot be constructed
            */
            private static bool TryConditionalRangeRequest(HttpRequestCacheValidator ctx) {
                //
                // The response is partially cached (that has been checked before calling this method)
                //
                if (ctx.CacheEntry.StreamSize >= Int32.MaxValue) {
                    //This is a restriction of HttpWebRequest implementation as on 01/28/03
                    if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_entry_size_too_big, ctx.CacheEntry.StreamSize));
                    return false;
                }

                /*
                    If the entity tag given in the If-Range header matches the current
                    entity tag for the entity, then the server SHOULD provide the
                    specified sub-range of the entity using a 206 (Partial content)
                    response. If the entity tag does not match, then the server SHOULD
                    return the entire entity using a 200 (OK) response.
                */
                string str = ctx.CacheHeaders.ETag;
                if (str != null) {
                    ctx.Request.Headers[HttpKnownHeaderNames.IfRange] = str;
                    ctx.RequestIfHeader1 = HttpKnownHeaderNames.IfRange;
                    ctx.RequestValidator1 =str;
                    if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_condition_if_range, ctx.Request.Headers[HttpKnownHeaderNames.IfRange]));
                    return true;
                }

                /*
                    - If only a Last-Modified value has been provided by an HTTP/1.0
                    origin server, MAY use that value in subrange cache-conditional
                    requests (using If-Unmodified-Since:). The user agent SHOULD
                    provide a way to disable this, in case of difficulty.
                */

                if (ctx.CacheEntry.LastModifiedUtc != DateTime.MinValue)
                {
                    str = ctx.CacheEntry.LastModifiedUtc.ToString("r", CultureInfo.InvariantCulture);
                    if (ctx.CacheHttpVersion.Major == 1 && ctx.CacheHttpVersion.Minor == 0)
                    {
                        // Well If-Unmodified-Since would require an additional request in case it WAS modified
                        // A User may want to excerise this path without relying on our implementation.
                        if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_conditional_range_not_implemented_on_http_10));
                        return false;
                    /*
                        //Http == 1.0
                        ctx.Request.Headers[HttpKnownHeaderNames.IfUnmodifiedSince] = str;
                        ctx.RequestIfHeader1 = HttpKnownHeaderNames.IfUnmodifiedSince;
                        ctx.RequestValidator1 = str;
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, "Request Condition = If-Unmodified-Since:" + ctx.Request.Headers[HttpKnownHeaderNames.IfUnmodifiedSince]);
                        return true;
                    */
                    }
                    else
                    {
                        //Http > 1.0
                        ctx.Request.Headers[HttpKnownHeaderNames.IfRange] = str;
                        ctx.RequestIfHeader1 = HttpKnownHeaderNames.IfRange;
                        ctx.RequestValidator1 =str;
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_condition_if_range, ctx.Request.Headers[HttpKnownHeaderNames.IfRange]));
                        return true;
                    }
                }

                if(Logging.On)Logging.PrintWarning(Logging.RequestCache, SR.GetString(SR.net_log_cache_cannot_construct_conditional_range_request));
                //Cannot construct a conditional request
                return false;
            }

            //
            // A template for 206 response that we serve from cache on a user range request
            // It's also used for cache update.
            //
            public static void Construct206PartialContent(HttpRequestCacheValidator ctx, int rangeStart) {
                ctx.CacheStatusCode         = HttpStatusCode.PartialContent;
                ctx.CacheStatusDescription  = PartialContentDescription;
                if (ctx.CacheHttpVersion == null) {
                    ctx.CacheHttpVersion = new Version(1,1);
                }
                string ranges = "bytes " + rangeStart + '-' + (rangeStart + ctx.CacheStreamLength-1) +'/' + (ctx.CacheEntityLength <= 0?"*":ctx.CacheEntityLength.ToString(NumberFormatInfo.InvariantInfo));
                ctx.CacheHeaders[HttpKnownHeaderNames.ContentRange] = ranges;
                ctx.CacheHeaders[HttpKnownHeaderNames.ContentLength] = ctx.CacheStreamLength.ToString(NumberFormatInfo.InvariantInfo);
                ctx.CacheEntry.IsPartialEntry = true;
            }
            //
            // A template for 200 response, used by cache update
            //
            public static void Construct200ok(HttpRequestCacheValidator ctx) {
                ctx.CacheStatusCode         = HttpStatusCode.OK;
                ctx.CacheStatusDescription  = Common.OkDescription;
                if (ctx.CacheHttpVersion == null)
                    ctx.CacheHttpVersion = new Version(1,1);

                ctx.CacheHeaders.Remove(HttpKnownHeaderNames.ContentRange);

                if (ctx.CacheEntityLength == -1)
                    {ctx.CacheHeaders.Remove(HttpKnownHeaderNames.ContentLength);}
                else
                    {ctx.CacheHeaders[HttpKnownHeaderNames.ContentLength] = ctx.CacheEntityLength.ToString(NumberFormatInfo.InvariantInfo);}
                ctx.CacheEntry.IsPartialEntry = false;
            }
            //
            // Clear the request from any conditional headers and request no-cache
            //
            public static void ConstructUnconditionalRefreshRequest(HttpRequestCacheValidator ctx) {

                WebHeaderCollection cc = ctx.Request.Headers;
                cc[HttpKnownHeaderNames.CacheControl]="max-age=0";
                cc[HttpKnownHeaderNames.Pragma]="no-cache";
                if (ctx.RequestIfHeader1 != null) {
                    cc.RemoveInternal(ctx.RequestIfHeader1);
                    ctx.RequestIfHeader1 = null;
                }
                if (ctx.RequestIfHeader2 != null) {
                    cc.RemoveInternal(ctx.RequestIfHeader2);
                    ctx.RequestIfHeader2 = null;
                }

                if (ctx.RequestRangeCache) {
                    cc.RemoveInternal(HttpKnownHeaderNames.Range);
                    ctx.RequestRangeCache = false;
                }
            }

            //
            // This is called when we have decided to take from Cache or update Cache
            //
            public static void ReplaceOrUpdateCacheHeaders(HttpRequestCacheValidator ctx, HttpWebResponse resp) {
                /*
                   In other words, the set of end-to-end headers received in the
                   incoming response overrides all corresponding end-to-end headers
                   stored with the cache entry (except for stored Warning headers with
                   warn-code 1xx, which are deleted even if not overridden).

                    This rule does not allow an origin server to use
                    a 304 (Not Modified) or a 206 (Partial Content) response to
                    entirely delete a header that it had provided with a previous
                    response.

               */

                if (ctx.CacheHeaders == null || (resp.StatusCode != HttpStatusCode.NotModified && resp.StatusCode != HttpStatusCode.PartialContent))
                {
                    // existing context is dropped
                    ctx.CacheHeaders = new WebHeaderCollection();
                }

                // Here we preserve Request headers that are present in the response Vary header
                string[] respVary = resp.Headers.GetValues(HttpKnownHeaderNames.Vary);
                if (respVary != null) {
                    ArrayList varyValues = new ArrayList();
                    HttpRequestCacheValidator.ParseHeaderValues(respVary,
                                                                HttpRequestCacheValidator.ParseValuesCallback,
                                                                varyValues);
                    if (varyValues.Count != 0 && ((string)(varyValues[0]))[0] != '*') {
                        // we got some request headers to save
                        if(Logging.On)Logging.PrintInfo(Logging.RequestCache, SR.GetString(SR.net_log_cache_saving_request_headers, resp.Headers[HttpKnownHeaderNames.Vary]));
                        if (ctx.SystemMeta == null) {
                            ctx.SystemMeta = new NameValueCollection(varyValues.Count+1, CaseInsensitiveAscii.StaticInstance);
                        }
                        for (int i = 0; i < varyValues.Count; ++i) {
                            string headerValue  = ctx.Request.Headers[(string)varyValues[i]];
                            ctx.SystemMeta[(string)varyValues[i]] = headerValue;
                        }
                    }
                }


                /*
                      - Hop-by-hop headers, which are meaningful only for a single
                        transport-level connection, and are not stored by caches or
                        forwarded by proxies.

                   The following HTTP/1.1 headers are hop-by-hop headers:

                      - Connection
                      - Keep-Alive
                      - Proxy-Authenticate
                      - Proxy-Authorization
                      - TE
                      - Trailers
                      - Transfer-Encoding
                      - Upgrade
                */


                // We add or Replace headers from the live response
                for (int i = 0; i < ctx.Response.Headers.Count; ++i) {
                    string key = ctx.Response.Headers.GetKey(i);
                    if (AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.Connection) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.KeepAlive) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.ProxyAuthenticate) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.ProxyAuthorization) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.TE) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.TransferEncoding) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.Trailer) ||
                        AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.Upgrade))
                    {
                        continue;

                    }
                    if (resp.StatusCode == HttpStatusCode.NotModified && AsciiLettersNoCaseEqual(key, HttpKnownHeaderNames.ContentLength)) {
                         continue;
                    }
                    ctx.CacheHeaders.ChangeInternal(key, ctx.Response.Headers[i]);
                }
            }
            //
            //
            //
            private static bool AsciiLettersNoCaseEqual(string s1, string s2) {
                if (s1.Length != s2.Length) {
                    return false;
                }
                for (int i = 0; i < s1.Length; ++i) {
                    if ((s1[i]|0x20) != (s2[i]|0x20)) {
                        return false;
                    }
                }
                return true;
            }
            //
            //
            //
            internal unsafe static bool UnsafeAsciiLettersNoCaseEqual(char* s1, int start, int length, string s2) {
                if (length-start < s2.Length) {
                    return false;
                }
                for (int i = 0; i < s2.Length; ++i) {
                    if ((s1[start+i]|0x20) != (s2[i]|0x20)) {
                        return false;
                    }
                }
                return true;
            }

            //
            // Parses the string on "bytes = start - end" or "bytes start-end/xxx"
            //
            // Returns
            //   true      = take start/end for range
            //   false     = parsing error
            public static bool GetBytesRange(string ranges, ref long start, ref long end, ref long total, bool isRequest) {

                ranges = ranges.ToLower(CultureInfo.InvariantCulture);

                int idx = 0;
                while (idx < ranges.Length && ranges[idx] == ' ') {
                    ++idx;
                }

                idx+=5;
                // The "ranges" string is already in lowercase
                if( idx >= ranges.Length || ranges[idx-5] != 'b' || ranges[idx-4] != 'y' || ranges[idx-3] != 't' || ranges[idx-2] != 'e' || ranges[idx-1] != 's')
                {
                    if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_only_byte_range_implemented));
                    return false;
                }

                if (isRequest) {
                    while (idx < ranges.Length && ranges[idx] == ' ') {
                        ++idx;
                    }
                    if (ranges[idx] != '=') {
                        return false;
                    }
                }
                else {
                    if (ranges[idx] != ' ') {
                        return false;
                    }
                }

                char ch = (char)0;
                while (++idx < ranges.Length && (ch=ranges[idx]) == ' ') {
                    ;
                }

                start = -1;
                if (ch != '-') {
                    // parsing start
                    if (idx < ranges.Length && ch >= '0' && ch <= '9') {
                        start = ch-'0';
                        while(++idx < ranges.Length && (ch = ranges[idx]) >= '0' && ch <= '9') {
                            start = start*10 + (ch-'0');
                        }
                    }

                    while (idx < ranges.Length && ch == ' ') {ch = ranges[++idx];}
                    if (ch != '-') {return false;}
                }

                // parsing end
                while (idx < ranges.Length && (ch = ranges[++idx]) == ' ') {
                    ;
                }

                end = -1;
                if (idx < ranges.Length && ch >= '0' && ch <= '9') {
                    end = ch-'0';
                    while(++idx < ranges.Length && (ch = ranges[idx]) >= '0' && ch <= '9') {
                        end = end*10 + (ch-'0');
                    }
                }
                if (isRequest) {
                    while (idx < ranges.Length) {
                        if (ranges[idx++] != ' ') {
                            if(Logging.On)Logging.PrintError(Logging.RequestCache, SR.GetString(SR.net_log_cache_multiple_complex_range_not_implemented));
                            return false;
                        }
                    }
                }
                else {
                    // parsing total
                    while (idx < ranges.Length && (ch = ranges[idx]) == ' ') {
                        ++idx;
                    }

                    if (ch != '/') {
                        return false;
                    }
                    while (++idx < ranges.Length && (ch=ranges[idx]) == ' ') {
                        ;
                    }

                    total = -1;
                    if (ch != '*') {
                        if (idx < ranges.Length && ch >= '0' && ch <= '9') {
                            total = ch-'0';
                            while (++idx < ranges.Length && (ch = ranges[idx]) >= '0' && ch <= '9') {
                                total = total*10 + (ch-'0');
                            }
                        }
                    }
                }

                if (!isRequest && (start == -1 || end == -1)) {
                    return false;
                }
                return true;
            }
        }
    }

}
