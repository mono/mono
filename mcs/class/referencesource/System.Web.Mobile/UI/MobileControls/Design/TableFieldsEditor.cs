//------------------------------------------------------------------------------
// <copyright file="TableFieldsEditor.cs" company="Microsoft">
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
    using System.Web.UI.MobileControls;

    /// <summary>
    ///    <para>
    ///       The editor for column collections.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class TableFieldsEditor : MobileUITypeEditor 
    {
        /// <summary>
        ///    <para>
        ///       Edits the value specified.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    An <see cref='System.ComponentModel.ITypeDescriptorContext'/> that specifies the context of the value to edit.
        /// </param>
        /// <param name=' provider'>
        ///    An <see cref='System.IServiceProvider'/> .
        /// </param>
        /// <param name=' value'>
        ///    The object to edit.
        /// </param>
        /// <returns>
        ///    <para>
        ///       The updated value.
        ///    </para>
        /// </returns>
        public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value) 
        {
            Debug.Assert(context != null);

            ObjectList objectList = context.Instance as ObjectList;
            Debug.Assert(objectList != null);

            ObjectListDesigner designer = GetDesigner(context) as ObjectListDesigner;
            Debug.Assert(designer != null);

            designer.InvokePropertyBuilder(ObjectListComponentEditor.IDX_GENERAL);

            return objectList.TableFields;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Disable the ... button in multi-selected case.
            if (context.Instance is ObjectList)
            {
                return base.GetEditStyle(context);
            }
            return UITypeEditorEditStyle.None;
        }
    }
}

