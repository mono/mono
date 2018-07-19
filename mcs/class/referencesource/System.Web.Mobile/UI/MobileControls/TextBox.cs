//------------------------------------------------------------------------------
// <copyright file="TextBox.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile TextBox class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox"]/*' />
    [
        ControlBuilderAttribute(typeof(TextBoxControlBuilder)),
        DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
        DefaultEvent("TextChanged"),
        DefaultProperty("Text"),
        Designer(typeof(System.Web.UI.Design.MobileControls.TextBoxDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerTextBoxAdapter)),
        ToolboxData("<{0}:TextBox runat=\"server\"></{0}:TextBox>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign),
        ValidationProperty("Text")
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class TextBox : TextControl, IPostBackDataHandler
    {
        private static readonly Object EventTextChanged = new Object();

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.IPostBackDataHandler.LoadPostData"]/*' />
        /// <internalonly/>
        protected bool LoadPostData(String key, NameValueCollection data)
        {
            bool dataChanged = false;
            bool handledByAdapter =
                Adapter.LoadPostData(key, data, null, out dataChanged);

            if (!handledByAdapter)
            {
                String value = data[key];
                if (Text != value)
                {
                    Text = value;
                    dataChanged = true;    // this will cause a RaisePostDataChangedEvent()
                }
            }

            return dataChanged;
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.IPostBackDataHandler.RaisePostDataChangedEvent"]/*' />
        /// <internalonly/>
        protected void RaisePostDataChangedEvent()
        {
            OnTextChanged(EventArgs.Empty);
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.OnTextChanged"]/*' />
        protected virtual void OnTextChanged(EventArgs e)
        {
            EventHandler onTextChangedHandler = (EventHandler)Events[EventTextChanged];
            if (onTextChangedHandler != null)
            {
                onTextChangedHandler(this,e);
            }
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.Password"]/*' />
        [
            Bindable(true),
            Browsable(true),
            DefaultValue(false),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.TextBox_Password)
        ]
        public bool Password
        {
            get
            {
                Object b = ViewState["Password"];
                return (b != null) ? (bool)b : false;
            }
            set
            {
                ViewState["Password"] = value;
            }
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.Numeric"]/*' />
        [
            Bindable(true),
            Browsable(true),
            DefaultValue(false),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.TextBox_Numeric)
        ]
        public bool Numeric
        {
            get
            {
                Object b = ViewState["Numeric"];
                return (b != null) ? (bool)b : false;
            }
            set
            {
                ViewState["Numeric"] = value;
            }
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.Size"]/*' />
        [
            Bindable(true),
            DefaultValue(0),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.TextBox_Size)
        ]
        public int Size
        {
            get
            {
                Object i = ViewState["Size"];
                return((i != null) ? (int)i : 0);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Size", value,
                        SR.GetString(SR.TextBox_NotNegativeNumber));
                }
                ViewState["Size"] = value;
            }
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.MaxLength"]/*' />
        [
            Bindable(true),
            DefaultValue(0),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.TextBox_MaxLength)
        ]
        public int MaxLength
        {
            get
            {
                Object i = ViewState["MaxLength"];
                return((i != null) ? (int)i : 0);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MaxLength", value,
                        SR.GetString(SR.TextBox_NotNegativeNumber));
                }
                ViewState["MaxLength"] = value;
            }
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.TextChanged"]/*' />
        [
            MobileSysDescription(SR.TextBox_OnTextChanged)
        ]
        public event EventHandler TextChanged
        {
            add
            {
                Events.AddHandler(EventTextChanged, value);
            }
            remove
            {
                Events.RemoveHandler(EventTextChanged, value);
            }
        }

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBox.Title"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Input_Title)
        ]
        public String Title
        {
            get
            {
                return ToString(ViewState["Title"]);
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        internal override bool TrimInnerText
        {
            get
            {
                return false;
            }
        }

        internal override bool TrimNewlines
        {
            get
            {
                return true;
            }
        }

        #region IPostBackDataHandler implementation
        bool IPostBackDataHandler.LoadPostData(String key, NameValueCollection data) {
            return LoadPostData(key, data);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }
        #endregion 
    }

    /*
     * TextBox Control Builder
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBoxControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class TextBoxControlBuilder : MobileControlBuilder
    {
        // Textbox allows whitespace inside text.

        /// <include file='doc\TextBox.uex' path='docs/doc[@for="TextBoxControlBuilder.AllowWhitespaceLiterals"]/*' />
        public override bool AllowWhitespaceLiterals()
        {
            return true;
        }
    }

}
