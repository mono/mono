//------------------------------------------------------------------------------
// <copyright file="DesignerAdapterAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * DesignerAdapter attribute. Can be attached to a control class to 
     * provide a type reference to the adapter that should be used in the
     * designer.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\DesignerAdapterAttribute.uex' path='docs/doc[@for="DesignerAdapterAttribute"]/*' />
    [
        AttributeUsage(AttributeTargets.Class, Inherited=true)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DesignerAdapterAttribute : Attribute 
    {
        private readonly String _typeName;

        /// <include file='doc\DesignerAdapterAttribute.uex' path='docs/doc[@for="DesignerAdapterAttribute.DesignerAdapterAttribute"]/*' />
        public DesignerAdapterAttribute(String adapterTypeName)
        {
            _typeName = adapterTypeName;
        }

        /// <include file='doc\DesignerAdapterAttribute.uex' path='docs/doc[@for="DesignerAdapterAttribute.DesignerAdapterAttribute1"]/*' />
        public DesignerAdapterAttribute(Type adapterType)
        {
            _typeName = adapterType.AssemblyQualifiedName;
        }

        /// <include file='doc\DesignerAdapterAttribute.uex' path='docs/doc[@for="DesignerAdapterAttribute.TypeName"]/*' />
        public virtual String TypeName
        {
            get 
            {
                return _typeName;
            }
        }
    }
}
