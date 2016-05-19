//------------------------------------------------------------------------------
// <copyright file="Command.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile Command class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\Command.uex' path='docs/doc[@for="Command"]/*' />
    [
        DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
        DefaultEvent("Click"),
        DefaultProperty("Text"),
        Designer(typeof(System.Web.UI.Design.MobileControls.CommandDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerCommandAdapter)),
        ToolboxData("<{0}:Command runat=\"server\">Command</{0}:Command>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Command : TextControl, IPostBackEventHandler, IPostBackDataHandler
    {
        private static readonly Object EventClick = new Object ();
        private static readonly Object EventItemCommand = new Object ();

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e) 
        {
            base.OnPreRender(e);
            // If this control will be rendered as an image

            Debug.Assert(ImageUrl != null);
            if (MobilePage != null
                && ImageUrl.Length != 0
                && MobilePage.Device.SupportsImageSubmit)
            {
                // HTML input controls of type image postback as name.x and
                // name.y which is not associated with this control by default
                // in Page.ProcessPostData().
                MobilePage.RegisterRequiresPostBack(this);
            }
        }
        
        /// <internalonly/>
        protected bool LoadPostData(String key, NameValueCollection data)
        {
            bool dataChanged;
            bool handledByAdapter =
                Adapter.LoadPostData(key, data, null, out dataChanged);

            // If the adapter handled the post back and set dataChanged this
            // was an image button (responds with ID.x and ID.y).
            if (handledByAdapter)
            {
                if(dataChanged)
                {
                    Page.RegisterRequiresRaiseEvent(this);
                }
            }
            // Otherwise if the adapter did not handle the past back, use
            // the same method as Page.ProcessPostData().
            else if(data[key] != null)
            {
                Page.RegisterRequiresRaiseEvent(this);
            }
            return false;  // no need to raise PostDataChangedEvent.
        }

        /// <internalonly/>
        protected void RaisePostDataChangedEvent() {
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.SoftkeyLabel"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Command_SoftkeyLabel)
        ]
        public String SoftkeyLabel
        {
            get
            {
                String s = (String) ViewState["Softkeylabel"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["Softkeylabel"] = value;
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.CommandName"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Command_CommandName)
        ]
        public String CommandName
        {
            get
            {
                String s = (String) ViewState["CommandName"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["CommandName"] = value;
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.CommandArgument"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Command_CommandArgument)
        ]
        public String CommandArgument
        {
            get
            {
                String s = (String) ViewState["CommandArgument"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["CommandArgument"] = value;
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.ImageUrl"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            Editor(typeof(System.Web.UI.Design.MobileControls.ImageUrlEditor),
                   typeof(UITypeEditor)),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Image_ImageUrl)
        ]
        public String ImageUrl
        {
            get
            {
                String s = (String) ViewState["ImageUrl"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["ImageUrl"] = value;
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.CausesValidation"]/*' />
        [
            Bindable(false),
            DefaultValue(true),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Command_CausesValidation)
        ]
        public bool CausesValidation
        {
            get
            {
                object b = ViewState["CausesValidation"];
                return((b == null) ? true : (bool)b);
            }
            set
            {
                ViewState["CausesValidation"] = value;
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.Format"]/*' />
        [
            Bindable(true),
            DefaultValue(CommandFormat.Button),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Command_Format)
        ]
        public CommandFormat Format
        {
            get
            {
                Object o = ViewState["Format"];
                return((o == null) ? CommandFormat.Button : (CommandFormat)o);
            }
            set
            {
                ViewState["Format"] = value;
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.OnClick"]/*' />
        protected virtual void OnClick(EventArgs e)
        {
            EventHandler onClickHandler = (EventHandler)Events[EventClick];
            if (onClickHandler != null)
            {
                onClickHandler(this,e);
            }
        }
        
        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.OnItemCommand"]/*' />
        protected virtual void OnItemCommand(CommandEventArgs e)
        {
            CommandEventHandler onItemCommandHandler = (CommandEventHandler)Events[EventItemCommand];
            if (onItemCommandHandler != null)
            {
                onItemCommandHandler(this,e);
            }

            RaiseBubbleEvent (this, e);
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.Click"]/*' />
        [        
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.Command_OnClick)
        ]
        public event EventHandler Click
        {
            add 
            {
                Events.AddHandler(EventClick, value);
            }
            remove 
            {
                Events.RemoveHandler(EventClick, value);
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.ItemCommand"]/*' />
        [        
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.Command_OnItemCommand)
        ]
        public event CommandEventHandler ItemCommand
        {
            add 
            {
                Events.AddHandler(EventItemCommand, value);
            }
            remove 
            {
                Events.RemoveHandler(EventItemCommand, value);
            }
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.IPostBackEventHandler.RaisePostBackEvent"]/*' />
        /// <internalonly/>
        protected void RaisePostBackEvent(String argument)
        {
            if (CausesValidation)
            {
                MobilePage.Validate();
            }

            // It is legitimate to reset the form back to the first page
            // after a form submit.
            Form.CurrentPage = 1;

            OnClick (EventArgs.Empty);
            OnItemCommand (new CommandEventArgs(CommandName, CommandArgument));
        }

        /// <include file='doc\Command.uex' path='docs/doc[@for="Command.IsFormSubmitControl"]/*' />
        protected override bool IsFormSubmitControl()
        {
            return true;
        }

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(String eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion 

        #region IPostBackDataHandler implementation
        bool IPostBackDataHandler.LoadPostData(String key, NameValueCollection data) {
            return LoadPostData(key, data);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }
        #endregion
    }
}
