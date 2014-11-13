//------------------------------------------------------------------------------
// <copyright file="AppliedDeviceFiltersEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Windows.Forms.Design;

    using DialogResult = System.Windows.Forms.DialogResult;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class AppliedDeviceFiltersTypeEditor : UITypeEditor
    {
        private static readonly String _appliedDeviceFiltersDescription = "AppliedDeviceFilters";

        public override Object EditValue(ITypeDescriptorContext context,  IServiceProvider  provider, Object value) 
        {
            Debug.Assert(context.Instance is Control, "Expected control");
            Control ctrl = (Control) context.Instance;

            IServiceProvider serviceProvider;
            ISite site = ctrl.Site;
            if (site == null && ctrl.Page != null) 
            {
                site = ctrl.Page.Site;
            }
            if (site != null) 
            {
                serviceProvider = site;
            }
            else 
            {
                serviceProvider = provider;
            }
            Debug.Assert(serviceProvider != null,
                "Failed to get the serviceProvider");
            
            IComponentChangeService changeService =
                (IComponentChangeService) serviceProvider.GetService(typeof(IComponentChangeService));

            IDesignerHost designerHost = 
                (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
            Debug.Assert(designerHost != null,
                "Must always have access to IDesignerHost service");

            IDeviceSpecificDesigner dsDesigner = 
                designerHost.GetDesigner(ctrl) as IDeviceSpecificDesigner;
            Debug.Assert(dsDesigner != null,
                "Expected component designer to implement IDeviceSpecificDesigner");

            IMobileWebFormServices wfServices = 
                (IMobileWebFormServices)serviceProvider.GetService(typeof(IMobileWebFormServices));

            DialogResult result = DialogResult.Cancel;

            DesignerTransaction transaction = designerHost.CreateTransaction(_appliedDeviceFiltersDescription);
            try 
            {
                if (changeService != null) 
                {
                    try 
                    {
                        changeService.OnComponentChanging(ctrl, null);
                    }
                    catch (CheckoutException ce) 
                    {
                        if (ce == CheckoutException.Canceled) 
                        {
                            return value;
                        }
                        throw;
                    }
                }

                try 
                {
                    AppliedDeviceFiltersDialog dialog = 
                        new AppliedDeviceFiltersDialog(dsDesigner, MobileControlDesigner.MergingContextChoices);
                    IWindowsFormsEditorService edSvc = 
                        (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                    result = edSvc.ShowDialog(dialog);
                }
                finally 
                {
                    if (changeService != null && result != DialogResult.Cancel) 
                    {
                        changeService.OnComponentChanged(ctrl, null, null, null);
                    }
                }
            }
            finally 
            {
                if (transaction != null) 
                {
                    if (result == DialogResult.OK) 
                    {
                        transaction.Commit();
                    }
                    else 
                    {
                        transaction.Cancel();
                    }
                }
            }

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

