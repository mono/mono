//------------------------------------------------------------------------------
// <copyright file="Panel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;                    
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel"]/*' />
    [
        ControlBuilderAttribute(typeof(PanelControlBuilder)),
        Designer(typeof(System.Web.UI.Design.MobileControls.PanelDesigner)),
        DesignerAdapter(typeof(System.Web.UI.MobileControls.Adapters.HtmlPanelAdapter)),
        PersistChildren(true),
        ToolboxData("<{0}:Panel runat=\"server\"></{0}:Panel>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]    
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Panel : MobileControl, ITemplateable
    {
        Panel _deviceSpecificContents = null;

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.Panel"]/*' />
        public Panel() : base()
        {
            Form frm = this as Form;
            if(frm == null)
            {
                _breakAfter = false;
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.AddLinkedForms"]/*' />
        public override void AddLinkedForms(IList linkedForms)
        {
            foreach (Control child in Controls)
            {
                if (child is MobileControl)
                {
                    ((MobileControl)child).AddLinkedForms(linkedForms);
                }
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.BreakAfter"]/*' />
        [
            DefaultValue(false),
        ]
        public override bool BreakAfter
        {
            get
            {
                return base.BreakAfter;
            }

            set
            {
                base.BreakAfter = value;
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.PaginateChildren"]/*' />
        protected override bool PaginateChildren
        {
            get
            {
                return Paginate;
            }
        }

        private bool _paginationStateChanged = false;
        internal bool PaginationStateChanged
        {
            get
            {
                return _paginationStateChanged;
            }
            set
            {
                _paginationStateChanged = value;
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.Paginate"]/*' />
        [
          Bindable(true),
          DefaultValue(false),
          MobileCategory(SR.Category_Behavior),
          MobileSysDescription(SR.Panel_Paginate)
        ]
        public virtual bool Paginate
        {
            get
            {
                Object o = ViewState["Paginate"];
                return o != null ? (bool)o : false;
            }

            set
            {
                bool wasPaginating = Paginate;
                ViewState["Paginate"] = value;
                if (IsTrackingViewState)
                {
                    PaginationStateChanged = true;
                    if (value == false && wasPaginating == true )
                    {
                        SetControlPageRecursive(this, 1);
                    }
                }
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.OnInit"]/*' />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (IsTemplated)
            {
                ClearChildViewState();
                CreateTemplatedUI(false);
                ChildControlsCreated = true;
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.PaginateRecursive"]/*' />
        public override void PaginateRecursive(ControlPager pager)
        {
            if (!EnablePagination)
            {
                return;
            }

            if (Paginate && Content != null)
            {
                Content.Paginate = true;
                Content.PaginateRecursive(pager);
                this.FirstPage = Content.FirstPage;
                this.LastPage = pager.PageCount;
            }
            else
            {
                base.PaginateRecursive(pager);
            }        
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.CreateDefaultTemplatedUI"]/*' />
        public override void CreateDefaultTemplatedUI(bool doDataBind)
        {
            ITemplate contentTemplate = GetTemplate(Constants.ContentTemplateTag);
            if (contentTemplate != null)
            {
                _deviceSpecificContents = new TemplateContainer();
                CheckedInstantiateTemplate (contentTemplate, _deviceSpecificContents, this);
                Controls.AddAt(0, _deviceSpecificContents);
            }
        }

        /// <include file='doc\Panel.uex' path='docs/doc[@for="Panel.Content"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Panel Content
        {
            get
            {
                return _deviceSpecificContents;
            }
        }
    }

    /*
     * Control builder for panels. 
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\Panel.uex' path='docs/doc[@for="PanelControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class PanelControlBuilder : LiteralTextContainerControlBuilder
    {
    }
}
