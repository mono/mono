//------------------------------------------------------------------------------
// <copyright file="IMobileWebFormServices.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web.UI.MobileControls;

    /// <include file='doc\IMobileWebFormServices.uex' path='docs/doc[@for="IMobileWebFormServices"]/*' />
    /// <summary>
    /// <para>
    /// Provides a contract for mobile webform designer.
    /// </para>
    /// </summary>
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public interface IMobileWebFormServices
    {
        /// <include file='doc\IMobileWebFormServices.uex' path='docs/doc[@for="IMobileWebFormServices.GetCache"]/*' />
        Object GetCache(String controlID, Object key);
        /// <include file='doc\IMobileWebFormServices.uex' path='docs/doc[@for="IMobileWebFormServices.SetCache"]/*' />
        void SetCache(String controlID, Object key, Object value);
        /// <include file='doc\IMobileWebFormServices.uex' path='docs/doc[@for="IMobileWebFormServices.RefreshPageView"]/*' />
        void RefreshPageView();
        /// <include file='doc\IMobileWebFormServices.uex' path='docs/doc[@for="IMobileWebFormServices.UpdateRenderingRecursive"]/*' />
        void UpdateRenderingRecursive(Control rootControl);
        /// <include file='doc\IMobileWebFormServices.uex' path='docs/doc[@for="IMobileWebFormServices.ClearUndoStack"]/*' />
        void ClearUndoStack();
    }
}
