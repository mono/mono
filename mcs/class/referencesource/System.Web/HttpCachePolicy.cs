//------------------------------------------------------------------------------
// <copyright file="HttpCachePolicy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Cache Policy class
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Security.Cryptography;
    using System.Web.Util;
    using Debug = System.Web.Util.Debug;

    //
    // Public constants for cache-control
    //


    /// <devdoc>
    ///    <para>
    ///       Provides enumeration values for all cache-control header settings.
    ///    </para>
    /// </devdoc>
    public enum HttpCacheability {

        /// <devdoc>
        ///    <para>
        ///       Indicates that
        ///       without a field name, a cache must force successful revalidation with the
        ///       origin server before satisfying the request. With a field name, the cache may
        ///       use the response to satisfy a subsequent request.
        ///    </para>
        /// </devdoc>
        NoCache = 1,

        /// <devdoc>
        ///    <para>
        ///       Default value. Specifies that the response is cachable only on the client,
        ///       not by shared caches.
        ///    </para>
        /// </devdoc>
        Private,

        /// <devdoc>
        ///    <para>
        ///       Specifies that the response should only be cached at the server.
        ///       Clients receive headers equivalent to a NoCache directive.
        ///    </para>
        /// </devdoc>
        Server,

        ServerAndNoCache = Server,

        /// <devdoc>
        ///    <para>
        ///       Specifies that the response is cachable by clients and shared caches.
        ///    </para>
        /// </devdoc>
        Public,

        ServerAndPrivate,
    }
    
    enum HttpCacheabilityLimits {
        MinValue = HttpCacheability.NoCache,
        MaxValue = HttpCacheability.ServerAndPrivate, 
        None = MaxValue + 1,          
    }
    

    /// <devdoc>
    ///    <para>
    ///       This class is a light abstraction over the Cache-Control: revalidation
    ///       directives.
    ///    </para>
    /// </devdoc>
    public enum HttpCacheRevalidation {

        /// <devdoc>
        ///    <para>
        ///       Indicates that Cache-Control: must-revalidate should be sent.
        ///    </para>
        /// </devdoc>
        AllCaches = 1,

        /// <devdoc>
        ///    <para>
        ///       Indicates that Cache-Control: proxy-revalidate should be sent.
        ///    </para>
        /// </devdoc>
        ProxyCaches = 2,

        /// <devdoc>
        ///    <para>
        ///       Default value. Indicates that no property has been set. If this is set, no
        ///       cache revalitation directive is sent.
        ///    </para>
        /// </devdoc>
        None = 3,
    }
    
    enum HttpCacheRevalidationLimits {
        MinValue = HttpCacheRevalidation.AllCaches,
        MaxValue = HttpCacheRevalidation.None
    }
    
    

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public enum HttpValidationStatus {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Invalid = 1,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IgnoreThisRequest = 2,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Valid = 3
    }


    /// <devdoc>
    ///    <para>Called back when the handler wants validation on a cache
    ///       item before it's served from the cache. If any handler invalidates
    ///       the item, the item is evicted from the cache and the request is handled as
    ///       if a cache miss were generated.</para>
    /// </devdoc>
    public delegate void HttpCacheValidateHandler(
            HttpContext context, Object data, ref HttpValidationStatus validationStatus);

    sealed class ValidationCallbackInfo {
        internal readonly HttpCacheValidateHandler  handler;
        internal readonly Object                    data;

        internal ValidationCallbackInfo(HttpCacheValidateHandler handler, Object data) {
            this.handler = handler;
            this.data = data;
        }
    }

    [Serializable]
    sealed class HttpCachePolicySettings {
        /* internal access */
        internal readonly bool                       _isModified;

        [NonSerialized]
        internal          ValidationCallbackInfo[]   _validationCallbackInfo;
        private           string[]                   _validationCallbackInfoForSerialization;

        internal readonly HttpResponseHeader         _headerCacheControl;  
        internal readonly HttpResponseHeader         _headerPragma;        
        internal readonly HttpResponseHeader         _headerExpires;       
        internal readonly HttpResponseHeader         _headerLastModified;
        internal readonly HttpResponseHeader         _headerEtag;          
        internal readonly HttpResponseHeader         _headerVaryBy;        

        /* internal access */
        internal readonly bool                       _hasSetCookieHeader; 
        internal readonly bool                       _noServerCaching;                 
        internal readonly String                     _cacheExtension;                  
        internal readonly bool                       _noTransforms;                    
        internal readonly bool                       _ignoreRangeRequests;
        internal readonly String[]                   _varyByContentEncodings;
        internal readonly String[]                   _varyByHeaderValues;              
        internal readonly String[]                   _varyByParamValues;               
        internal readonly string                     _varyByCustom;                    
        internal readonly HttpCacheability           _cacheability;                    
        internal readonly bool                       _noStore;                         
        internal readonly String[]                   _privateFields;                   
        internal readonly String[]                   _noCacheFields;                   
        internal readonly DateTime                   _utcExpires;                         
        internal readonly bool                       _isExpiresSet;                    
        internal readonly TimeSpan                   _maxAge;                          
        internal readonly bool                       _isMaxAgeSet;                     
        internal readonly TimeSpan                   _proxyMaxAge;                     
        internal readonly bool                       _isProxyMaxAgeSet;                
        internal readonly int                        _slidingExpiration;               
        internal readonly TimeSpan                   _slidingDelta;
        internal readonly DateTime                   _utcTimestampCreated;
        internal readonly int                        _validUntilExpires;               
        internal readonly int                        _allowInHistory;
        internal readonly HttpCacheRevalidation      _revalidation;                    
        internal readonly DateTime                   _utcLastModified;                    
        internal readonly bool                       _isLastModifiedSet;               
        internal readonly String                     _etag;                            
        internal readonly bool                       _generateLastModifiedFromFiles;   
        internal readonly bool                       _generateEtagFromFiles;           
        internal readonly int                        _omitVaryStar;

        internal readonly bool                       _hasUserProvidedDependencies;

        internal HttpCachePolicySettings(
                bool                        isModified,
                ValidationCallbackInfo[]    validationCallbackInfo,
                bool                        hasSetCookieHeader,
                bool                        noServerCaching,    
                String                      cacheExtension,     
                bool                        noTransforms,       
                bool                        ignoreRangeRequests,
                String[]                    varyByContentEncodings,
                String[]                    varyByHeaderValues, 
                String[]                    varyByParamValues,  
                string                      varyByCustom,       
                HttpCacheability            cacheability,
                bool                        noStore,                
                String[]                    privateFields,          
                String[]                    noCacheFields,          
                DateTime                    utcExpires,                
                bool                        isExpiresSet,           
                TimeSpan                    maxAge,                 
                bool                        isMaxAgeSet,            
                TimeSpan                    proxyMaxAge,            
                bool                        isProxyMaxAgeSet,       
                int                         slidingExpiration,      
                TimeSpan                    slidingDelta,
                DateTime                    utcTimestampCreated,
                int                         validUntilExpires,      
                int                         allowInHistory,
                HttpCacheRevalidation       revalidation,
                DateTime                    utcLastModified,                  
                bool                        isLastModifiedSet,             
                String                      etag,                          
                bool                        generateLastModifiedFromFiles, 
                bool                        generateEtagFromFiles,
                int                         omitVaryStar,
                HttpResponseHeader          headerCacheControl,    
                HttpResponseHeader          headerPragma,          
                HttpResponseHeader          headerExpires,         
                HttpResponseHeader          headerLastModified,    
                HttpResponseHeader          headerEtag,            
                HttpResponseHeader          headerVaryBy,
                bool                        hasUserProvidedDependencies) {

            _isModified                     = isModified                        ;
            _validationCallbackInfo         = validationCallbackInfo            ;

            _hasSetCookieHeader             = hasSetCookieHeader                ;
            _noServerCaching                = noServerCaching                   ;
            _cacheExtension                 = cacheExtension                    ;
            _noTransforms                   = noTransforms                      ;
            _ignoreRangeRequests            = ignoreRangeRequests               ;
            _varyByContentEncodings         = varyByContentEncodings            ;
            _varyByHeaderValues             = varyByHeaderValues                ;
            _varyByParamValues              = varyByParamValues                 ;
            _varyByCustom                   = varyByCustom                      ;
            _cacheability                   = cacheability                      ;
            _noStore                        = noStore                           ;
            _privateFields                  = privateFields                     ;
            _noCacheFields                  = noCacheFields                     ;
            _utcExpires                     = utcExpires                        ;
            _isExpiresSet                   = isExpiresSet                      ;
            _maxAge                         = maxAge                            ;
            _isMaxAgeSet                    = isMaxAgeSet                       ;
            _proxyMaxAge                    = proxyMaxAge                       ;
            _isProxyMaxAgeSet               = isProxyMaxAgeSet                  ;
            _slidingExpiration              = slidingExpiration                 ;
            _slidingDelta                   = slidingDelta                      ;
            _utcTimestampCreated            = utcTimestampCreated               ;
            _validUntilExpires              = validUntilExpires                 ;
            _allowInHistory                 = allowInHistory                    ;
            _revalidation                   = revalidation                      ;
            _utcLastModified                = utcLastModified                   ;
            _isLastModifiedSet              = isLastModifiedSet                 ;
            _etag                           = etag                              ;
            _generateLastModifiedFromFiles  = generateLastModifiedFromFiles     ;
            _generateEtagFromFiles          = generateEtagFromFiles             ;
            _omitVaryStar                   = omitVaryStar                      ;

            _headerCacheControl             = headerCacheControl                ;
            _headerPragma                   = headerPragma                      ;
            _headerExpires                  = headerExpires                     ;
            _headerLastModified             = headerLastModified                ;
            _headerEtag                     = headerEtag                        ;
            _headerVaryBy                   = headerVaryBy                      ;
            _hasUserProvidedDependencies    = hasUserProvidedDependencies       ;

        }

        [OnSerializing()]
        private void OnSerializingMethod(StreamingContext context) {
            if (_validationCallbackInfo == null)
                return;

            // create a string representation of each callback
            // note that ValidationCallbackInfo.data is assumed to be null
            String[] callbackInfos = new String[_validationCallbackInfo.Length * 2];
            for (int i = 0; i < _validationCallbackInfo.Length; i++) {
                Debug.Assert(_validationCallbackInfo[i].data == null, "_validationCallbackInfo[i].data == null");

                HttpCacheValidateHandler handler = _validationCallbackInfo[i].handler;
                string targetTypeName = System.Web.UI.Util.GetAssemblyQualifiedTypeName(handler.Method.ReflectedType);
                string methodName = handler.Method.Name;
                callbackInfos[2 * i] = targetTypeName;
                callbackInfos[2 * i + 1] = methodName;
            }

            _validationCallbackInfoForSerialization = callbackInfos;
        }

        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context) {
            if (_validationCallbackInfoForSerialization == null)
                return;

            // re-create each ValidationCallbackInfo from its string representation
            ValidationCallbackInfo[] callbackInfos = new ValidationCallbackInfo[_validationCallbackInfoForSerialization.Length / 2];
            for (int i = 0; i < _validationCallbackInfoForSerialization.Length; i += 2) {
                string targetTypeName = _validationCallbackInfoForSerialization[i];
                string methodName = _validationCallbackInfoForSerialization[i+1];
                Type target = null;
                if (!String.IsNullOrEmpty(targetTypeName)) {
                    target = BuildManager.GetType(targetTypeName, true /*throwOnFail*/, false /*ignoreCase*/);
                }
                if (target == null) {
                    throw new SerializationException(SR.GetString(SR.Type_cannot_be_resolved, targetTypeName));
                }
                HttpCacheValidateHandler handler = (HttpCacheValidateHandler) Delegate.CreateDelegate(typeof(HttpCacheValidateHandler), target, methodName);
                callbackInfos[i / 2] = new ValidationCallbackInfo(handler, null);
            }
            _validationCallbackInfo = callbackInfos;
        }   

        internal bool                       IsModified              {get {return _isModified                ;}}
        internal ValidationCallbackInfo[]   ValidationCallbackInfo  {get {return _validationCallbackInfo    ;}}   

        internal HttpResponseHeader         HeaderCacheControl      {get {return _headerCacheControl        ;}}   
        internal HttpResponseHeader         HeaderPragma            {get {return _headerPragma              ;}}   
        internal HttpResponseHeader         HeaderExpires           {get {return _headerExpires             ;}}   
        internal HttpResponseHeader         HeaderLastModified      {get {return _headerLastModified        ;}}   
        internal HttpResponseHeader         HeaderEtag              {get {return _headerEtag                ;}}   
        internal HttpResponseHeader         HeaderVaryBy            {get {return _headerVaryBy              ;}}   

        internal bool                       hasSetCookieHeader      {get {return _hasSetCookieHeader        ;}}
        internal bool                       NoServerCaching         {get {return _noServerCaching           ;}}   
        internal String                     CacheExtension          {get {return _cacheExtension            ;}}   
        internal bool                       NoTransforms            {get {return _noTransforms              ;}}   
        internal bool                       IgnoreRangeRequests     {get {return _ignoreRangeRequests       ;}}
        internal String[]                   VaryByContentEncodings  {get {
                    return (_varyByContentEncodings == null) ? null : (string[]) _varyByContentEncodings.Clone()    ;}} 
        internal String[]                   VaryByHeaders           {get {
                    return (_varyByHeaderValues == null) ? null : (string[]) _varyByHeaderValues.Clone()    ;}} 

        internal String[]                   VaryByParams            {get {
                    return (_varyByParamValues == null) ? null : (string[]) _varyByParamValues.Clone()      ;}} 

        internal bool                       IgnoreParams            {get {
                    return _varyByParamValues != null && _varyByParamValues[0].Length == 0;}}

        internal HttpCacheability           CacheabilityInternal    {get { return _cacheability;}}

        internal bool                       NoStore                 {get {return _noStore                   ;}}

        internal String[]                   PrivateFields           {get {
                    return (_privateFields == null) ? null : (string[]) _privateFields.Clone()              ;}}    

        internal String[]                   NoCacheFields           {get {
                return (_noCacheFields == null) ? null : (string[]) _noCacheFields.Clone()                  ;}}    

        internal DateTime                   UtcExpires              {get {return _utcExpires                ;}}     
        internal bool                       IsExpiresSet            {get {return _isExpiresSet              ;}}     
        internal TimeSpan                   MaxAge                  {get {return _maxAge                    ;}}     
        internal bool                       IsMaxAgeSet             {get {return _isMaxAgeSet               ;}}     
        internal TimeSpan                   ProxyMaxAge             {get {return _proxyMaxAge               ;}}     
        internal bool                       IsProxyMaxAgeSet        {get {return _isProxyMaxAgeSet          ;}}     

        internal int                        SlidingExpirationInternal {get {return _slidingExpiration       ;}}
        internal bool                       SlidingExpiration         {get {return _slidingExpiration == 1  ;}}

        internal TimeSpan                   SlidingDelta            {get {return _slidingDelta              ;}}
        internal DateTime                   UtcTimestampCreated     {get {return _utcTimestampCreated       ;}}

        internal int                        ValidUntilExpiresInternal {get {return _validUntilExpires       ;}}
        internal bool                       ValidUntilExpires       {get {
                return     _validUntilExpires == 1 
                        && !SlidingExpiration
                        && !GenerateLastModifiedFromFiles 
                        && !GenerateEtagFromFiles
                        && ValidationCallbackInfo == null;}}

        internal int                        AllowInHistoryInternal  {get {return _allowInHistory            ;}}

        internal HttpCacheRevalidation      Revalidation            {get {return _revalidation              ;}}       
        internal DateTime                   UtcLastModified         {get {return _utcLastModified           ;}}       
        internal bool                       IsLastModifiedSet       {get {return _isLastModifiedSet         ;}}       
        internal String                     ETag                    {get {return _etag                      ;}}       
        internal bool                       GenerateLastModifiedFromFiles {get {return _generateLastModifiedFromFiles;}}    
        internal bool                       GenerateEtagFromFiles         {get {return _generateEtagFromFiles        ;}}    

        internal string                     VaryByCustom            {get {return _varyByCustom              ;}}

        internal bool                       HasUserProvidedDependencies {get {return _hasUserProvidedDependencies; }}

        internal bool IsValidationCallbackSerializable() {
            if (_validationCallbackInfo != null) {
                foreach(ValidationCallbackInfo info in _validationCallbackInfo) {
                    if (info.data != null
                        || !info.handler.Method.IsStatic) {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool HasValidationPolicy() {
            return      ValidUntilExpires
                   ||   GenerateLastModifiedFromFiles  
                   ||   GenerateEtagFromFiles          
                   ||   ValidationCallbackInfo != null;
        }

        internal int                       OmitVaryStarInternal {get {return _omitVaryStar;}}
    }



    /// <devdoc>
    ///    <para>Contains methods for controlling the ASP.NET output cache.</para>
    /// </devdoc>
    public sealed class HttpCachePolicy {
        static TimeSpan s_oneYear = new TimeSpan(TimeSpan.TicksPerDay * 365);
        static HttpResponseHeader   s_headerPragmaNoCache;
        static HttpResponseHeader   s_headerExpiresMinus1;

        bool                    _isModified;
        bool                    _hasSetCookieHeader;
        bool                    _noServerCaching;
        String                  _cacheExtension;
        bool                    _noTransforms;
        bool                    _ignoreRangeRequests;
        HttpCacheVaryByContentEncodings _varyByContentEncodings;
        HttpCacheVaryByHeaders  _varyByHeaders;
        HttpCacheVaryByParams   _varyByParams;
        string                  _varyByCustom;

        HttpCacheability        _cacheability;
        bool                    _noStore;       
        HttpDictionary          _privateFields; 
        HttpDictionary          _noCacheFields; 

        DateTime                _utcExpires;           
        bool                    _isExpiresSet;      
        TimeSpan                _maxAge;            
        bool                    _isMaxAgeSet;       
        TimeSpan                _proxyMaxAge;       
        bool                    _isProxyMaxAgeSet;  
        int                     _slidingExpiration; 
        DateTime                _utcTimestampCreated;    
        TimeSpan                _slidingDelta; 
        DateTime                _utcTimestampRequest;    
        int                     _validUntilExpires; 
        int                     _allowInHistory;

        HttpCacheRevalidation   _revalidation;
        DateTime                _utcLastModified;      
        bool                    _isLastModifiedSet; 
        String                  _etag;              

        bool                    _generateLastModifiedFromFiles; 
        bool                    _generateEtagFromFiles;         
        int                     _omitVaryStar;

        ArrayList               _validationCallbackInfo;


        bool                    _useCachedHeaders;
        HttpResponseHeader      _headerCacheControl; 
        HttpResponseHeader      _headerPragma;       
        HttpResponseHeader      _headerExpires;      
        HttpResponseHeader      _headerLastModified; 
        HttpResponseHeader      _headerEtag;         
        HttpResponseHeader      _headerVaryBy;       

        bool                    _noMaxAgeInCacheControl;

        bool                    _hasUserProvidedDependencies;


        internal HttpCachePolicy() {
            _varyByContentEncodings = new HttpCacheVaryByContentEncodings();
            _varyByHeaders = new HttpCacheVaryByHeaders();
            _varyByParams = new HttpCacheVaryByParams();
            Reset();
        }

        /*
         * Restore original values
         */
        internal void Reset() {
            _varyByContentEncodings.Reset();
            _varyByHeaders.Reset();
            _varyByParams.Reset();

            _isModified = false;
            _hasSetCookieHeader = false;
            _noServerCaching = false;
            _cacheExtension = null;
            _noTransforms = false;
            _ignoreRangeRequests = false;
            _varyByCustom = null;
            _cacheability = (HttpCacheability) (int) HttpCacheabilityLimits.None;
            _noStore = false;
            _privateFields = null;
            _noCacheFields = null;
            _utcExpires = DateTime.MinValue;
            _isExpiresSet = false;
            _maxAge = TimeSpan.Zero;
            _isMaxAgeSet = false;
            _proxyMaxAge = TimeSpan.Zero;
            _isProxyMaxAgeSet = false;
            _slidingExpiration = -1;
            _slidingDelta = TimeSpan.Zero;
            _utcTimestampCreated = DateTime.MinValue;
            _utcTimestampRequest = DateTime.MinValue;
            _validUntilExpires = -1;
            _allowInHistory = -1;
            _revalidation = HttpCacheRevalidation.None;
            _utcLastModified = DateTime.MinValue;
            _isLastModifiedSet = false;
            _etag = null;

            _generateLastModifiedFromFiles = false; 
            _generateEtagFromFiles = false;         
            _validationCallbackInfo = null;       
        
            _useCachedHeaders = false;
            _headerCacheControl = null;
            _headerPragma = null;        
            _headerExpires = null;       
            _headerLastModified = null;  
            _headerEtag = null;          
            _headerVaryBy = null;       

            _noMaxAgeInCacheControl = false;

            _hasUserProvidedDependencies = false;

            _omitVaryStar = -1;
        }

        /*
         * Reset based on a cached response. Includes data needed to generate
         * header for a cached response.
         */
        internal void ResetFromHttpCachePolicySettings(
                HttpCachePolicySettings settings,
                DateTime                utcTimestampRequest) {

            int i, n;
            string[] fields;

            _utcTimestampRequest = utcTimestampRequest;

            _varyByContentEncodings.SetContentEncodings(settings.VaryByContentEncodings);
            _varyByHeaders.SetHeaders(settings.VaryByHeaders);                          
            _varyByParams.SetParams(settings.VaryByParams);

            _isModified                       = settings.IsModified;                    
            _hasSetCookieHeader               = settings.hasSetCookieHeader;
            _noServerCaching                  = settings.NoServerCaching;               
            _cacheExtension                   = settings.CacheExtension;                
            _noTransforms                     = settings.NoTransforms;                  
            _ignoreRangeRequests              = settings.IgnoreRangeRequests;
            _varyByCustom                     = settings.VaryByCustom;
            _cacheability                     = settings.CacheabilityInternal;                  
            _noStore                          = settings.NoStore;
            _utcExpires                       = settings.UtcExpires;                       
            _isExpiresSet                     = settings.IsExpiresSet;                  
            _maxAge                           = settings.MaxAge;                        
            _isMaxAgeSet                      = settings.IsMaxAgeSet;                   
            _proxyMaxAge                      = settings.ProxyMaxAge;                   
            _isProxyMaxAgeSet                 = settings.IsProxyMaxAgeSet;              
            _slidingExpiration                = settings.SlidingExpirationInternal;             
            _slidingDelta                     = settings.SlidingDelta;
            _utcTimestampCreated              = settings.UtcTimestampCreated;
            _validUntilExpires                = settings.ValidUntilExpiresInternal;
            _allowInHistory                   = settings.AllowInHistoryInternal;
            _revalidation                     = settings.Revalidation;                  
            _utcLastModified                  = settings.UtcLastModified;                  
            _isLastModifiedSet                = settings.IsLastModifiedSet;             
            _etag                             = settings.ETag;                          
            _generateLastModifiedFromFiles    = settings.GenerateLastModifiedFromFiles; 
            _generateEtagFromFiles            = settings.GenerateEtagFromFiles;         
            _omitVaryStar                     = settings.OmitVaryStarInternal;
            _hasUserProvidedDependencies      = settings.HasUserProvidedDependencies;

            _useCachedHeaders = true;
            _headerCacheControl = settings.HeaderCacheControl;
            _headerPragma = settings.HeaderPragma;        
            _headerExpires = settings.HeaderExpires;       
            _headerLastModified = settings.HeaderLastModified;  
            _headerEtag = settings.HeaderEtag;          
            _headerVaryBy = settings.HeaderVaryBy;        

            _noMaxAgeInCacheControl = false;

            fields = settings.PrivateFields;
            if (fields != null) {
                _privateFields = new HttpDictionary();
                for (i = 0, n = fields.Length; i < n; i++) {
                    _privateFields.SetValue(fields[i], fields[i]);
                }
            }

            fields = settings.NoCacheFields;
            if (fields != null) {
                _noCacheFields = new HttpDictionary();
                for (i = 0, n = fields.Length; i < n; i++) {
                    _noCacheFields.SetValue(fields[i], fields[i]);
                }
            }

            if (settings.ValidationCallbackInfo != null) {
                _validationCallbackInfo = new ArrayList();
                for (i = 0, n = settings.ValidationCallbackInfo.Length; i < n; i++) {
                    _validationCallbackInfo.Add(new ValidationCallbackInfo(
                            settings.ValidationCallbackInfo[i].handler,
                            settings.ValidationCallbackInfo[i].data));
                }
            }
        }

        /// <summary>
        /// Return true if the CachePolicy has been modified
        /// </summary>
        /// <returns></returns>
        public bool IsModified() {
            return _isModified || _varyByContentEncodings.IsModified() || _varyByHeaders.IsModified() || _varyByParams.IsModified();
        }

        void Dirtied() {
            _isModified = true;
            _useCachedHeaders = false;
        }

        static internal void AppendValueToHeader(StringBuilder s, String value) {
            if (!String.IsNullOrEmpty(value)) {
                if (s.Length > 0) {
                    s.Append(", ");
                }

                s.Append(value);
            }
        }

        static readonly string[] s_cacheabilityTokens = new String[]
        {
            null,           // no enum
            "no-cache",     // HttpCacheability.NoCache
            "private",      // HttpCacheability.Private
            "no-cache",     // HttpCacheability.ServerAndNoCache
            "public",       // HttpCacheability.Public
            "private",      // HttpCacheability.ServerAndPrivate
            null            // None - not specified
        };

        static readonly string[] s_revalidationTokens = new String[]
        {
            null,               // no enum
            "must-revalidate",  // HttpCacheRevalidation.AllCaches
            "proxy-revalidate", // HttpCacheRevalidation.ProxyCaches
            null                // HttpCacheRevalidation.None
        };

        static readonly int[] s_cacheabilityValues = new int[]
        {
            -1,     // no enum
            0,      // HttpCacheability.NoCache
            2,      // HttpCacheability.Private
            1,      // HttpCacheability.ServerAndNoCache
            4,      // HttpCacheability.Public
            3,      // HttpCacheability.ServerAndPrivate
            100,    // None - though private by default, an explicit set will override
        };

        DateTime UpdateLastModifiedTimeFromDependency(CacheDependency dep) {
            DateTime utcFileLastModifiedMax = dep.UtcLastModified;
            if (utcFileLastModifiedMax < _utcLastModified) {
                utcFileLastModifiedMax = _utcLastModified;
            }
            // account for difference between file system time 
            // and DateTime.Now. On some machines it appears that
            // the last modified time is further in the future
            // that DateTime.Now                
            DateTime utcNow = DateTime.UtcNow;
            if (utcFileLastModifiedMax > utcNow) {
                utcFileLastModifiedMax = utcNow;
            }
            return utcFileLastModifiedMax;
        }

        /*
         * Calculate LastModified and ETag
         * 
         * The LastModified date is the latest last-modified date of 
         * every file that is added as a dependency.
         * 
         * The ETag is generated by concatentating the appdomain id, 
         * filenames and last modified dates of all files into a single string, 
         * then hashing it and Base 64 encoding the hash.
         */
        void UpdateFromDependencies(HttpResponse response) {
            CacheDependency dep = null;
            // if _etag != null && _generateEtagFromFiles == true, then this HttpCachePolicy
            // was created from HttpCachePolicySettings and we don't need to update _etag.
            if (_etag == null && _generateEtagFromFiles) {
                dep = response.CreateCacheDependencyForResponse();
                if (dep == null) {
                    return;
                }
                string id = dep.GetUniqueID();
                if (id == null) {
                    throw new HttpException(SR.GetString(SR.No_UniqueId_Cache_Dependency));
                }
                DateTime utcFileLastModifiedMax = UpdateLastModifiedTimeFromDependency(dep);
                StringBuilder sb = new StringBuilder(256);
                sb.Append(HttpRuntime.AppDomainIdInternal);
                sb.Append(id);
                sb.Append("+LM");
                sb.Append(utcFileLastModifiedMax.Ticks.ToString(CultureInfo.InvariantCulture));
                _etag = Convert.ToBase64String(CryptoUtil.ComputeSHA256Hash(Encoding.UTF8.GetBytes(sb.ToString())));

                //WOS 1540412: if we generate the etag based on file dependencies, encapsulate it within quotes.
                _etag = "\"" + _etag + "\"";
            }

            if (_generateLastModifiedFromFiles) {
                if (dep == null) {
                    dep = response.CreateCacheDependencyForResponse();
                    if (dep == null) {
                        return;
                    }
                }
                DateTime utcFileLastModifiedMax = UpdateLastModifiedTimeFromDependency(dep);
                UtcSetLastModified(utcFileLastModifiedMax);                
            }
        }


        void UpdateCachedHeaders(HttpResponse response) {
            StringBuilder       sb;
            HttpCacheability    cacheability;
            int                 i, n;
            String              expirationDate;           
            String              lastModifiedDate;         
            String              varyByHeaders;            
            bool                omitVaryStar;

            if (_useCachedHeaders) {
                return;
            }

            //To enable Out of Band OutputCache Module support, we will always refresh the UtcTimestampRequest.
            if (_utcTimestampCreated == DateTime.MinValue) {
                _utcTimestampCreated = response.Context.UtcTimestamp;
            }
            _utcTimestampRequest = response.Context.UtcTimestamp; 

            if (_slidingExpiration != 1) {
                _slidingDelta = TimeSpan.Zero;
            }
            else if (_isMaxAgeSet) {
                _slidingDelta = _maxAge;
            } 
            else if (_isExpiresSet) {
                _slidingDelta = _utcExpires - _utcTimestampCreated;
            }
            else {
                _slidingDelta = TimeSpan.Zero;
            }

            _headerCacheControl = null;
            _headerPragma = null;      
            _headerExpires = null;     
            _headerLastModified = null;
            _headerEtag = null;        
            _headerVaryBy = null;      

            UpdateFromDependencies(response);

            /*
             * Cache control header
             */
            sb = new StringBuilder();

            if (_cacheability == (HttpCacheability) (int) HttpCacheabilityLimits.None) {
                cacheability = HttpCacheability.Private;
            }
            else {
                cacheability = _cacheability;
            }

            AppendValueToHeader(sb, s_cacheabilityTokens[(int) cacheability]);

            if (cacheability == HttpCacheability.Public && _privateFields != null) {
                Debug.Assert(_privateFields.Size > 0);

                AppendValueToHeader(sb, "private=\"");
                sb.Append(_privateFields.GetKey(0));
                for (i = 1, n = _privateFields.Size; i < n; i++) {
                    AppendValueToHeader(sb, _privateFields.GetKey(i));
                }

                sb.Append('\"');
            }

            if (    cacheability != HttpCacheability.NoCache &&
                    cacheability != HttpCacheability.ServerAndNoCache && 
                    _noCacheFields != null) {

                Debug.Assert(_noCacheFields.Size > 0);

                AppendValueToHeader(sb, "no-cache=\"");
                sb.Append(_noCacheFields.GetKey(0));
                for (i = 1, n = _noCacheFields.Size; i < n; i++) {
                    AppendValueToHeader(sb, _noCacheFields.GetKey(i));
                }

                sb.Append('\"');
            } 

            if (_noStore) {
                AppendValueToHeader(sb, "no-store");
            }

            AppendValueToHeader(sb, s_revalidationTokens[(int)_revalidation]);

            if (_noTransforms) {
                AppendValueToHeader(sb, "no-transform");
            }

            if (_cacheExtension != null) {
                AppendValueToHeader(sb, _cacheExtension);
            }


            /*
             * don't send expiration information when item shouldn't be cached
             * for cached header, only add max-age when it doesn't change
             * based on the time requested
             */
            if (      _slidingExpiration == 1                 
                 &&   cacheability != HttpCacheability.NoCache
                 &&   cacheability != HttpCacheability.ServerAndNoCache) {
                
                if (_isMaxAgeSet && !_noMaxAgeInCacheControl) {
                    AppendValueToHeader(sb, "max-age=" + ((long)_maxAge.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                }
    
                if (_isProxyMaxAgeSet && !_noMaxAgeInCacheControl) {
                    AppendValueToHeader(sb, "s-maxage=" + ((long)(_proxyMaxAge).TotalSeconds).ToString(CultureInfo.InvariantCulture));
                }
            }

            if (sb.Length > 0) {
                _headerCacheControl = new HttpResponseHeader(HttpWorkerRequest.HeaderCacheControl, sb.ToString());
            }

            /*
             * Pragma: no-cache and Expires: -1
             */
            if (cacheability == HttpCacheability.NoCache || cacheability == HttpCacheability.ServerAndNoCache) {
                if (s_headerPragmaNoCache == null) {
                    s_headerPragmaNoCache = new HttpResponseHeader(HttpWorkerRequest.HeaderPragma, "no-cache");
                }

                _headerPragma = s_headerPragmaNoCache;

                if (_allowInHistory != 1) {
                    if (s_headerExpiresMinus1 == null) {
                        s_headerExpiresMinus1 = new HttpResponseHeader(HttpWorkerRequest.HeaderExpires, "-1");
                    }

                    _headerExpires = s_headerExpiresMinus1;
                }
            }
            else {
                /*
                 * Expires header.
                 */
                if (_isExpiresSet && _slidingExpiration != 1) {
                    expirationDate = HttpUtility.FormatHttpDateTimeUtc(_utcExpires);
                    _headerExpires = new HttpResponseHeader(HttpWorkerRequest.HeaderExpires, expirationDate);
                }

                /*
                 * Last Modified header.
                 */
                if (_isLastModifiedSet) {
                    lastModifiedDate = HttpUtility.FormatHttpDateTimeUtc(_utcLastModified);
                    _headerLastModified = new HttpResponseHeader(HttpWorkerRequest.HeaderLastModified, lastModifiedDate);
                }


                if (cacheability != HttpCacheability.Private) {
                    /*
                     * Etag.
                     */
                    if (_etag != null) {
                        _headerEtag = new HttpResponseHeader(HttpWorkerRequest.HeaderEtag, _etag);
                    }

                    /*
                     * Vary
                     */
                    varyByHeaders = null;

                    // automatic VaryStar processing
                    // See if anyone has explicitly set this value
                    if (_omitVaryStar != -1) {
                        omitVaryStar = _omitVaryStar == 1 ? true : false;
                    }
                    else {
                        // If no one has set this value, go with the default from config
                        RuntimeConfig config = RuntimeConfig.GetLKGConfig(response.Context);
                        OutputCacheSection outputCacheConfig = config.OutputCache;
                        if (outputCacheConfig != null) {
                            omitVaryStar = outputCacheConfig.OmitVaryStar;
                        }
                        else {
                            omitVaryStar = OutputCacheSection.DefaultOmitVaryStar;
                        }
                    }
                    
                    if (!omitVaryStar) {
                        // Dev10 Bug 425047 - OutputCache Location="ServerAndClient" (HttpCacheability.ServerAndPrivate) should 
                        // not use "Vary: *" so the response can be cached on the client
                        if (_varyByCustom != null || (_varyByParams.IsModified() && !_varyByParams.IgnoreParams)) {
                            varyByHeaders = "*";
                        }
                    }

                    if (varyByHeaders == null) {
                       varyByHeaders = _varyByHeaders.ToHeaderString();
                    }

                    if (varyByHeaders != null) {
                       _headerVaryBy = new HttpResponseHeader(HttpWorkerRequest.HeaderVary, varyByHeaders);
                    }
                }
            }

            _useCachedHeaders = true;
        }



        /*
         * Generate headers and append them to the list
         */
        internal void GetHeaders(ArrayList headers, HttpResponse response) {
            StringBuilder       sb;
            String              expirationDate;           
            TimeSpan            age, maxAge, proxyMaxAge; 
            DateTime            utcExpires;                  
            HttpResponseHeader  headerExpires;
            HttpResponseHeader  headerCacheControl;

            UpdateCachedHeaders(response);
            headerExpires = _headerExpires;
            headerCacheControl = _headerCacheControl;

            /* 
             * reconstruct headers that vary with time 
             * don't send expiration information when item shouldn't be cached
             */
            if (_cacheability != HttpCacheability.NoCache && _cacheability != HttpCacheability.ServerAndNoCache) {
                if (_slidingExpiration == 1) {
                    /* update Expires header */
                    if (_isExpiresSet) {
                        utcExpires = _utcTimestampRequest + _slidingDelta;
                        expirationDate = HttpUtility.FormatHttpDateTimeUtc(utcExpires);
                        headerExpires = new HttpResponseHeader(HttpWorkerRequest.HeaderExpires, expirationDate);
                    }
                }
                else {
                    if (_isMaxAgeSet || _isProxyMaxAgeSet) {
                        /* update max-age, s-maxage components of Cache-Control header */
                        if (headerCacheControl != null) {
                            sb = new StringBuilder(headerCacheControl.Value);
                        }
                        else {
                            sb = new StringBuilder();
                        }

                        age = _utcTimestampRequest - _utcTimestampCreated;
                        if (_isMaxAgeSet) {
                            maxAge = _maxAge - age;
                            if (maxAge < TimeSpan.Zero) {
                                maxAge = TimeSpan.Zero;
                            }

                            if (!_noMaxAgeInCacheControl)
                                AppendValueToHeader(sb, "max-age=" + ((long)maxAge.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                        }

                        if (_isProxyMaxAgeSet) {
                            proxyMaxAge = _proxyMaxAge - age;
                            if (proxyMaxAge < TimeSpan.Zero) {
                                proxyMaxAge = TimeSpan.Zero;
                            }

                            if (!_noMaxAgeInCacheControl)
                                AppendValueToHeader(sb, "s-maxage=" + ((long)(proxyMaxAge).TotalSeconds).ToString(CultureInfo.InvariantCulture));
                        }

                        headerCacheControl = new HttpResponseHeader(HttpWorkerRequest.HeaderCacheControl, sb.ToString());
                    }
                }
            }

            if (headerCacheControl != null) {
                headers.Add(headerCacheControl);
            }

            if (_headerPragma != null) {
                headers.Add(_headerPragma);
            }

            if (headerExpires != null) {
                headers.Add(headerExpires);
            }

            if (_headerLastModified != null) {
                headers.Add(_headerLastModified);
            }

            if (_headerEtag != null) {
                headers.Add(_headerEtag);
            }

            if (_headerVaryBy != null) {
                headers.Add(_headerVaryBy);
            }
        }
 
        /*
        * Public methods
        */

        internal HttpCachePolicySettings GetCurrentSettings(HttpResponse response) {
            String[]                    varyByContentEncodings;
            String[]                    varyByHeaders;
            String[]                    varyByParams;
            String[]                    privateFields;
            String[]                    noCacheFields;
            ValidationCallbackInfo[]    validationCallbackInfo;
            
            UpdateCachedHeaders(response);

            varyByContentEncodings = _varyByContentEncodings.GetContentEncodings();
            varyByHeaders = _varyByHeaders.GetHeaders();
            varyByParams = _varyByParams.GetParams();

            if (_privateFields != null) {
                privateFields = _privateFields.GetAllKeys();
            }
            else {
                privateFields = null;
            }

            if (_noCacheFields != null) {
                noCacheFields = _noCacheFields.GetAllKeys();
            }
            else {
                noCacheFields = null;
            }

            if (_validationCallbackInfo != null) {
                validationCallbackInfo = new ValidationCallbackInfo[_validationCallbackInfo.Count];
                _validationCallbackInfo.CopyTo(0, validationCallbackInfo, 0, _validationCallbackInfo.Count);
            }
            else {
                validationCallbackInfo = null;
            }

            return new HttpCachePolicySettings(
                    _isModified,                   
                    validationCallbackInfo,
                    _hasSetCookieHeader,
                    _noServerCaching,   
                    _cacheExtension,             
                    _noTransforms,
                    _ignoreRangeRequests,
                    varyByContentEncodings,
                    varyByHeaders,                  
                    varyByParams,                  
                    _varyByCustom,
                    _cacheability,                 
                    _noStore,
                    privateFields,
                    noCacheFields,               
                    _utcExpires,                      
                    _isExpiresSet,                 
                    _maxAge,                       
                    _isMaxAgeSet,                  
                    _proxyMaxAge,                  
                    _isProxyMaxAgeSet,             
                    _slidingExpiration,            
                    _slidingDelta,
                    _utcTimestampCreated,
                    _validUntilExpires,
                    _allowInHistory,
                    _revalidation,                 
                    _utcLastModified,                 
                    _isLastModifiedSet,            
                    _etag,                         
                    _generateLastModifiedFromFiles,
                    _generateEtagFromFiles,
                    _omitVaryStar,
                    _headerCacheControl, 
                    _headerPragma,       
                    _headerExpires,      
                    _headerLastModified,
                    _headerEtag,
                    _headerVaryBy,
                    _hasUserProvidedDependencies);
        }

        internal bool   HasValidationPolicy() {

            return      _generateLastModifiedFromFiles  
                   ||   _generateEtagFromFiles          
                   ||   _validationCallbackInfo != null 
                   ||  (_validUntilExpires == 1 && _slidingExpiration != 1);

        }

        internal bool   HasExpirationPolicy() {
            return _slidingExpiration != 1 && (_isExpiresSet || _isMaxAgeSet);
        }

        internal bool   IsKernelCacheable(HttpRequest request, bool enableKernelCacheForVaryByStar) {
            return  _cacheability == HttpCacheability.Public
                && !_hasUserProvidedDependencies // Consider (Microsoft): rework dependency model to support user-provided dependencies
                && !_hasSetCookieHeader
                && !_noServerCaching
                && HasExpirationPolicy()
                && _cacheExtension == null
                && !_varyByContentEncodings.IsModified()
                && !_varyByHeaders.IsModified()
                && (!_varyByParams.IsModified() || _varyByParams.IgnoreParams || (_varyByParams.IsVaryByStar && enableKernelCacheForVaryByStar))
                && !_noStore
                && _varyByCustom == null
                && _privateFields == null
                && _noCacheFields == null
                && _validationCallbackInfo == null
                && (request != null && request.HttpVerb == HttpVerb.GET);
        }

        // VSUQFE 4225: expose some cache policy info
        // because ISAPIWorkerRequestInProcForIIS6.CheckKernelModeCacheability needs to know about it
        internal bool   IsVaryByStar {get {return _varyByParams.IsVaryByStar; }}

        internal DateTime UtcGetAbsoluteExpiration() {
            DateTime absoluteExpiration = Cache.NoAbsoluteExpiration;

            Debug.Assert(_utcTimestampCreated != DateTime.MinValue, "_utcTimestampCreated != DateTime.MinValue");
            if (_slidingExpiration != 1) {
                if (_isMaxAgeSet) {
                    absoluteExpiration = _utcTimestampCreated + _maxAge;
                }
                else if (_isExpiresSet) {
                    absoluteExpiration = _utcExpires;
                }
            }

            return absoluteExpiration;
        }

        // Expose this property to OutputCacheUtility class 
        // In order to enable Out of Band output cache module to access the Validation Callback Info
        internal IEnumerable GetValidationCallbacks() {
            if (_validationCallbackInfo == null) {
                return new ArrayList();
            }

            return _validationCallbackInfo;
        }

        /*
         * Cache at server?
         */

        /// <devdoc>
        ///    <para>A call to this method stops all server caching for the current response. </para>
        /// </devdoc>
        public void SetNoServerCaching() {
            Dirtied();
            _noServerCaching = true;
        }

        /// <summary>
        /// Return True if we should stops all server caching for current response
        /// </summary>
        /// <returns></returns>
        public bool GetNoServerCaching() {
            return _noServerCaching;
        }

        internal void SetHasSetCookieHeader() {
            Dirtied();
            _hasSetCookieHeader = true;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void SetVaryByCustom(string custom) {
            if (custom == null) {
                throw new ArgumentNullException("custom");
            }

            if (_varyByCustom != null) {
                throw new InvalidOperationException(SR.GetString(SR.VaryByCustom_already_set));
            }

            Dirtied();
            _varyByCustom = custom;
        }

        /// <summary>
        /// Get the Vary by Custom Value
        /// </summary>
        /// <returns></returns>
        public string GetVaryByCustom() {
            return _varyByCustom;
        }
        /*
         * Cache-Control: extension        
         */

        /// <devdoc>
        ///    <para>Appends a cache control extension directive to the Cache-Control: header.</para>
        /// </devdoc>
        public void AppendCacheExtension(String extension) {
            if (extension == null) {
                throw new ArgumentNullException("extension");
            }

            Dirtied();
            if (_cacheExtension == null) {
                _cacheExtension = extension;
            }
            else {
                _cacheExtension = _cacheExtension + ", " + extension;
            }
        }

        /// <summary>
        /// Get Cache Extensions Value
        /// </summary>
        /// <returns></returns>
        public string GetCacheExtensions() {
            return _cacheExtension;
        }

        /*
         * Cache-Control: no-transform        
         */

        /// <devdoc>
        ///    <para>Enables the sending of the CacheControl:
        ///       no-transform directive.</para>
        /// </devdoc>
        public void SetNoTransforms() {
            Dirtied();
            _noTransforms = true;
        }

        /// <summary>
        /// Return true if No-transform directive, enables the sending of the CacheControl
        /// </summary>
        /// <returns></returns>
        public bool GetNoTransforms() {
            return _noTransforms;
        }

        internal void SetIgnoreRangeRequests() {
            Dirtied();
            _ignoreRangeRequests = true;
        }

        /// <summary>
        /// Return true if ignore range request
        /// </summary>
        /// <returns></returns>
        public bool GetIgnoreRangeRequests() {
            return _ignoreRangeRequests;
        }

        /// <devdoc>
        ///    <para>Contains policy for the Vary: header.</para>
        /// </devdoc>
        public HttpCacheVaryByContentEncodings VaryByContentEncodings {
            get {
                return _varyByContentEncodings;
            }
        }


        /// <devdoc>
        ///    <para>Contains policy for the Vary: header.</para>
        /// </devdoc>
        public HttpCacheVaryByHeaders VaryByHeaders { 
            get {
                return _varyByHeaders;
            }
        }


        /// <devdoc>
        ///    <para>Contains params to vary GETs and POSTs by.</para>
        /// </devdoc>
        public HttpCacheVaryByParams VaryByParams { 
            get {
                return _varyByParams;
            }
        }

        /*
         * Cacheability policy
         * 
         * Cache-Control: public | private[=1#field] | no-cache[=1#field] | no-store
         */

        /// <devdoc>
        ///    <para>Sets the Cache-Control header to one of the values of 
        ///       HttpCacheability. This is used to enable the Cache-Control: public, private, and no-cache directives.</para>
        /// </devdoc>
        public void SetCacheability(HttpCacheability cacheability) {
            if ((int) cacheability < (int) HttpCacheabilityLimits.MinValue || 
                (int) HttpCacheabilityLimits.MaxValue < (int) cacheability) {

                throw new ArgumentOutOfRangeException("cacheability");
            }

            if (s_cacheabilityValues[(int)cacheability] < s_cacheabilityValues[(int)_cacheability]) {
                Dirtied();
                _cacheability = cacheability;
            }
        }

        /// <summary>
        /// Get the Cache-control (public, private and no-cache) directive
        /// </summary>
        /// <returns></returns>
        public HttpCacheability GetCacheability() {
            return _cacheability;
        }
        
       
        /// <devdoc>
        ///    <para>Sets the Cache-Control header to one of the values of HttpCacheability in 
        ///       conjunction with a field-level exclusion directive.</para>
        /// </devdoc>
        public void SetCacheability(HttpCacheability cacheability, String field) {
            if (field == null) {
                throw new ArgumentNullException("field");
            }

            switch (cacheability) {
                case HttpCacheability.Private:
                    if (_privateFields == null) {
                        _privateFields = new HttpDictionary();
                    }

                    _privateFields.SetValue(field, field);

                    break;

                case HttpCacheability.NoCache:
                    if (_noCacheFields == null) {
                        _noCacheFields = new HttpDictionary();
                    }

                    _noCacheFields.SetValue(field, field);

                    break;

                default:
                    throw new ArgumentException(
                            SR.GetString(SR.Cacheability_for_field_must_be_private_or_nocache),
                            "cacheability");
            }

            Dirtied();
        }



        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void SetNoStore() {
            Dirtied();
            _noStore = true;
        }

        internal void SetDependencies(bool hasUserProvidedDependencies) {
            Dirtied();
            _hasUserProvidedDependencies = hasUserProvidedDependencies;
        }
        
        /// <summary>
        /// return true if no store is set
        /// </summary>
        /// <returns></returns>
        public bool GetNoStore() {
            return _noStore;
        }

        /*
         * Expiration policy.
         */

        /*
         * Expires: RFC date
         */

        /// <devdoc>
        ///    <para>Sets the Expires: header to the given absolute date.</para>
        /// </devdoc>
        public void SetExpires(DateTime date) {
            DateTime utcDate, utcNow;

            utcDate = DateTimeUtil.ConvertToUniversalTime(date);
            utcNow = DateTime.UtcNow;

            if (utcDate - utcNow > s_oneYear) {
                utcDate = utcNow + s_oneYear;
            }

            if (!_isExpiresSet || utcDate < _utcExpires) {
                Dirtied();
                _utcExpires = utcDate;
                _isExpiresSet = true;
            }
        }

        /// <summary>
        /// Return the expire header as absolute expire datetime 
        /// </summary>
        /// <returns></returns>
        public DateTime GetExpires() {
            return _utcExpires;
        }

        /*
         * Cache-Control: max-age=delta-seconds
         */

        /// <devdoc>
        ///    <para>Sets Cache-Control: s-maxage based on the specified time span</para>
        /// </devdoc>
        public void SetMaxAge(TimeSpan delta) {
            if (delta < TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException("delta");
            }

            if (s_oneYear < delta) {
                delta = s_oneYear;
            }

            if (!_isMaxAgeSet || delta < _maxAge) {
                Dirtied();
                _maxAge = delta;
                _isMaxAgeSet = true;
            }
        }

        /// <summary>
        /// Get the Cache-Control Max Age
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetMaxAge() {
            return _maxAge;
        }       

        // Suppress max-age and s-maxage in cache-control header (required for IIS6 kernel mode cache)
        internal void SetNoMaxAgeInCacheControl() {
            _noMaxAgeInCacheControl = true;
        }

        /*
         * Cache-Control: s-maxage=delta-seconds
         */

        /// <devdoc>
        ///    <para>Sets the Cache-Control: s-maxage header based on the specified time span.</para>
        /// </devdoc>
        public void SetProxyMaxAge(TimeSpan delta) {
            if (delta < TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException("delta");
            }

            if (!_isProxyMaxAgeSet || delta < _proxyMaxAge) {
                Dirtied();
                _proxyMaxAge = delta;
                _isProxyMaxAgeSet = true;
            }
        }

        /// <summary>
        /// Get the Cache-Control: Proxy Max Age Value
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetProxyMaxAge() {
            return _proxyMaxAge;
        }

        /*
         * Sliding Expiration
         */

        /// <devdoc>
        ///    <para>Make expiration sliding: that is, if cached, it should be renewed with each
        ///       response. This feature is identical in spirit to the IIS
        ///       configuration option to add an expiration header relative to the current response
        ///       time. This feature is identical in spirit to the IIS configuration option to add
        ///       an expiration header relative to the current response time.</para>
        /// </devdoc>
        public void SetSlidingExpiration(bool slide) {
            if (_slidingExpiration == -1 || _slidingExpiration == 1) {
                Dirtied();
                _slidingExpiration = (slide) ? 1 : 0;
            }
        }

        /// <summary>
        /// Return true if to make expiration sliding. that is, if cached, it should be renewed with each
        /// response. This feature is identical in spirit to the IIS
        /// configuration option to add an expiration header relative to the current response
        /// time. This feature is identical in spirit to the IIS configuration option to add
        /// an expiration header relative to the current response time.
        /// </summary>
        /// <returns></returns>
        public bool HasSlidingExpiration() {
            return _slidingExpiration == 1;
        }

        public void SetValidUntilExpires(bool validUntilExpires) {
            if (_validUntilExpires == -1 || _validUntilExpires == 1) {
                Dirtied();
                _validUntilExpires = (validUntilExpires) ? 1 : 0;
            }
        }

        /// <summary>
        /// Return true if valid until expires
        /// </summary>
        /// <returns></returns>
        public bool IsValidUntilExpires() {
            return _validUntilExpires == 1;
        }

        public void SetAllowResponseInBrowserHistory(bool allow) {
            if (_allowInHistory == -1 || _allowInHistory == 1) {
                Dirtied();
                _allowInHistory = (allow) ? 1 : 0;
            }
        }

        /* 
         * Validation policy. 
         */

        /*
         * Cache-control: must-revalidate | proxy-revalidate
         */

        /// <devdoc>
        ///    <para>Set the Cache-Control: header to reflect either the must-revalidate or 
        ///       proxy-revalidate directives based on the supplied value. The default is to
        ///       not send either of these directives unless explicitly enabled using this
        ///       method.</para>
        /// </devdoc>
        public void SetRevalidation(HttpCacheRevalidation revalidation) {
            if ((int) revalidation < (int) HttpCacheRevalidationLimits.MinValue || 
                (int) HttpCacheRevalidationLimits.MaxValue < (int) revalidation) {
                throw new ArgumentOutOfRangeException("revalidation");
            }

            if ((int) revalidation < (int) _revalidation) {
                Dirtied();
                _revalidation = revalidation;
            }
        }

        /// <summary>
        /// Get the Cache-Control: header to reflect either the must-revalidate or 
        /// proxy-revalidate directives. 
        /// The default is to not send either of these directives unless explicitly enabled using this method.
        /// </summary>
        /// <returns></returns>
        public HttpCacheRevalidation GetRevalidation() {
            return _revalidation;
        }

         /*
         * Etag
         */

        /// <devdoc>
        ///    <para>Set the ETag header to the supplied string. Once an ETag is set, 
        ///       subsequent attempts to set it will fail and an exception will be thrown.</para>
        /// </devdoc>
        public void SetETag(String etag) {
            if (etag == null) {
                throw new ArgumentNullException("etag");
            }

            if (_etag != null) {
                throw new InvalidOperationException(SR.GetString(SR.Etag_already_set));
            }

            if (_generateEtagFromFiles) {
                throw new InvalidOperationException(SR.GetString(SR.Cant_both_set_and_generate_Etag));
            }

            Dirtied();
            _etag = etag;
        }

        /// <summary>
        /// Get the ETag header. Once an ETag is set, 
        /// subsequent attempts to set it will fail and an exception will be thrown.
        /// </summary>
        /// <returns></returns>
        public string GetETag() {
            return _etag;
        }


        /*
         * Last-Modified: RFC Date
         */

        /// <devdoc>
        ///    <para>Set the Last-Modified: header to the DateTime value supplied. If this 
        ///       violates the restrictiveness hierarchy, this method will fail.</para>
        /// </devdoc>
        public void SetLastModified(DateTime date) {
            DateTime utcDate = DateTimeUtil.ConvertToUniversalTime(date);
            UtcSetLastModified(utcDate);
        }

        void UtcSetLastModified(DateTime utcDate) {

           /*
            * DevDiv# 545481
            * Time may differ if the system time changes in the middle of the request. 
            * Adjust the timestamp to Now if necessary.
            */

            DateTime utcNow = DateTime.UtcNow;
            if (utcDate > utcNow) {
                utcDate = utcNow;
            }

            /*
             * Because HTTP dates have a resolution of 1 second, we
             * need to store dates with 1 second resolution or comparisons
             * will be off.
             */

            utcDate = new DateTime(utcDate.Ticks - (utcDate.Ticks % TimeSpan.TicksPerSecond));
            if (!_isLastModifiedSet || utcDate > _utcLastModified) {
                Dirtied();
                _utcLastModified = utcDate;
                _isLastModifiedSet = true;
            }
        }

        /// <summary>
        /// Get the Last-Modified header. 
        /// </summary>
        /// <returns></returns>
        public DateTime GetUtcLastModified() {
            return _utcLastModified;
        }


        /// <devdoc>
        ///    <para>Sets the Last-Modified: header based on the timestamps of the
        ///       file dependencies of the handler.</para>
        /// </devdoc>
        public void SetLastModifiedFromFileDependencies() {
            Dirtied();
            _generateLastModifiedFromFiles = true; 
        }

        /// <summary>
        /// Return true if the Last-Modified header is set to base on the timestamps of the
        /// file dependencies of the handler.
        /// </summary>
        /// <returns></returns>
        public bool GetLastModifiedFromFileDependencies() {
            return _generateLastModifiedFromFiles;
        }


        /// <devdoc>
        ///    <para>Sets the Etag header based on the timestamps of the file 
        ///       dependencies of the handler.</para>
        /// </devdoc>
        public void SetETagFromFileDependencies() {
            if (_etag != null) {
                throw new InvalidOperationException(SR.GetString(SR.Cant_both_set_and_generate_Etag));
            }

            Dirtied();
            _generateEtagFromFiles = true;         
        }

        /// <summary>
        /// Return true if the Etag header has been set to base on the timestamps of the file 
        /// dependencies of the handler
        /// </summary>
        /// <returns></returns>
        public bool GetETagFromFileDependencies() {
            return _generateEtagFromFiles;
        }

        public void SetOmitVaryStar(bool omit) {
            Dirtied();
            if (_omitVaryStar == -1 || _omitVaryStar == 1) {
                Dirtied();
                _omitVaryStar = (omit) ? 1 : 0;
            }
        }

        /// <summary>
        /// Return true if to omit Vary Star
        /// </summary>
        /// <returns></returns>
        public int GetOmitVaryStar() {
            return _omitVaryStar;
        }

        /// <devdoc>
        ///    <para>Registers a validation callback for the current response.</para>
        /// </devdoc>
        public void AddValidationCallback(
                HttpCacheValidateHandler handler, Object data) {

            if (handler == null) {
                throw new ArgumentNullException("handler");
            }

            Dirtied();
            if (_validationCallbackInfo == null) {
                _validationCallbackInfo = new ArrayList();
            }

            _validationCallbackInfo.Add(new ValidationCallbackInfo(handler, data));
        }
        /// <summary>
        /// Utc Timestamp Created
        /// </summary>
        public DateTime UtcTimestampCreated {
            get {
                return _utcTimestampCreated;
            }
            set {
                _utcTimestampCreated = value;
            }
        }
    }
}
