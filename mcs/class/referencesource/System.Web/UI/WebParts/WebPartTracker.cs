//------------------------------------------------------------------------------
// <copyright file="WebPartTracker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;

    public sealed class WebPartTracker : IDisposable {
        private bool _disposed;
        private WebPart _webPart;
        private ProviderConnectionPoint _providerConnectionPoint;

        public WebPartTracker(WebPart webPart, ProviderConnectionPoint providerConnectionPoint) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            if (providerConnectionPoint == null) {
                throw new ArgumentNullException("providerConnectionPoint");
            }

            if (providerConnectionPoint.ControlType != webPart.GetType()) {
                throw new ArgumentException(System.Web.SR.GetString(System.Web.SR.WebPartManager_InvalidConnectionPoint), "providerConnectionPoint");
            }

            _webPart = webPart;
            _providerConnectionPoint = providerConnectionPoint;

            if (++Count > 1) {
                webPart.SetConnectErrorMessage(System.Web.SR.GetString(System.Web.SR.WebPartTracker_CircularConnection, _providerConnectionPoint.DisplayName));
            }
        }

        public bool IsCircularConnection {
            get {
                return (Count > 1);
            }
        }

        private int Count {
            get {
                int count;
                _webPart.TrackerCounter.TryGetValue(_providerConnectionPoint, out count);
                return count;
            }
            set {
                _webPart.TrackerCounter[_providerConnectionPoint] = value;
            }
        }

        void IDisposable.Dispose() {
            if (!_disposed) {
                System.Web.Util.Debug.Assert(Count >= 1);
                Count--;
                _disposed = true;
            }
        }
    }
}
