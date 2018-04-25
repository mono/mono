//------------------------------------------------------------------------------
// <copyright file="MobileComponentEditorPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Drawing;
    using System.Web.UI.Design;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using ControlDesigner = System.Web.UI.Design.ControlDesigner;

    /// <summary>
    ///   The base class for all mobile component editor pages.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class MobileComponentEditorPage : ComponentEditorPage 
    {
        private ControlDesigner         _designer = null;
        private IHelpService            _helpService = null;
        private ISite                   _site = null;
        private MobileControl           _control = null;

        protected abstract String HelpKeyword 
        {
            get;
        }

        protected ISite DesignerSite
        {
            get
            {
                if (_site != null)
                {
                    return _site;
                }

                IComponent selectedComponent = GetSelectedComponent();
                _site = selectedComponent.Site;
                Debug.Assert(_site != null, "Expected the component to be sited.");

                return _site;
            }
        }

        private IHelpService HelpService
        {
            get
            {
                if (_helpService != null)
                {
                    return _helpService;
                }

                _helpService = 
                    (IHelpService)DesignerSite.GetService(typeof(IHelpService));
                Debug.Assert(_helpService != null);

                return _helpService;
            }
        }

        protected MobileControl GetBaseControl()
        {
            if (_control != null)
            {
                return _control;
            }

            IComponent selectedComponent = GetSelectedComponent();
            Debug.Assert(selectedComponent is MobileControl);
            _control = (MobileControl)selectedComponent;

            return _control;
        }

        protected ControlDesigner GetBaseDesigner() 
        {
            if (_designer != null)
            {
                return _designer;
            }

            IDesignerHost designerHost = 
                (IDesignerHost)DesignerSite.GetService(typeof(IDesignerHost));
            Debug.Assert(designerHost != null, "Expected a designer host.");

            _designer = (ControlDesigner)designerHost.GetDesigner(GetSelectedComponent());

            Debug.Assert(_designer != null, "Expected a designer for the selected component");

            return _designer;
        }

        /* Removed for DCR 4240
        protected bool IsValidName(String name)
        {
            return DesignerUtility.IsValidName(name);
        }
        */

        public override void ShowHelp()
        {
            HelpService.ShowHelpFromKeyword(HelpKeyword);
        }

        public override bool SupportsHelp() 
        {
            return true;
        }

        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        protected class LoadingModeResource : IDisposable
        {
            private MobileComponentEditorPage _page;

            internal LoadingModeResource(MobileComponentEditorPage page)
            {
                _page = page;
                _page.EnterLoadingMode();
            }

            public void Dispose()
            {
                _page.ExitLoadingMode();
            }
        }
    }
}
