//------------------------------------------------------------------------------
// <copyright file="ControlCachePolicy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Web;
using System.Web.Util;
using System.Web.UI.WebControls;
using System.Web.Caching;
using System.Security.Permissions;

public sealed class ControlCachePolicy {

    private static ControlCachePolicy _cachePolicyStub = new ControlCachePolicy();

    private BasePartialCachingControl _pcc;

    internal ControlCachePolicy() {
    }

    internal ControlCachePolicy(BasePartialCachingControl pcc) {
        _pcc = pcc;
    }

    internal static ControlCachePolicy GetCachePolicyStub() {
        // Return a stub, which returns SupportsCaching==false and throws on everything else.
        return _cachePolicyStub;
    }

    // Check whether it is valid to access properties on this object
    private void CheckValidCallingContext() {

        // If it's not being cached, the CachePolicy can't be used
        if (_pcc == null) {
            throw new HttpException(
                SR.GetString(SR.UC_not_cached));
        }

        // Make sure it's not being used too late
        if (_pcc.ControlState >= ControlState.PreRendered) {
            throw new HttpException(
                SR.GetString(SR.UCCachePolicy_unavailable));
        }
    }


    public bool SupportsCaching {
        get {
            // Caching is supported if we have a PartialCachingControl
            return (_pcc != null);
        }
    }


    public bool Cached {
        get {
            CheckValidCallingContext();

            return !_pcc._cachingDisabled;
        }
        
        set {
            CheckValidCallingContext();

            _pcc._cachingDisabled = !value;
        }
    }


    public TimeSpan Duration {
        get {
            CheckValidCallingContext();

            return _pcc.Duration;
        }
        
        set {
            CheckValidCallingContext();

            _pcc.Duration = value;
        }
    }


    public HttpCacheVaryByParams VaryByParams {
        get {
            CheckValidCallingContext();

            return _pcc.VaryByParams;
        }
    }


    public string VaryByControl {
        get {
            CheckValidCallingContext();

            return _pcc.VaryByControl;
        }
        
        set {
            CheckValidCallingContext();

            _pcc.VaryByControl = value;
        }
    }


    public CacheDependency Dependency {
        get {
            CheckValidCallingContext();

            return _pcc.Dependency;
        }
        
        set {
            CheckValidCallingContext();

            _pcc.Dependency = value;
        }
    }


    public void SetVaryByCustom(string varyByCustom) {
        CheckValidCallingContext();

        _pcc._varyByCustom = varyByCustom;
    }


    public void SetSlidingExpiration(bool useSlidingExpiration) {
        CheckValidCallingContext();

        _pcc._useSlidingExpiration = useSlidingExpiration;
    }


    public void SetExpires(DateTime expirationTime) {
        CheckValidCallingContext();

        _pcc._utcExpirationTime = DateTimeUtil.ConvertToUniversalTime(expirationTime);
    }

    public String ProviderName {
        get {
            CheckValidCallingContext();
            if (_pcc._provider == null) {
                return OutputCache.ASPNET_INTERNAL_PROVIDER_NAME;
            }
            else {
                return _pcc._provider;            
            }
        }
        set {
            CheckValidCallingContext();
            if (value == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME) {
                value = null;
            }
            OutputCache.ThrowIfProviderNotFound(value);
            _pcc._provider = value;
        }
    }
}

}

