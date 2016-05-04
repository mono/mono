//------------------------------------------------------------------------------
// <copyright file="ObjectListGeneralPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Globalization;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using Control = System.Windows.Forms.Control;
    using Label = System.Windows.Forms.Label;
    using CheckBox = System.Windows.Forms.CheckBox;
    using TextBox = System.Windows.Forms.TextBox;
    using ComboBox = System.Windows.Forms.ComboBox;
    using DataBinding = System.Web.UI.DataBinding;

    /// <summary>
    ///   The General page for the ObjectList control.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class ObjectListGeneralPage : MobileComponentEditorPage
    {
        private TextBox _txtBackCommandText;
        private TextBox _txtDetailsCommandText;
        private TextBox _txtMoreText;
        private TextBox _txtItemCount;
        private TextBox _txtItemsPerPage;

        protected override String HelpKeyword 
        {
            get 
            {
                return "net.Mobile.ObjectListProperties.General";
            }
        }

        private void InitForm()
        {
            GroupLabel grplblAppearance = new GroupLabel();
            grplblAppearance.SetBounds(4, 4, 392, 16);
            grplblAppearance.Text = SR.GetString(SR.ObjectListGeneralPage_AppearanceGroupLabel);
            grplblAppearance.TabIndex = 2;
            grplblAppearance.TabStop = false;

            Label lblBackCommandText = new Label();
            lblBackCommandText.SetBounds(12, 24, 174, 16);
            lblBackCommandText.Text = SR.GetString(SR.ObjectListGeneralPage_BackCommandTextCaption);
            lblBackCommandText.TabStop = false;
            lblBackCommandText.TabIndex = 3;

            _txtBackCommandText = new TextBox();
            _txtBackCommandText.SetBounds(12, 40, 154, 20);
            _txtBackCommandText.TabIndex = 4;
            _txtBackCommandText.TextChanged += new EventHandler(this.OnSetPageDirty);

            Label lblDetailsCommandText = new Label();
            lblDetailsCommandText.SetBounds(206, 24, 174, 16);
            lblDetailsCommandText.Text = SR.GetString(SR.ObjectListGeneralPage_DetailsCommandTextCaption);
            lblDetailsCommandText.TabStop = false;
            lblDetailsCommandText.TabIndex = 5;

            _txtDetailsCommandText = new TextBox();
            _txtDetailsCommandText.SetBounds(206, 40, 154, 20);
            _txtDetailsCommandText.TabIndex = 6;
            _txtDetailsCommandText.TextChanged += new EventHandler(this.OnSetPageDirty);

            Label lblMoreText = new Label();
            lblMoreText.SetBounds(12, 67, 174, 16);
            lblMoreText.Text = SR.GetString(SR.ObjectListGeneralPage_MoreTextCaption);
            lblMoreText.TabStop = false;
            lblMoreText.TabIndex = 7;

            _txtMoreText = new TextBox();
            _txtMoreText.SetBounds(12, 83, 154, 20);
            _txtMoreText.TabIndex = 8;
            _txtMoreText.TextChanged += new EventHandler(this.OnSetPageDirty);

            GroupLabel pagingGroup = new GroupLabel();
            Label itemCountLabel = new Label();
            _txtItemCount = new TextBox();

            Label itemsPerPageLabel = new Label();
            _txtItemsPerPage = new TextBox();

            pagingGroup.SetBounds(4, 118, 392, 16);
            pagingGroup.Text = SR.GetString(SR.ListGeneralPage_PagingGroupLabel);
            pagingGroup.TabIndex = 9;
            pagingGroup.TabStop = false;

            itemCountLabel.SetBounds(12, 138, 174, 16);
            itemCountLabel.Text = SR.GetString(SR.ListGeneralPage_ItemCountCaption);
            itemCountLabel.TabStop = false;
            itemCountLabel.TabIndex = 10;

            _txtItemCount.SetBounds(12, 154, 154, 20);
            _txtItemCount.TextChanged += new EventHandler(this.OnSetPageDirty);
            _txtItemCount.KeyPress += new KeyPressEventHandler(this.OnKeyPressNumberTextBox);
            _txtItemCount.TabIndex = 11;

            itemsPerPageLabel.SetBounds(206, 138, 174, 16);
            itemsPerPageLabel.Text = SR.GetString(SR.ListGeneralPage_ItemsPerPageCaption);
            itemsPerPageLabel.TabStop = false;
            itemsPerPageLabel.TabIndex = 12;

            _txtItemsPerPage.SetBounds(206, 154, 154, 20);
            _txtItemsPerPage.TextChanged += new EventHandler(this.OnSetPageDirty);
            _txtItemsPerPage.KeyPress += new KeyPressEventHandler(this.OnKeyPressNumberTextBox);
            _txtItemsPerPage.TabIndex = 13;

            this.Text = SR.GetString(SR.ObjectListGeneralPage_Title);
            this.Size = new Size(402, 300);
            this.CommitOnDeactivate = true;
            this.Icon = new Icon(
                typeof(System.Web.UI.Design.MobileControls.MobileControlDesigner),
                "General.ico"
            );

            this.Controls.AddRange(new Control[]
                           {
                                grplblAppearance,
                                lblBackCommandText,
                                _txtBackCommandText,
                                lblDetailsCommandText,
                                _txtDetailsCommandText,
                                lblMoreText,
                                _txtMoreText,
                                pagingGroup,
                                itemCountLabel,
                                _txtItemCount,
                                itemsPerPageLabel,
                                _txtItemsPerPage
                           });
        }

        /// <summary>
        ///   Loads the component into the page.
        /// </summary>
        protected override void LoadComponent() 
        {
            ObjectList objectList = (ObjectList)GetBaseControl();

            _txtItemCount.Text = objectList.ItemCount.ToString(CultureInfo.InvariantCulture);
            _txtItemsPerPage.Text = objectList.ItemsPerPage.ToString(CultureInfo.InvariantCulture);
            _txtBackCommandText.Text = objectList.BackCommandText;
            _txtDetailsCommandText.Text = objectList.DetailsCommandText;
            _txtMoreText.Text = objectList.MoreText;
        }

        private void OnSetPageDirty(Object source, EventArgs e) 
        {
            if (IsLoading())
            {
                return;
            }
            SetDirty();
        }

        private void OnKeyPressNumberTextBox(Object source, KeyPressEventArgs e)
        {
            if (!((e.KeyChar >='0' && e.KeyChar <= '9') ||
                e.KeyChar == 8))
            {
                e.Handled = true;
                SafeNativeMethods.MessageBeep(unchecked((int)0xFFFFFFFF));
            }
        }

        /// <summary>
        ///   Saves the component loaded into the page.
        /// </summary>
        protected override void SaveComponent() 
        {
            ObjectList objectList = (ObjectList)GetBaseControl();
            ObjectListDesigner objectListDesigner = (ObjectListDesigner)GetBaseDesigner();

            try
            {
                int itemCount = 0;

                if (_txtItemCount.Text.Length != 0)
                {
                    itemCount = Int32.Parse(_txtItemCount.Text, CultureInfo.InvariantCulture);
                }
                objectList.ItemCount = itemCount;
            }
            catch (Exception)
            {
                _txtItemCount.Text = objectList.ItemCount.ToString(CultureInfo.InvariantCulture);
            }

            try
            {
                int itemsPerPage = 0;

                if (_txtItemsPerPage.Text.Length != 0)
                {
                    itemsPerPage = Int32.Parse(_txtItemsPerPage.Text, CultureInfo.InvariantCulture);
                }
                objectList.ItemsPerPage = itemsPerPage;
            }
            catch (Exception)
            {
                _txtItemsPerPage.Text = objectList.ItemsPerPage.ToString(CultureInfo.InvariantCulture);
            }

            objectList.BackCommandText = _txtBackCommandText.Text;
            objectList.DetailsCommandText = _txtDetailsCommandText.Text;
            objectList.MoreText = _txtMoreText.Text;

            TypeDescriptor.Refresh(objectList);
        }

        /// <summary>
        ///   Sets the component that is to be edited in the page.
        /// </summary>
        public override void SetComponent(IComponent component) 
        {
            base.SetComponent(component);
            InitForm();
        }
    }
}
