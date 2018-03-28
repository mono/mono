using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Caching {
    [Serializable]
    internal class OutputCacheEntry: IOutputCacheEntry {
        // fields that hold metadata for the entry
        private Guid                      _cachedVaryId;
        private HttpCachePolicySettings   _settings;
        private string                    _kernelCacheUrl;
        private string                    _dependenciesKey;
        private string[]                  _dependencies; // file dependencies

        //response fields
        private int                       _statusCode;
        private String                    _statusDescription;
        private List<HeaderElement>       _headerElements;
        private List<ResponseElement>     _responseElements;

        internal Guid                    CachedVaryId      { get { return _cachedVaryId; } }
        internal HttpCachePolicySettings Settings          { get { return _settings; } }
        internal string                  KernelCacheUrl    { get { return _kernelCacheUrl; } }
        internal string                  DependenciesKey   { get { return _dependenciesKey; } }
        internal string[]                Dependencies      { get { return _dependencies; } }
        internal int                     StatusCode        { get { return _statusCode; } }
        internal string                  StatusDescription { get { return _statusDescription; } }

        public   List<HeaderElement>     HeaderElements    { get { return _headerElements; } 
                                                             set { _headerElements = value; } }

        public   List<ResponseElement>   ResponseElements  { get { return _responseElements; }
                                                             set { _responseElements = value; } }

        private OutputCacheEntry() {
            // hide default constructor
        }
        
        internal OutputCacheEntry(Guid cachedVaryId,
                                  HttpCachePolicySettings settings,
                                  string kernelCacheUrl,
                                  string dependenciesKey,
                                  string[] dependencies,
                                  int statusCode,
                                  string statusDescription,
                                  List<HeaderElement> headerElements,
                                  List<ResponseElement> responseElements) {
            _cachedVaryId = cachedVaryId;
            _settings = settings;
            _kernelCacheUrl = kernelCacheUrl;
            _dependenciesKey = dependenciesKey;
            _dependencies = dependencies;
            _statusCode = statusCode;
            _statusDescription = statusDescription;
            _headerElements = headerElements;
            _responseElements = responseElements;            
        }
    }
}
