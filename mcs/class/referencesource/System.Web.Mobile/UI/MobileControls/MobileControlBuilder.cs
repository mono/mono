//------------------------------------------------------------------------------
// <copyright file="MobileControlBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Control builder for mobile controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\MobileControlBuilder.uex' path='docs/doc[@for="MobileControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileControlBuilder : ControlBuilder
    {
        /// <include file='doc\MobileControlBuilder.uex' path='docs/doc[@for="MobileControlBuilder.AllowWhitespaceLiterals"]/*' />
        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }

        /// <include file='doc\MobileControlBuilder.uex' path='docs/doc[@for="MobileControlBuilder.GetChildControlType"]/*' />
        public override Type GetChildControlType(String tagName, IDictionary attributes) 
        {
            Type type;

            if (String.Compare(tagName, typeof(DeviceSpecific).Name, StringComparison.OrdinalIgnoreCase) == 0) 
            {
                type = typeof(DeviceSpecific);
            }
            else
            {
                type = base.GetChildControlType(tagName, attributes);
                //if (type == null)
                //{
                //    type = Parser.RootBuilder.GetChildControlType(tagName, attributes);
                //}
            }

            //  enforce valid control nesting behaviour

            if (typeof(Form).IsAssignableFrom(type))
            {
                throw new Exception(
                    SR.GetString(SR.MobileControlBuilder_ControlMustBeTopLevelOfPage,
                                 "Form"));
            }

            if (typeof(StyleSheet).IsAssignableFrom(type))
            {
                throw new Exception(
                    SR.GetString(SR.MobileControlBuilder_ControlMustBeTopLevelOfPage,
                                 "StyleSheet"));
            }

            if (typeof(Style).IsAssignableFrom(type) && !typeof(StyleSheet).IsAssignableFrom(ControlType))
            {
                throw new Exception(
                    SR.GetString(SR.MobileControlBuilder_StyleMustBeInStyleSheet));
            }

            return type;
        }

    }

}
