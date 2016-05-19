//------------------------------------------------------------------------------
// <copyright file="IControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * ControlAdapter Interface.
     * A control adapter handles all of the (potentially) device specific 
     * functionality for a mobile control.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public interface IControlAdapter
    {
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.Control"]/*' />
        MobileControl Control
        {
            get;
            set;
        }
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.Page"]/*' />
        MobilePage Page
        {
            get;
        }
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.OnInit"]/*' />

        void OnInit(EventArgs e);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.OnLoad"]/*' />
        void OnLoad(EventArgs e);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.OnPreRender"]/*' />
        void OnPreRender(EventArgs e);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.Render"]/*' />
        void Render(HtmlTextWriter writer);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.OnUnload"]/*' />
        void OnUnload(EventArgs e);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.CreateTemplatedUI"]/*' />
        void CreateTemplatedUI(bool doDataBind);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.HandlePostBackEvent"]/*' />
        bool HandlePostBackEvent(String eventArgument);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.LoadPostData"]/*' />

        // used by controls that implement IPostBackDataHandler to handle
        // situations where the post data is interpreted based upon generating
        // device.  Returns true if there is no device-specific handling, and
        // the general control should handle it.
        bool LoadPostData(String postDataKey,
                          NameValueCollection postCollection,
                          Object controlPrivateData,
                          out bool dataChanged);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.LoadAdapterState"]/*' />
            
        void LoadAdapterState(Object state);
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.SaveAdapterState"]/*' />
        Object SaveAdapterState();
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.VisibleWeight"]/*' />

        int VisibleWeight
        {
            get;
        }
        /// <include file='doc\IControlAdapter.uex' path='docs/doc[@for="IControlAdapter.ItemWeight"]/*' />
        int ItemWeight
        {
            get;
        }
    }

}
