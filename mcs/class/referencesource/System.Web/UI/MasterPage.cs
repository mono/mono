//------------------------------------------------------------------------------
// <copyright file="MasterPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * MasterPage class definition
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Web.Compilation;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class MasterPageControlBuilder : UserControlControlBuilder {
        internal static readonly String AutoTemplatePrefix = "Template_";
    }


    /// <devdoc>
    ///    Default ControlBuilder used to parse master page files. Note that this class inherits from FileLevelPageControlBuilder
    ///    to reuse masterpage handling code.
    /// </devdoc>
    public class FileLevelMasterPageControlBuilder: FileLevelPageControlBuilder {

        internal override void AddContentTemplate(object obj, string templateName, ITemplate template) {
            MasterPage master = (MasterPage)obj;
            master.AddContentTemplate(templateName, template);
        }
    }


    /// <devdoc>
    ///    <para>This class is not marked as abstract, because the VS designer
    ///          needs to instantiate it when opening .master files</para>
    /// </devdoc>
    [
    Designer("Microsoft.VisualStudio.Web.WebForms.MasterPageWebFormDesigner, " + AssemblyRef.MicrosoftVisualStudioWeb, typeof(IRootDesigner)),
    ControlBuilder(typeof(MasterPageControlBuilder)),
    ParseChildren(false)
    ]
    public class MasterPage : UserControl {

        private VirtualPath _masterPageFile;
        private MasterPage _master;

        // The collection used to store the templates created on the content page.
        private IDictionary _contentTemplates;

        // The collection used to store the content templates defined in MasterPage.
        private IDictionary _contentTemplateCollection;
        private IList _contentPlaceHolders;

        private bool _masterPageApplied;

        // The page or masterpage that hosts this masterPage.
        internal TemplateControl _ownerControl;


        public MasterPage() {
        }


        /// <devdoc>
        ///    <para>Dictionary used to store the content templates that are passed in from the content pages.</para>
        /// </devdoc>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal IDictionary ContentTemplates {
            get {
                return _contentTemplates;
            }
        }


        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal IList ContentPlaceHolders {
            get {
                if (_contentPlaceHolders == null) {
                    _contentPlaceHolders = new ArrayList();
                }

                return _contentPlaceHolders;
            }
        }


        /// <devdoc>
        ///    <para>The MasterPage used by this nested MasterPage control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.MasterPage_MasterPage)
        ]
        public MasterPage Master {
            get {
                if (_master == null && !_masterPageApplied) {
                    _master = MasterPage.CreateMaster(this, Context, _masterPageFile, _contentTemplateCollection);
                }

                return _master;
            }
        }


        /// <devdoc>
        ///    <para>Gets and sets the masterPageFile of this control.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.MasterPage_MasterPageFile)
        ]
        public string MasterPageFile {
            get {
                return VirtualPath.GetVirtualPathString(_masterPageFile);
            }
            set {
                if (_masterPageApplied) {
                    throw new InvalidOperationException(SR.GetString(SR.PropertySetBeforePageEvent, "MasterPageFile", "Page_PreInit"));
                }

                if (value != VirtualPath.GetVirtualPathString(_masterPageFile)) {
                    _masterPageFile = VirtualPath.CreateAllowNull(value);

                    if (_master != null && Controls.Contains(_master)) {
                        Controls.Remove(_master);
                    }
                    _master = null;
                }
            }
        }


        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal void AddContentTemplate(string templateName, ITemplate template) {
            if (_contentTemplateCollection == null) {
                _contentTemplateCollection = new Hashtable(10, StringComparer.OrdinalIgnoreCase);
            }

            try {
                _contentTemplateCollection.Add(templateName, template);
            } 
            catch (ArgumentException) {
                throw new HttpException(SR.GetString(SR.MasterPage_Multiple_content, templateName));
            }
        }

        internal static MasterPage CreateMaster(TemplateControl owner, HttpContext context,
            VirtualPath masterPageFile, IDictionary contentTemplateCollection) {

            Debug.Assert(owner is MasterPage || owner is Page);

            MasterPage master = null;

            if (masterPageFile == null) {
                if (contentTemplateCollection != null && contentTemplateCollection.Count > 0) {
                    throw new HttpException(SR.GetString(SR.Content_only_allowed_in_content_page));
                }
 
                return null;
            }

            // If it's relative, make it *app* relative.  Treat is as relative to this
            // user control (ASURT 55513)
            VirtualPath virtualPath = VirtualPathProvider.CombineVirtualPathsInternal(
                owner.TemplateControlVirtualPath, masterPageFile);

            // Compile the declarative control and get its Type
            ITypedWebObjectFactory result = (ITypedWebObjectFactory)BuildManager.GetVPathBuildResult(
                context, virtualPath);

            // Make sure it has the correct base type
            if (!typeof(MasterPage).IsAssignableFrom(result.InstantiatedType)) {
                throw new HttpException(SR.GetString(SR.Invalid_master_base,
                    masterPageFile));
            }

            master = (MasterPage)result.CreateInstance();
            
            master.TemplateControlVirtualPath = virtualPath;

            if (owner.HasControls()) {
                foreach (Control control in owner.Controls) {
                    LiteralControl literal = control as LiteralControl;
                    if (literal == null || Util.FirstNonWhiteSpaceIndex(literal.Text) >= 0) {
                        throw new HttpException(SR.GetString(SR.Content_allowed_in_top_level_only));
                    }
                }

                // Remove existing controls.
                owner.Controls.Clear();
            }

            // Make sure the control collection is writable.
            if (owner.Controls.IsReadOnly) {
                throw new HttpException(SR.GetString(SR.MasterPage_Cannot_ApplyTo_ReadOnly_Collection));
            }

            if (contentTemplateCollection != null) {
                foreach(String contentName in contentTemplateCollection.Keys) {
                    if (!master.ContentPlaceHolders.Contains(contentName.ToLower(CultureInfo.InvariantCulture))) {
                        throw new HttpException(SR.GetString(SR.MasterPage_doesnt_have_contentplaceholder, contentName, masterPageFile));
                    }
                }
                master._contentTemplates = contentTemplateCollection;
            }

            master._ownerControl = owner;
            master.InitializeAsUserControl(owner.Page);
            owner.Controls.Add(master);
            return master;
        }

        internal static void ApplyMasterRecursive(MasterPage master, IList appliedMasterFilePaths) {

            Debug.Assert(appliedMasterFilePaths != null);

            // Recursively apply master pages to the nested masterpages.
            if (master.Master != null) {

                string pageFile = master._masterPageFile.VirtualPathString.ToLower(CultureInfo.InvariantCulture);
                if (appliedMasterFilePaths.Contains(pageFile)) {
                    throw new InvalidOperationException(SR.GetString(SR.MasterPage_Circular_Master_Not_Allowed, master._masterPageFile));
                }

                appliedMasterFilePaths.Add(pageFile);

                ApplyMasterRecursive(master.Master, appliedMasterFilePaths);
            }

            master._masterPageApplied = true;
        }

        public void InstantiateInContentPlaceHolder(Control contentPlaceHolder, ITemplate template) {
            HttpContext context = HttpContext.Current;

            // Remember the old TemplateControl
            TemplateControl oldControl = context.TemplateControl;

            // Storing the template control into the context
            // since each thread needs to set it differently.
            context.TemplateControl = _ownerControl;

            try {
                // Instantiate the template using the correct TemplateControl
                template.InstantiateIn(contentPlaceHolder);
            }
            finally {
                // Revert back to the old templateControl
                context.TemplateControl = oldControl;
            }
        }
    }
}
