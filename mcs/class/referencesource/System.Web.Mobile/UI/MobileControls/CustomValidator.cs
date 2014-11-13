//------------------------------------------------------------------------------
// <copyright file="CustomValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics;
using System.Web.UI.WebControls;
using WebCntrls = System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile CustomValidator class.
     * The CustomValidator provides the ability to easily write custom server
     * validation logic.  A user-defined function is called via a single-cast
     * delegate to provide server-side custom validation.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\CustomValidator.uex' path='docs/doc[@for="CustomValidator"]/*' />
    [
        DefaultEvent("ServerValidate"),
        ToolboxData("<{0}:CustomValidator runat=\"server\" ErrorMessage=\"CustomValidator\"></{0}:CustomValidator>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class CustomValidator : BaseValidator
    {
        private WebCntrls.CustomValidator _webCustomValidator;

        // Static objects to identify individual events stored in Events
        // property.
        private static readonly Object EventServerValidate = new Object();

        /// <include file='doc\CustomValidator.uex' path='docs/doc[@for="CustomValidator.CreateWebValidator"]/*' />
        protected override WebCntrls.BaseValidator CreateWebValidator()
        {
            _webCustomValidator = new WebCntrls.CustomValidator();

            // Adding wrapper event handlers for event properties exposed by
            // the aggregated control.  For more details about the mechanism,
            // please see the comment in the constructor of
            // Mobile.UI.AdRotator.
            ServerValidateEventHandler eventHandler =
                new ServerValidateEventHandler(WebServerValidate);

            _webCustomValidator.ServerValidate += eventHandler;

            return _webCustomValidator;
        }

        ////////////////////////////////////////////////////////////////////////
        // Mimic the properties exposed in the original CustomValidator.
        ////////////////////////////////////////////////////////////////////////

        /// <include file='doc\CustomValidator.uex' path='docs/doc[@for="CustomValidator.ServerValidate"]/*' />
        [
            Bindable(false),
            DefaultValue(null),
            MobileSysDescription(SR.CustomValidator_OnServerValidate)
        ]
        public event ServerValidateEventHandler ServerValidate
        {
            add
            {
                Events.AddHandler(EventServerValidate, value);
            }
            remove
            {
                Events.RemoveHandler(EventServerValidate, value);
            }
        }

        // protected method (which can be overridden by subclasses) for
        // raising user events
        /// <include file='doc\CustomValidator.uex' path='docs/doc[@for="CustomValidator.OnServerValidate"]/*' />
        protected virtual bool OnServerValidate(String value)
        {
            ServerValidateEventHandler handler = (ServerValidateEventHandler)Events[EventServerValidate];
            if (handler != null) 
            {
                ServerValidateEventArgs args = new ServerValidateEventArgs(value, true);
                handler(this, args);
                return args.IsValid;
            }
            else 
            {
                return true;
            }
        }

        private void WebServerValidate(Object source, ServerValidateEventArgs e)
        {
            // Invoke user events for further manipulation specified by user
            Debug.Assert(e != null, "Unexpected null parameter!");
            e.IsValid = OnServerValidate(e.Value);
        }

        /// <include file='doc\CustomValidator.uex' path='docs/doc[@for="CustomValidator.EvaluateIsValid"]/*' />
        protected override bool EvaluateIsValid()
        {
            return EvaluateIsValidInternal();
        }

        /////////////////////////////////////////////////////////////////////
        // Helper function adopted from WebForms CustomValidator
        /////////////////////////////////////////////////////////////////////

        /// <include file='doc\CustomValidator.uex' path='docs/doc[@for="CustomValidator.ControlPropertiesValid"]/*' />
        protected override bool ControlPropertiesValid()
        {
            // Need to override the BaseValidator implementation, because for
            // CustomValidator, it is fine for ControlToValidate to be blank.
            String controlToValidate = ControlToValidate;
            if (controlToValidate.Length > 0)
            {
                return base.ControlPropertiesValid();
            }
            return true;
        }
    }
}
