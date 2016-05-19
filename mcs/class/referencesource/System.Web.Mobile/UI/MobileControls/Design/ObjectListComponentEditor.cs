//------------------------------------------------------------------------------
// <copyright file="ObjectListComponentEditor.cs" company="Microsoft">
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
    ///       Provides a component editor for a Mobile ObjectList <see cref='System.Web.UI.MobileControls.ObjectList'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.ObjectList'/>
    /// <seealso cref='System.Web.UI.Design.MobileControls.ObjectListDesigner'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ObjectListComponentEditor : BaseTemplatedMobileComponentEditor 
    {

        // The set of pages used within the ObjectList ComponentEditor
        private static Type[] _editorPages = new Type[]
                                             {
                                                 typeof(ObjectListGeneralPage),
                                                 typeof(ObjectListCommandsPage),
                                                 typeof(ObjectListFieldsPage)
                                             };

        internal const int IDX_GENERAL = 0;
        internal const int IDX_COMMANDS = 1;
        internal const int IDX_FIELDS = 2;

        /// <summary>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.Web.UI.Design.MobileControls.ObjectListComponentEditor'/>.
        ///    </para>
        /// </summary>
        public ObjectListComponentEditor() : this(IDX_GENERAL)
        {
        }

        /// <summary>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.Web.UI.Design.MobileControls.ObjectListComponentEditor'/>.
        ///    </para>
        /// </summary>
        /// <param name='initialPage'>
        ///    The index of the initial page.
        /// </param>
        public ObjectListComponentEditor(int initialPage) : base(initialPage)
        {
        }

        /// <summary>
        ///    <para>
        ///       Gets the set of all pages in the <see cref='System.Web.UI.MobileControls.ObjectList'/>
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
    }
}
