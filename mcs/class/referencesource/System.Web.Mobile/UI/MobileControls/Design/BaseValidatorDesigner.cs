//------------------------------------------------------------------------------
// <copyright file="BaseValdiatorDesigner.cs" company="Microsoft">
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
    using System.Web.UI.WebControls;

    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Provides
    ///       a designer for controls derived from ValidatorBase.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class BaseValidatorDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.BaseValidator _baseValidator;

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
            Debug.Assert(component is System.Web.UI.MobileControls.BaseValidator,
                         "BaseValidatorDesigner.Initialize - Invalid BaseValidator Control");
            _baseValidator = (System.Web.UI.MobileControls.BaseValidator) component;
            base.Initialize(component);

            // remove the contained asp validator within mobile validator so that it won't
            // be persisted.
            for (int i = _baseValidator.Controls.Count - 1; i >= 0; i--)
            {
                Control child = _baseValidator.Controls[i];
                if (child is System.Web.UI.WebControls.BaseValidator)
                {
                    _baseValidator.Controls.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Gets the design time HTML of ValidatorBase controls.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The design time
        ///       HTML of the control.
        ///    </para>
        /// </returns>
        protected override String GetDesignTimeNormalHtml()
        {
            Debug.Assert(_baseValidator.Text != null);

            String originalText  = _baseValidator.ErrorMessage;
            ValidatorDisplay validatorDisplay = _baseValidator.Display;
            bool blankText = (validatorDisplay == ValidatorDisplay.None || 
                             (originalText.Trim().Length == 0 && _baseValidator.Text.Trim().Length == 0));
            if (blankText)
            {
                _baseValidator.ErrorMessage = "[" + _baseValidator.ID + "]";
            }

            DesignerTextWriter tw = new DesignerTextWriter();
            _baseValidator.Adapter.Render(tw);

            if (blankText)
            {
                _baseValidator.ErrorMessage = originalText;
            }

            return tw.ToString();
        }
    }
}
