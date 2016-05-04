//------------------------------------------------------------------------------
// <copyright file="OutputCacheParameters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI 
{
    using System;
    using System.Web.Util;
    using System.Security.Permissions;

    [FlagsAttribute()]
    internal enum OutputCacheParameter : int
    {
        // Flags to determine if a particular parameter has been set
        CacheProfile          = 0x00000001,
        Duration              = 0x00000002,
        Enabled               = 0x00000004,
        Location              = 0x00000008,
        NoStore               = 0x00000010,
        SqlDependency         = 0x00000020,
        VaryByControl         = 0x00000040,
        VaryByCustom          = 0x00000080,
        VaryByHeader          = 0x00000100,
        VaryByParam           = 0x00000200,
        VaryByContentEncoding = 0x00000400
    }

    public sealed class OutputCacheParameters
    {
        #pragma warning disable 0649
        private SimpleBitVector32 _flags;
        #pragma warning restore 0649
        private bool _enabled = true;
        private int _duration;
        private OutputCacheLocation _location;
        private string _varyByCustom;
        private string _varyByParam;
        private string _varyByContentEncoding;
        private string _varyByHeader;
        private bool _noStore;
        private string _sqlDependency;
        private string _varyByControl;
        private string _cacheProfile;

        public OutputCacheParameters()
        {
        }

        internal bool IsParameterSet(OutputCacheParameter value)
        {
            return _flags[(int) value];
        }
        
        public bool Enabled
        {
            get {
                return _enabled;
            }
            set {
                _flags[(int) OutputCacheParameter.Enabled] = true;
                _enabled = value;
            }
        }
        
        public int Duration
        {
            get {
                return _duration;
            }
            set {
                _flags[(int) OutputCacheParameter.Duration] = true;
                _duration = value;
            }
        }

        public OutputCacheLocation Location
        {
            get {
                return _location;
            }
            set {
                _flags[(int) OutputCacheParameter.Location] = true;
                _location = value;
            }
        }

        public string VaryByCustom
        {
            get {
                return _varyByCustom;
            }
            set {
                _flags[(int) OutputCacheParameter.VaryByCustom] = true;
                _varyByCustom = value;
            }
        }

        public string VaryByParam
        {
            get {
                return _varyByParam;
            }
            set {
                _flags[(int) OutputCacheParameter.VaryByParam] = true;
                _varyByParam = value;
            }
        }

        public string VaryByContentEncoding
        {
            get {
                return _varyByContentEncoding;
            }
            set {
                _flags[(int) OutputCacheParameter.VaryByContentEncoding] = true;
                _varyByContentEncoding = value;
            }
        }

        public string VaryByHeader
        {
            get {
                return _varyByHeader;
            }
            set {
                _flags[(int) OutputCacheParameter.VaryByHeader] = true;
                _varyByHeader = value;
            }
        }

        public bool NoStore
        {
            get {
                return _noStore;
            }
            set {
                _flags[(int) OutputCacheParameter.NoStore] = true;
                _noStore = value;
            }
        }

        public string SqlDependency
        {
            get {
                return _sqlDependency;
            }
            set {
                _flags[(int) OutputCacheParameter.SqlDependency] = true;
                _sqlDependency = value;
            }
        }

        public string VaryByControl
        {
            get {
                return _varyByControl;
            }
            set {
                _flags[(int) OutputCacheParameter.VaryByControl] = true;
                _varyByControl = value;
            }
        }

        public string CacheProfile
        {
            get {
                return _cacheProfile;
            }
            set {
                _flags[(int) OutputCacheParameter.CacheProfile] = true;
                _cacheProfile = value;
            }
        }
    }

}

