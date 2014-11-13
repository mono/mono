//------------------------------------------------------------------------------
// <copyright file="BrowserTree.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Collections;
    using System.Collections.Specialized;

    //
    //
    //    <browsers>
    //    <browser id="XXX" parentID="YYY">
    //         <identification>
    //              <userAgent match="xxx" />
    //              <header name="HTTP_X_JPHONE_DISPLAY" match="xxx" />
    //              <capability name="majorVersion" match="^6$" />
    //         </identification>
    //         <capture>
    //              <header name="HTTP_X_UP_DEVCAP_NUMSOFTKEYS" match="?'softkeys'\d+)" />
    //         </capture>
    //         <capabilities>
    //              <mobileDeviceManufacturer>OpenWave</mobileDeviceManufacturer>
    //              <numberOfSoftKeys>$(softkeys)</numberOfSoftKeys>
    //         </capabilities>
    //         <controlAdapters>
    //              <adapter controlType="System.Web.UI.WebControls.Image" 
    //                       adapterType="System.Web.UI.WebControls.Adapters.Html32ImageAdapter" />
    //         </controlAdapters>
    //    </browser>
    //    </browsers>
    //
         
    internal class BrowserTree : OrderedDictionary {
        internal BrowserTree() : base(StringComparer.OrdinalIgnoreCase) {
        }
    }
}
