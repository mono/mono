//------------------------------------------------------------------------------
// <copyright file="StyleSheetRefUrlEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Web.UI.Design;

namespace System.Web.UI.Design.MobileControls
{
    /// <summary>
    ///   Provides an editor for visually picking an ASCX Url.
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class StyleSheetRefUrlEditor: UrlEditor
    {
        /// <summary>
        ///    Caption of the editor UI
        /// </summary>
        protected override String Caption
        {
            get
            {
                return SR.GetString(SR.StyleSheetRefUrlEditor_Caption);
            }
        }

        /// <summary>
        ///    Filter used by the editor
        /// </summary>
        protected override String Filter
        {
            get
            {
                return SR.GetString(SR.StyleSheetRefUrlEditor_Filter);
            }
        }

        /// <summary>
        ///   NoAbsolute = Don't allow absoulte urls.
        ///   None = Otherwise.
        /// </summary>
        protected override UrlBuilderOptions Options 
        {
            get 
            {
                return UrlBuilderOptions.NoAbsolute;
            }
        }
    }
}
    
