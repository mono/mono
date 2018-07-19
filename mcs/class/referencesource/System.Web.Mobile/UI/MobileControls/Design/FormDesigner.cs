//------------------------------------------------------------------------------
// <copyright file="FormDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Drawing;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.MobileControls;

    using Microsoft.Win32;

    /// <summary>
    ///    <para>
    ///       Provides design-time support for the <see cref='System.Web.UI.MobileControls.Form'/>
    ///       mobile control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.Form'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class FormDesigner : MobileContainerDesigner
    {
        private Form   _form;
        private TemporaryBitmapFile _backgroundBmpFile = null;
        private static readonly Attribute[] _emptyAttrs = new Attribute[0];
        private const String _titlePropertyName = "Title";

        public override String ID 
        {
            get 
            {
                return base.ID; 
            }
           
            set 
            {
                base.ID = value;
                ChangeBackgroundImage();
            }
        }

        public virtual String Title
        {
            get
            {
                return _form.Title;
            }

            set
            {
                _form.Title = value;
                ChangeBackgroundImage();
            }
        }

        private bool ValidContainment
        {
            get
            {
                return (ContainmentStatus == ContainmentStatus.AtTopLevel);
            }
        }

        private void ChangeBackgroundImage()
        {
            bool infoMode = false;
            String newMessage = GetErrorMessage(out infoMode);
            OnBackgroundImageChange(newMessage, infoMode);
        }

        protected override void Dispose(bool disposing) 
        {
            if (disposing)
            {
                SystemEvents.UserPreferenceChanged -= 
                    new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
            }

            base.Dispose(disposing);
        }

        protected override String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            if (!DesignerAdapterUtil.InMobileUserControl(_form))
            {
                if (DesignerAdapterUtil.InUserControl(_form))
                {
                    infoMode = true;
                    return MobileControlDesigner._userControlWarningMessage;
                }

                if (!DesignerAdapterUtil.InMobilePage(_form))
                {
                    return SR.GetString(SR.MobileControl_MobilePageErrorMessage);
                }
            }

            if (!ValidContainment)
            {
                return SR.GetString(SR.MobileControl_TopPageContainmentErrorMessage);
            }

            return null;
        }

        /// <summary>
        ///    <para>
        ///       Initializes the designer using
        ///       the specified component.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element being designed.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component being
        ///       designed.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is Form,
                         "FormDesigner.Initialize - Invalid Form Control");

            // This must be called first in order to get properties from runtime control.
            base.Initialize(component);

            _form = (Form) component;

            if (_form.DeviceSpecific != null) {
                _form.DeviceSpecific = null;
            }

            SystemEvents.UserPreferenceChanged += 
                new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        internal override void OnBackgroundImageChange(String message, bool infoMode)
        {
            ImageCreator.CreateBackgroundImage(
                ref _backgroundBmpFile,
                _form.ID,
                _form.Title,
                message,
                infoMode,
                GetDefaultSize().Width
            );

            SetBehaviorStyle("backgroundImage",
                "url(" + _backgroundBmpFile.Url + ")");
            SetBehaviorStyle(
                "paddingTop",
                _backgroundBmpFile.UnderlyingBitmap.Height + 8
            );
        }

        private void OnUserPreferenceChanged(Object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color)
            {
                bool infoMode;
                String newMessage = GetErrorMessage(out infoMode);
                OnBackgroundImageChange(newMessage, infoMode);
            }            
        }

        /// <summary>
        ///   Adjust the appearance based on current status.
        /// </summary>
        protected override void OnContainmentChanged()
        {
            base.OnContainmentChanged();

            SetBehaviorStyle("marginTop", ValidContainment ? "5px" : "3px");
            SetBehaviorStyle("marginBottom", ValidContainment ? "5px" : "3px");
            SetBehaviorStyle("marginRight", ValidContainment ? "30%" : "5px");
        }

        protected override void PreFilterProperties(IDictionary properties) 
        {
            base.PreFilterProperties(properties);

            PropertyDescriptor prop = (PropertyDescriptor)properties[_titlePropertyName];
            Debug.Assert(prop != null);
            properties[_titlePropertyName] = 
                TypeDescriptor.CreateProperty(GetType(), prop, _emptyAttrs);
        }

        protected override void SetControlDefaultAppearance()
        {
            base.SetControlDefaultAppearance();

            // Customized border style
            SetBehaviorStyle("borderStyle", "solid");
        }
    }
}
