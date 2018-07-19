//------------------------------------------------------------------------------
// <copyright file="PolygonHotSpot.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;


    /// <devdoc>
    /// <para>Implements HotSpot for polygon regions.</para>
    /// </devdoc>
    public sealed class PolygonHotSpot : HotSpot { 

        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Appearance"),
        WebSysDescription(SR.PolygonHotSpot_Coordinates),
        ]
        public String Coordinates {
            get {
                String o = ViewState["Coordinates"] as String;
                return o != null ? o : String.Empty;
            }
            set {
                ViewState["Coordinates"] = value;
            }
        }


        protected internal override string MarkupName {
            get {
                return "poly";
            }
        }
        

        public override string GetCoordinates() {
            return Coordinates;
        }
    }
}
