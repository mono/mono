//------------------------------------------------------------------------------
// <copyright file="PersistNameAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.MobileControls 
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;

    /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute"]/*' />
    [
        AttributeUsage(AttributeTargets.Class)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class PersistNameAttribute : Attribute 
    {
        /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute.Default"]/*' />
        public static readonly PersistNameAttribute Default = new PersistNameAttribute(String.Empty);

        private String _name = String.Empty;

        /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute.Name"]/*' />
        public String Name 
        {
            get
            {
                return this._name;
            }
        }

        /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute.PersistNameAttribute"]/*' />
        public PersistNameAttribute(String name)
        {
            this._name = name;
        }

        /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute.Equals"]/*' />
        public override bool Equals(Object obj)
        {
            if (obj == this) 
            {
                return true;
            }

            if ((obj != null) && (obj is PersistNameAttribute)) 
            {
                return(String.Compare(((PersistNameAttribute)obj).Name, _name, StringComparison.OrdinalIgnoreCase) == 0);
            }

            return false;
        }

        /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute.GetHashCode"]/*' />
        public override int GetHashCode()
        {
            return _name.ToLower(CultureInfo.InvariantCulture).GetHashCode();
        }

        /// <include file='doc\PersistNameAttribute.uex' path='docs/doc[@for="PersistNameAttribute.IsDefaultAttribute"]/*' />
        public override bool IsDefaultAttribute() 
        {
            return this.Equals(Default);
        }
    }
}
