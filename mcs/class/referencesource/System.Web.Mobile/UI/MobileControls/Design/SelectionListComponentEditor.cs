//------------------------------------------------------------------------------
// <copyright file="SelectionListComponentEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms.Design;

    /// <summary>
    ///    <para>
    ///       Provides a component editor for a Mobile SelectionList 
    ///       <see cref='System.Web.UI.MobileControls.SelectionList'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.List'/>
    /// <seealso cref='System.Web.UI.Design.MobileControls.ListDesigner'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class SelectionListComponentEditor : WindowsFormsComponentEditor
    {
        private int _initialPage;

        // The set of pages used within the List ComponentEditor
        private static Type[] _editorPages = new Type[]
                                             {
                                                 typeof(ListGeneralPage),
                                                 typeof(ListItemsPage)
                                             };

        internal const int IDX_GENERAL = 0;
        internal const int IDX_ITEMS = 1;

        /// <summary>
        ///    <para>
        ///       Initializes a new instance of 
        ///       <see cref='System.Web.UI.Design.MobileControls.SelectionListComponentEditor'/>.
        ///    </para>
        /// </summary>
        public SelectionListComponentEditor()
        {
            _initialPage = IDX_GENERAL;
        }

        /// <summary>
        ///    <para>
        ///       Initializes a new instance of 
        ///       <see cref='System.Web.UI.Design.MobileControls.SelectionListComponentEditor'/>.
        ///    </para>
        /// </summary>
        /// <param name='initialPage'>
        ///    The index of the initial page.
        /// </param>
        public SelectionListComponentEditor(int initialPage)
        {
            this._initialPage = initialPage;
        }

        /// <summary>
        ///    <para>
        ///       Gets the set of all pages in the <see cref='System.Web.UI.MobileControls.List'/>
        ///       .
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       An array consisting of the set of component editor pages.
        ///    </para>
        /// </returns>
        /// <remarks>
        ///    <note type="inheritinfo">
        ///       This method may
        ///       be overridden to change the set of pages to show.
        ///    </note>
        /// </remarks>
        protected override Type[] GetComponentEditorPages()
        {
            return _editorPages;
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
