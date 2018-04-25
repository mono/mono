//------------------------------------------------------------------------------
// <copyright file="ValidationSummaryDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Provides a designer for the <see cref='System.Web.UI.MobileControls.ValidationSummary'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.ValidationSummary'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ValidationSummaryDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.ValidationSummary _validationSummary;

        /// <summary>
        ///    <para>
        ///       Initializes the designer.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element being designed.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component being
        ///       designed.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.ValidationSummary,
                         "ValidationSummaryDesigner.Initialize - Invalid ValidationSummary Control");
            _validationSummary = (System.Web.UI.MobileControls.ValidationSummary) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Gets the HTML to be used for the design time representation of the control runtime.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The design time HTML.
        ///    </para>
        /// </returns>
        protected override String GetDesignTimeNormalHtml()
        {
            DesignerTextWriter tw = new DesignerTextWriter();
            _validationSummary.Adapter.Render(tw);

            return tw.ToString();
        }
    }
}
