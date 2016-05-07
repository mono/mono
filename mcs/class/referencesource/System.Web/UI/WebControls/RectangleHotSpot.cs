//------------------------------------------------------------------------------
// <copyright file="RectangleHotSpot.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {
    
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;

    /// <devdoc>
    /// <para>Implements HotSpot for rectangle regions.</para>
    /// </devdoc>
    public sealed class RectangleHotSpot : HotSpot {

        [
        WebCategory("Appearance"),
        DefaultValue(0),
        WebSysDescription(SR.RectangleHotSpot_Bottom),
        ]
        public int Bottom {
            get {
                object o = ViewState["Bottom"];
                return o != null? (int)o : 0;
            }
            set {
                ViewState["Bottom"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(0),
        WebSysDescription(SR.RectangleHotSpot_Left),
        ]
        public int Left {
            get {
                object o = ViewState["Left"];
                return o != null? (int)o : 0;
            }
            set {
                ViewState["Left"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(0),
        WebSysDescription(SR.RectangleHotSpot_Right),
        ]
        public int Right {
            get {
                object o = ViewState["Right"];
                return o != null? (int)o : 0;
            }
            set {
                ViewState["Right"] = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(0),
        WebSysDescription(SR.RectangleHotSpot_Top),
        ]
        public int Top {
            get {
                object o = ViewState["Top"];
                return o != null? (int)o : 0;
            }
            set {
                ViewState["Top"] = value;
            }
        }


        protected internal override string MarkupName {
            get {
                return "rect";
            }
        }


        public override string GetCoordinates() {
            return Left + "," + Top + "," + Right + "," + Bottom;
        }
    }
}
