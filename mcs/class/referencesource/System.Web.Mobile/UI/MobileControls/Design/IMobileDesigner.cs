//------------------------------------------------------------------------------
// <copyright file="IMobileDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{

    /// <include file='doc\IMobileDesigner.uex' path='docs/doc[@for="IMobileDesigner"]/*' />
    /// <summary>
    /// <para>
    /// Provides a contract for all mobile control designers.
    /// </para>
    /// </summary>
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public interface IMobileDesigner
    {
        /// <include file='doc\IMobileDesigner.uex' path='docs/doc[@for="IMobileDesigner.UpdateRendering"]/*' />
        void UpdateRendering();
    }
}
