//------------------------------------------------------------------------------
// <copyright file="ChtmlCommandAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * ChtmlCommandAdapter class.
     */
    /// <include file='doc\ChtmlCommandAdapter.uex' path='docs/doc[@for="ChtmlCommandAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ChtmlCommandAdapter : HtmlCommandAdapter
    {
        /// <include file='doc\ChtmlCommandAdapter.uex' path='docs/doc[@for="ChtmlCommandAdapter.RequiresFormTag"]/*' />
        public override bool RequiresFormTag
        {
            get
            {
                return true;
            }
        }

        /// <include file='doc\ChtmlCommandAdapter.uex' path='docs/doc[@for="ChtmlCommandAdapter.AddAttributes"]/*' />
        protected override void AddAttributes(HtmlMobileTextWriter writer)
        {
            AddAccesskeyAttribute(writer);
            AddJPhoneMultiMediaAttributes(writer);
        }
    }
}
