//------------------------------------------------------------------------------
// <copyright file="UnsettableComboBox.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    //   Standard combobox with a "Not Set" item as the first item in its dropdown.
    //   It also automatically blanks out the "Not Set" item on losing focus.
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class UnsettableComboBox : ComboBox 
    {
        private String notSetText;
        private String notSetCompactText;

        internal UnsettableComboBox() 
        {
            notSetText = SR.GetString(SR.UnsettableComboBox_NotSetText);
            notSetCompactText = SR.GetString(SR.UnsettableComboBox_NotSetCompactText);
        }

        internal String NotSetText 
        {
            get 
            {
                return notSetText;
            }
            set 
            {
                notSetText = value;
            }
        }

        internal String NotSetCompactText 
        {
            get 
            {
                return notSetCompactText;
            }
            set 
            {
                notSetCompactText = value;
            }
        }

        public override String Text 
        {
            get 
            {
                // handle DropDown styles in Templating Options dialog
                // if (this.SelectedIndex == 0) || (this.SelectedIndex == -1))
                if (this.SelectedIndex == 0)
                {
                    return String.Empty;
                }
                else
                {
                    return base.Text;
                }
            }

            set 
            {
                if (value == notSetCompactText)
                {
                    base.Text = String.Empty;
                }
                else
                {
                    base.Text = value;
                }
            }
        }

        internal void AddItem(Object item) 
        {
            EnsureNotSetItem();
            Items.Add(item);
        }

        internal void EnsureNotSetItem() 
        {
            if (Items.Count == 0) 
            {
                Items.Add(notSetText);
            }
        }

#if UNUSED_CODE
        internal bool IsSet() 
        {
            return SelectedIndex > 0;
        }
#endif
        protected override void OnLostFocus(EventArgs e) 
        {
            base.OnLostFocus(e);

            if (SelectedIndex == 0)
            {
                SelectedIndex = -1;
            }
        }

        protected override void SetItemsCore(IList values)
        {
            Items.Clear();
            
            if (!DesignMode) 
            {
                Items.Add(notSetText);
            }
            
            // Unfortunately. the interfaces between SetItemsCore and
            // AddItemsCore are mismatched as of 3106.
            ArrayList items = new ArrayList();
            foreach(Object item in values)
            {
                items.Add(item);
            }
            
            base.AddItemsCore(items.ToArray());
        }
    }
}
