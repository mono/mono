//------------------------------------------------------------------------------
// <copyright file="PanelDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Drawing;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.MobileControls;

    using Microsoft.Win32;
    using System.Globalization;

    /// <summary>
    ///    <para>
    ///       Provides design-time support for the <see cref='System.Web.UI.MobileControls.Panel'/>
    ///       web control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.Panel'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class PanelDesigner : MobileContainerDesigner
    {
        private Panel  _panel;
        private TemporaryBitmapFile _backgroundBmpFile = null;
        private Size   _defaultSize;

        internal PanelDesigner()
        {
            _defaultSize = new Size(300, 45);
        }

        private bool ValidContainment
        {
            get
            {
                return (ContainmentStatus == ContainmentStatus.InForm ||
                    ContainmentStatus == ContainmentStatus.InPanel ||
                    ContainmentStatus == ContainmentStatus.InTemplateFrame);
            }
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

        protected override Size GetDefaultSize()
        {
            return _defaultSize;
        }

        protected override String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            // Skip containment checking if the control is placed in MobileUserControl
            if (DesignerAdapterUtil.InMobileUserControl(_panel))
            {
                return null;
            }

            if (DesignerAdapterUtil.InUserControl(_panel))
            {
                infoMode = true;
                return MobileControlDesigner._userControlWarningMessage;
            }

            if (!DesignerAdapterUtil.InMobilePage(_panel))
            {
                return SR.GetString(SR.MobileControl_MobilePageErrorMessage);
            }

            if (!ValidContainment)
            {
                return SR.GetString(SR.MobileControl_FormPanelContainmentErrorMessage);
            }

            return null;
        }

        /// <summary>
        ///   Initializes the designer with the Form control that this instance
        ///   of the designer is associated with.
        /// </summary>
        /// <param name='component'>
        ///   The associated Form control.
        /// </param>
        /// <seealso cref='IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is Panel,
                "PanelDesigner.Initialize - Invalid Panel Control");

            // This must be called first in order to get properties from runtime control.
            base.Initialize(component);

            _panel = (Panel) component;

            SystemEvents.UserPreferenceChanged += 
                new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        internal override void OnBackgroundImageChange(String message, bool infoMode)
        {
            if (message == null)
            {
                RemoveBehaviorStyle("backgroundImage");
                SetBehaviorStyle("paddingTop", 8);
            }
            else
            {
                ImageCreator.CreateBackgroundImage(
                    ref _backgroundBmpFile,
                    String.Empty,
                    String.Empty,
                    message,
                    infoMode,
                    GetDefaultSize().Width
                );

                // redraw the background image here
                SetBehaviorStyle("backgroundImage",
                    "url(" + _backgroundBmpFile.Url + ")");
                SetBehaviorStyle(
                    "paddingTop",
                    _backgroundBmpFile.UnderlyingBitmap.Height + 8
                );
            }
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
        //protected override void SetStyleAttributes()
        protected override void OnContainmentChanged()
        {
            base.OnContainmentChanged();

            SetBehaviorStyle("marginRight",
                ContainmentStatus == ContainmentStatus.AtTopLevel ? "30%" : "5px");

            SetBehaviorStyle("marginTop", ValidContainment? "3px" : "5px");
            SetBehaviorStyle("marginBottom", ValidContainment? "3px" : "5px");
            SetBehaviorStyle("width", ValidContainment? "100%" : GetDefaultSize().Width.ToString(CultureInfo.InvariantCulture) + "px");
        }

        protected override void SetControlDefaultAppearance()
        {
            base.SetControlDefaultAppearance();

            // Customize styles
            SetBehaviorStyle("borderStyle", "dotted");
        }
    }
}
