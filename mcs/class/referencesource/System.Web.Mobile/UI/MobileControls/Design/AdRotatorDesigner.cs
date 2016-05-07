//------------------------------------------------------------------------------
// <copyright file="AdRotatorDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Provides design-time support for the <see cref='System.Web.UI.MobileControls.AdRotator'/>
    ///       mobile control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.AdRotator'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class AdRotatorDesigner : MobileControlDesigner 
    {
        private System.Web.UI.MobileControls.AdRotator _adRotator;

        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.AdRotator,
                         "AdRotatorDesigner.Initialize - Invalid AdRotator Control");
            _adRotator = (System.Web.UI.MobileControls.AdRotator) component;
            base.Initialize(component);
        }

        protected override String GetDesignTimeNormalHtml()
        {
            DesignerTextWriter writer = new DesignerTextWriter();
            _adRotator.Adapter.Render(writer);

            return writer.ToString();
        }
    }
}
