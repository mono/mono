//------------------------------------------------------------------------------
// <copyright file="BaseTemplatedMobileComponentEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    /// <summary>
    ///    <para>
    ///       Provides the
    ///       base component editor for Mobile Templated controls.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class BaseTemplatedMobileComponentEditor : WindowsFormsComponentEditor
    {
        private int _initialPage;

        /// <summary>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.Web.UI.Design.MobileControls.BaseTemplatedMobileComponentEditor'/>.
        ///    </para>
        /// </summary>
        /// <param name='initialPage'>
        ///    The index of the initial page.
        /// </param>
        internal BaseTemplatedMobileComponentEditor(int initialPage)
        {
            this._initialPage = initialPage;
        }

        /// <summary>
        ///    <para>
        ///       Edits a component.
        ///    </para>
        /// </summary>
        /// <param name='parent'>
        ///    The <see cref='System.Windows.Forms.IWin32Window'/> parent.
        /// </param>
        /// <param name='context'>
        /// </param>
        /// <param name=' obj'>
        ///    The component to edit.
        /// </param>
        public override bool EditComponent(ITypeDescriptorContext context, Object obj, IWin32Window parent)
        {
            bool result = false;
            bool inTemplateMode = false;

            Debug.Assert(obj is IComponent, "Expected obj to be an IComponent");
            IComponent comp = (IComponent)obj;
            ISite compSite = comp.Site;

            if (compSite != null)
            {
                IDesignerHost designerHost = (IDesignerHost)compSite.GetService(typeof(IDesignerHost));

                IDesigner compDesigner = designerHost.GetDesigner(comp);
                Debug.Assert(compDesigner is TemplatedControlDesigner,
                             "Expected component to have a TemplatedControlDesigner");

                TemplatedControlDesigner tplDesigner = (TemplatedControlDesigner) compDesigner;
                inTemplateMode = tplDesigner.InTemplateMode;
            }
            
            if (inTemplateMode == false)
            {
                result = base.EditComponent(context, obj, parent);
            }
            else
            {
                MessageBox.Show(SR.GetString(SR.BaseTemplatedMobileComponentEditor_TemplateModeErrorMessage), 
                                SR.GetString(SR.BaseTemplatedMobileComponentEditor_TemplateModeErrorTitle),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return result;
        }

        /// <summary>
        ///    <para>
        ///       Gets the index of the initial component editor page.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The index of the initial page.
        ///    </para>
        /// </returns>
        protected override int GetInitialComponentEditorPageIndex()
        {
            return _initialPage;
        }
    }
}

