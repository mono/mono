#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
#if WEB_EXTENSIONS_CODE
    internal abstract class MapFile
#else
    [CLSCompliant(true)]
    public abstract class MapFile
#endif
    {
        private List<ProxyGenerationError> _loadErrors;

        /// <summary>
        /// Errors encountered during load
        /// </summary>        
        public IEnumerable<ProxyGenerationError> LoadErrors
        {
            get
            {
                return _loadErrors != null ? _loadErrors : Enumerable.Empty<ProxyGenerationError>();
            }
            internal set
            {
                _loadErrors = new List<ProxyGenerationError>(value);
            }
        }

        /// <summary>
        /// Unique ID of the reference group.  It is a GUID string.
        /// </summary>        
        public abstract string ID { get; set; }

        /// <summary>
        /// Metadata source item list
        /// </summary>
        public abstract List<MetadataSource> MetadataSourceList { get; }

        /// <summary>
        /// Metadata item list
        /// </summary>
        public abstract List<MetadataFile> MetadataList { get; }

        /// <summary>
        /// Extension item list
        /// </summary>
        public abstract List<ExtensionFile> Extensions { get; }
    }
}
