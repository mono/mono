//------------------------------------------------------------------------------
// <copyright file="ControlSkin.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.UI {


    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public delegate System.Web.UI.Control ControlSkinDelegate(Control control);


    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class ControlSkin {

        private Type _controlType;
        private ControlSkinDelegate _controlSkinDelegate;


        public ControlSkin(Type controlType, ControlSkinDelegate themeDelegate) {
            _controlType = controlType;
            _controlSkinDelegate = themeDelegate;
        }


        public Type ControlType {
            get {
                return _controlType;
            }
        }


        public void ApplySkin(Control control) {
            _controlSkinDelegate(control);
        }
    }
}
