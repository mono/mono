/*++
Copyright (c) Microsoft Corporation

Module Name:

    RequestCacheManager.cs

Abstract:
    The class implements the app domain wide configuration
    for request cache aware clients

Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

    Jan 25 2004 - Changed the visibility of the class from public to internal, compressed unused logic.

--*/
namespace System.Net.Cache {
using System;
using System.Collections;
using System.Net.Configuration;
using System.Configuration;


    /// <summary>  Specifies app domain-wide default settings for request caching </summary>
    internal sealed class RequestCacheManager {
        private RequestCacheManager() {}

        private static volatile RequestCachingSectionInternal s_CacheConfigSettings;

        private static readonly RequestCacheBinding s_BypassCacheBinding = new RequestCacheBinding (null, null, new RequestCachePolicy(RequestCacheLevel.BypassCache));

        private static volatile RequestCacheBinding s_DefaultGlobalBinding;
        private static volatile RequestCacheBinding s_DefaultHttpBinding;
        private static volatile RequestCacheBinding s_DefaultFtpBinding;

        //
        // ATN: the caller MUST use uri.Scheme as a parameter, otheriwse cannot do schme ref comparision
        //
        internal static RequestCacheBinding GetBinding(string internedScheme)
        {
            if (internedScheme == null)
                throw new ArgumentNullException("uriScheme");

            if (s_CacheConfigSettings == null)
                LoadConfigSettings();

            if(s_CacheConfigSettings.DisableAllCaching)
                return s_BypassCacheBinding;

            if (internedScheme.Length == 0)
                return s_DefaultGlobalBinding;

            if ((object)internedScheme == (object)Uri.UriSchemeHttp || (object)internedScheme == (object)Uri.UriSchemeHttps)
                return s_DefaultHttpBinding;

            if ((object)internedScheme == (object)Uri.UriSchemeFtp)
                return s_DefaultFtpBinding;

            return s_BypassCacheBinding;
        }

        internal static bool IsCachingEnabled
        {
            get
            {
                if (s_CacheConfigSettings == null)
                    LoadConfigSettings();

                return !s_CacheConfigSettings.DisableAllCaching;
            }
        }

        //
        internal static void SetBinding(string uriScheme, RequestCacheBinding binding)
        {
            if (uriScheme == null)
                throw new ArgumentNullException("uriScheme");

            if (s_CacheConfigSettings == null)
                LoadConfigSettings();

            if(s_CacheConfigSettings.DisableAllCaching)
                return;

            if (uriScheme.Length == 0)
                s_DefaultGlobalBinding = binding;
            else if (uriScheme == Uri.UriSchemeHttp || uriScheme == Uri.UriSchemeHttps)
                s_DefaultHttpBinding = binding;
            else if (uriScheme == Uri.UriSchemeFtp)
                s_DefaultFtpBinding = binding;
        }
        //
        private static void LoadConfigSettings()
        {
            // Any concurent access shall go here and block until we've grabbed the config settings
            lock (s_BypassCacheBinding)
            {
                if (s_CacheConfigSettings == null)
                {
                    RequestCachingSectionInternal settings = RequestCachingSectionInternal.GetSection();

                    s_DefaultGlobalBinding = new RequestCacheBinding (settings.DefaultCache, settings.DefaultHttpValidator, settings.DefaultCachePolicy);
                    s_DefaultHttpBinding = new RequestCacheBinding (settings.DefaultCache, settings.DefaultHttpValidator, settings.DefaultHttpCachePolicy);
                    s_DefaultFtpBinding = new RequestCacheBinding (settings.DefaultCache, settings.DefaultFtpValidator, settings.DefaultFtpCachePolicy);

                    s_CacheConfigSettings = settings;
                }
            }
        }
    }

    //
    //
    internal class RequestCacheBinding  {
        private RequestCache          m_RequestCache;
        private RequestCacheValidator m_CacheValidator;
        private RequestCachePolicy    m_Policy;


        internal RequestCacheBinding (RequestCache requestCache, RequestCacheValidator cacheValidator, RequestCachePolicy  policy) {
            m_RequestCache = requestCache;
            m_CacheValidator = cacheValidator;
            m_Policy = policy;
        }

        internal RequestCache Cache {
            get {return m_RequestCache;}
        }

        internal RequestCacheValidator Validator {
            get {return m_CacheValidator;}
        }

        internal RequestCachePolicy Policy {
            get {return m_Policy;}
        }

    }
}
