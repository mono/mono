//------------------------------------------------------------------------------
// <copyright file="WebPartMovingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    public class WebPartMovingEventArgs : WebPartCancelEventArgs {
        private WebPartZoneBase _zone;
        private int _zoneIndex;

        public WebPartMovingEventArgs(WebPart webPart, WebPartZoneBase zone, int zoneIndex) : base(webPart) {
            _zone = zone;
            _zoneIndex = zoneIndex;
        }

        public WebPartZoneBase Zone {
            get {
                return _zone;
            }
            set {
                _zone = value;
            }
        }

        public int ZoneIndex {
            get {
                return _zoneIndex;
            }
            set {
                _zoneIndex = value;
            }
        }
    }
}


