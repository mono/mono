//------------------------------------------------------------------------------
// <copyright file="CircleHotSpot.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;

    /// <devdoc>
    /// <para>Implements HotSpot for Circular regions.</para>
    /// </devdoc>
    public sealed class CircleHotSpot : HotSpot {

        protected internal override string MarkupName {
            get {
                return "circle";
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the radius of the MapCircle.</para>
        /// </devdoc>
        [
        DefaultValue(0),
        WebCategory("Appearance"),
        WebSysDescription(SR.CircleHotSpot_Radius),
        ]
        public int Radius {
            get {
                object radius = ViewState["Radius"];
                return (radius == null)? 0 : (int)radius;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Radius"] = value;
            }
        }


        [
        DefaultValue(0),
        WebCategory("Appearance"),
        WebSysDescription(SR.CircleHotSpot_X),
        ]
        public int X {
            get {
                object o = ViewState["X"];
                return o != null? (int)o : 0;
            }
            set {
                ViewState["X"] = value;
            }
        }


        [
        DefaultValue(0),
        WebCategory("Appearance"),
        WebSysDescription(SR.CircleHotSpot_Y),
        ]
        public int Y {
            get {
                object o = ViewState["Y"];
                return o != null? (int)o : 0;
            }
            set {
                ViewState["Y"] = value;
            }
        }


        public override string GetCoordinates() {
            return X + "," + Y + "," + Radius;
        }
    }
}
