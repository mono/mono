//------------------------------------------------------------------------------
// <copyright file="FieldCollectionEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Web.UI.WebControls;
    using System.Runtime.Serialization.Formatters;

    using ObjectList = System.Web.UI.MobileControls.ObjectList;

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
    internal class FieldCollectionEditor : UITypeEditor 
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
            IDesignerHost designerHost = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            Debug.Assert(designerHost != null, "Did not get DesignerHost service.");

            Debug.Assert(context.Instance is ObjectList, "Expected objectlist");
            ObjectList _objectList = (ObjectList)context.Instance;

            ObjectListDesigner _designer = (ObjectListDesigner)designerHost.GetDesigner(_objectList);
            Debug.Assert(_designer != null, "Did not get designer for component");

            _designer.InvokePropertyBuilder(ObjectListComponentEditor.IDX_FIELDS);
            return value;
        }

        /// <summary>
        ///    <para>
        ///       Gets the edit style.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    An <see cref='System.ComponentModel.ITypeDescriptorContext'/> that specifies the associated context.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A <see cref='System.Drawing.Design.UITypeEditorEditStyle'/> that represents the edit style.
        ///    </para>
        /// </returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
