//------------------------------------------------------------------------------
// <copyright file="MobileControlDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class MobileControlDesigner :
        ControlDesigner, IMobileDesigner, IDeviceSpecificDesigner
    {
        private bool                            _containmentStatusDirty = true;
        private ContainmentStatus               _containmentStatus;
        private IDesignerHost                   _host;
        private IWebFormsDocumentService        _iWebFormsDocumentService;
        private IMobileWebFormServices          _iMobileWebFormServices;
        private MobileControl                   _mobileControl;
        private System.Windows.Forms.Control    _header;

        internal static readonly String resourceDllUrl =
            "res://" + typeof(MobileControlDesigner).Module.FullyQualifiedName;

        internal static readonly String errorIcon =
            resourceDllUrl + "//ERROR_GIF";

        internal static readonly String infoIcon =
            resourceDllUrl + "//INFO_GIF";

        internal static readonly String defaultErrorDesignTimeHTML =
            @"
                <table cellpadding=2 cellspacing=0 width='{4}' style='font-family:tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow'>
                    <tr><td><span style='font-weight:bold'>&nbsp;{0}</span> - {1}</td></tr>
                    <tr><td>
                        <table style='font-family:tahoma;font-size:8pt;color:window;background-color:ButtonShadow'>
                            <tr>
                                <td valign='top'><img src={3} /></td>
                                <td width='100%'>{2}</td>
                            </tr>
                        </table>
                    </td></tr>
                </table>
             ";

        internal static readonly String _formPanelContainmentErrorMessage =
            SR.GetString(SR.MobileControl_FormPanelContainmentErrorMessage);

        internal static readonly String _mobilePageErrorMessage =
            SR.GetString(SR.MobileControl_MobilePageErrorMessage);

        internal static readonly String _topPageContainmentErrorMessage =
            SR.GetString(SR.MobileControl_TopPageContainmentErrorMessage);

        internal static readonly String _userControlWarningMessage =
            SR.GetString(SR.MobileControl_UserControlWarningMessage);

        private const String _appliedDeviceFiltersPropName = "AppliedDeviceFilters";
        private const String _propertyOverridesPropName = "PropertyOverrides";
        private const String _defaultDeviceSpecificIdentifier = "unique";

        private static readonly string[] _nonBrowsableProperties = new string[] {
            "EnableTheming",
            "Expressions",
            "SkinID",
        };

        // predefined constants used for mergingContext
        internal const int MergingContextChoices = 0;
        internal const int MergingContextTemplates = 1;
        internal const int MergingContextProperties = 2;

        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            Editor(typeof(AppliedDeviceFiltersTypeEditor), typeof(UITypeEditor)),
            MergableProperty(false),
            MobileCategory(SR.Category_DeviceSpecific),
            MobileSysDescription(SR.MobileControl_AppliedDeviceFiltersDescription),
            ParenthesizePropertyName(true),
        ]
        protected String AppliedDeviceFilters
        {
            get
            {
                return String.Empty;
            }
        }

        protected ContainmentStatus ContainmentStatus
        {
            get
            {
                if (!_containmentStatusDirty)
                {
                    return _containmentStatus;
                }

                _containmentStatus =
                    DesignerAdapterUtil.GetContainmentStatus(_mobileControl);

                _containmentStatusDirty = false;
                return _containmentStatus;
            }
        }

        internal Object DesignTimeElementInternal
        {
            get
            {
                return typeof(HtmlControlDesigner).InvokeMember("DesignTimeElement", 
                    BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic, 
                    null, this, null, CultureInfo.InvariantCulture);
            }
        }

        public override bool DesignTimeHtmlRequiresLoadComplete
        {
            get
            {
                return true;
            }
        }

        private IDesignerHost Host
        {
            get
            {
                if (_host != null)
                {
                    return _host;
                }
                _host = (IDesignerHost)GetService(typeof(IDesignerHost));
                Debug.Assert(_host != null);
                return _host;
            }
        }

        internal IMobileWebFormServices IMobileWebFormServices
        {
            get
            {
                if (_iMobileWebFormServices == null)
                {
                    _iMobileWebFormServices =
                        (IMobileWebFormServices)GetService(typeof(IMobileWebFormServices));
                }

                return _iMobileWebFormServices;
            }
        }

        private IWebFormsDocumentService IWebFormsDocumentService
        {
            get
            {
                if (_iWebFormsDocumentService == null)
                {
                    _iWebFormsDocumentService =
                        (IWebFormsDocumentService)GetService(typeof(IWebFormsDocumentService));

                    Debug.Assert(_iWebFormsDocumentService != null);
                }

                return _iWebFormsDocumentService;
            }
        }

        /// <summary>
        ///     Indicates whether the initial page load is completed
        /// </summary>
        protected bool LoadComplete
        {
            get
            {
                return !IWebFormsDocumentService.IsLoading;
            }
        }

        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            Editor(typeof(PropertyOverridesTypeEditor), typeof(UITypeEditor)),
            MergableProperty(false),
            MobileCategory(SR.Category_DeviceSpecific),
            MobileSysDescription(SR.MobileControl_DeviceSpecificPropsDescription),
            ParenthesizePropertyName(true),
        ]
        protected String PropertyOverrides
        {
            get
            {
                return String.Empty;
            }
        }

        private bool ValidContainment
        {
            get
            {
                return (
                    ContainmentStatus == ContainmentStatus.InForm ||
                    ContainmentStatus == ContainmentStatus.InPanel ||
                    ContainmentStatus == ContainmentStatus.InTemplateFrame);
            }
        }

        protected virtual String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            // Skip containment checking if the control is placed in MobileUserControl
            if (!DesignerAdapterUtil.InMobileUserControl(_mobileControl))
            {
                if (DesignerAdapterUtil.InUserControl(_mobileControl))
                {
                    infoMode = true;
                    return MobileControlDesigner._userControlWarningMessage;
                }

                if (!DesignerAdapterUtil.InMobilePage(_mobileControl))
                {
                    return _mobilePageErrorMessage;
                }

                if (!ValidContainment)
                {
                    return _formPanelContainmentErrorMessage;
                }
            }

            bool containsTag;
            bool containsDataboundLiteral;
            _mobileControl.GetControlText(out containsTag, out containsDataboundLiteral);

            if (containsTag)
            {
                return SR.GetString(SR.MobileControl_InnerTextCannotContainTagsDesigner);
            }

            // Containment is valid, return null;
            return null;
        }

        /// <summary>
        ///    <para>
        ///       Gets the HTML to be used for the design time representation of the control runtime.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The design time HTML.
        ///    </para>
        /// </returns>
        public sealed override String GetDesignTimeHtml()
        {
            if (!LoadComplete)
            {
                return null;
            }

            bool infoMode = false;
            String errorMessage = GetErrorMessage(out infoMode);
            SetStyleAttributes();

            if (null != errorMessage)
            {
                return GetDesignTimeErrorHtml(errorMessage, infoMode);
            }

            String designTimeHTML = null;
            try
            {
                designTimeHTML = GetDesignTimeNormalHtml();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                designTimeHTML = GetDesignTimeErrorHtml(ex.Message, false);
            }

            return designTimeHTML;
        }

        protected virtual String GetDesignTimeNormalHtml()
        {
            return GetEmptyDesignTimeHtml();
        }

        /// <summary>
        ///    <para>
        ///       Gets the HTML to be used at design time as the representation of the
        ///       control when the control runtime does not return any rendered
        ///       HTML. The default behavior is to return a string containing the name
        ///       of the component.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The name of the component, by default.
        ///    </para>
        /// </returns>
        protected override String GetEmptyDesignTimeHtml()
        {
            return "<div style='width:100%'>" + base.GetEmptyDesignTimeHtml() + "</div>";
        }

        protected override sealed String GetErrorDesignTimeHtml(Exception e)
        {
            return base.GetErrorDesignTimeHtml(e);
        }

        /// <summary>
        ///
        /// </summary>
        protected virtual String GetDesignTimeErrorHtml(String errorMessage, bool infoMode)
        {
            return DesignerAdapterUtil.GetDesignTimeErrorHtml(
                errorMessage, infoMode, _mobileControl, Behavior, ContainmentStatus);
        }

        /// <summary>
        ///    <para>
        ///       Gets the HTML to be persisted for the content present within the associated server control runtime.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       Persistable Inner HTML.
        ///    </para>
        /// </returns>
        public override String GetPersistInnerHtml()
        {
            if (!IsDirty)
            {
                // Returning a null string will prevent the actual save.
                return null;
            }

            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);

            // HACK ALERT:
            // We need to temporarily swap out the Text property to avoid being
            // persisted into control inner text. However, setting the Text property
            // will wipe out all control collections, therefore we need to cache
            // the Text value too.
            bool hasControls = _mobileControl.HasControls();
            if ((_mobileControl is TextControl || _mobileControl is TextView)
                && hasControls)
            {
                String originalText = null;
                Control[] children = null;

                // Cache all child controls here.
                children = new Control[_mobileControl.Controls.Count];
                _mobileControl.Controls.CopyTo(children, 0);

                // Replace the text with empty string.
                if (_mobileControl is TextControl)
                {
                    originalText = ((TextControl)_mobileControl).Text;
                    ((TextControl)_mobileControl).Text = String.Empty;
                }
                else
                {
                    originalText = ((TextView)_mobileControl).Text;
                    ((TextView)_mobileControl).Text = String.Empty;
                }

                try
                {
                    // Persist inner properties without Text property.
                    MobileControlPersister.PersistInnerProperties(sw, _mobileControl, Host);
                    // Persist the child collections.
                    foreach (Control c in children)
                    {
                        MobileControlPersister.PersistControl(sw, c, Host);
                    }
                }
                finally
                {
                    // Write the original text back to control.
                    if (_mobileControl is TextControl)
                    {
                        ((TextControl)_mobileControl).Text = originalText;
                    }
                    else
                    {
                        ((TextView)_mobileControl).Text = originalText;
                    }

                    // Add the child controls back.
                    foreach (Control c in children)
                    {
                        _mobileControl.Controls.Add(c);
                    }
                }
            }
            else
            {
                MobileControlPersister.PersistInnerProperties(sw, _mobileControl, Host);
            }

            IsDirty = false;
            return sw.ToString();
        }

        /// <summary>
        ///    <para>
        ///       Initializes the designer with the component for design.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element for design.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component for
        ///       design.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.MobileControl,
                "MobileControlDesigner.Initialize - Invalid MobileControl Control");

            base.Initialize(component);

            _mobileControl = (System.Web.UI.MobileControls.MobileControl) component;
        }

        protected virtual void SetStyleAttributes()
        {
            //Debug.Assert(Behavior != null, "Behavior is null, Load completed? " + LoadComplete.ToString());

            DesignerAdapterUtil.SetStandardStyleAttributes(Behavior, ContainmentStatus);
        }

        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs ce)
        {
            // Delegate to the base class implementation first!
            base.OnComponentChanged(sender, ce);

            MemberDescriptor member = ce.Member;
            if (member != null &&
                member.GetType().FullName.Equals(Constants.ReflectPropertyDescriptorTypeFullName))
            {
                PropertyDescriptor propDesc = (PropertyDescriptor)member;
                String propName = propDesc.Name;

                if ((_mobileControl is TextControl || _mobileControl is TextView)
                    && propName.Equals("Text"))
                {
                    _mobileControl.Controls.Clear();
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Notification that is called when the associated control is parented.
        ///    </para>
        /// </summary>
        public override void OnSetParent()
        {
            base.OnSetParent();

            _containmentStatusDirty = true;
            if (LoadComplete)
            {
                UpdateRendering();
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            properties[_appliedDeviceFiltersPropName] =
                TypeDescriptor.CreateProperty(this.GetType(), _appliedDeviceFiltersPropName, typeof(String));

            properties[_propertyOverridesPropName] =
                TypeDescriptor.CreateProperty(this.GetType(), _propertyOverridesPropName, typeof(String));

            foreach (string propertyName in _nonBrowsableProperties) {
                PropertyDescriptor property = (PropertyDescriptor) properties[propertyName];
                Debug.Assert(property != null, "Property is null: " + propertyName);
                if (property != null) {
                    properties[propertyName] = TypeDescriptor.CreateProperty(this.GetType(), property, BrowsableAttribute.No);
                }
            }
        }

        /*
         *  IMobileDesigner INTERFACE IMPLEMENTATION
         */

        /// <summary>
        ///
        /// </summary>
        public void UpdateRendering()
        {
            _mobileControl.RefreshStyle();

            UpdateDesignTimeHtml();
        }

        ////////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        void IDeviceSpecificDesigner.SetDeviceSpecificEditor
            (IRefreshableDeviceSpecificEditor editor)
        {
        }

        String IDeviceSpecificDesigner.CurrentDeviceSpecificID
        {
            get
            {
                return _defaultDeviceSpecificIdentifier;
            }
        }

        System.Windows.Forms.Control IDeviceSpecificDesigner.Header
        {
            get
            {
                return _header;
            }
        }

        System.Web.UI.Control IDeviceSpecificDesigner.UnderlyingControl
        {
            get
            {
                return _mobileControl;
            }
        }

        Object IDeviceSpecificDesigner.UnderlyingObject
        {
            get
            {
                return _mobileControl;
            }
        }

        void IDeviceSpecificDesigner.InitHeader(int mergingContext)
        {
            HeaderPanel panel = new HeaderPanel();
            HeaderLabel lblDescription = new HeaderLabel();

            lblDescription.TabIndex = 0;
            lblDescription.Text = SR.GetString(
                SR.MobileControl_SettingGenericChoiceDescription
            );
            panel.Height = lblDescription.Height;
            panel.Width = lblDescription.Width;
            panel.Controls.Add(lblDescription);
            _header = panel;
        }

        void IDeviceSpecificDesigner.RefreshHeader(int mergingContext)
        {
        }

        bool IDeviceSpecificDesigner.GetDeviceSpecific(String deviceSpecificParentID, out DeviceSpecific ds)
        {
            Debug.Assert(_defaultDeviceSpecificIdentifier == deviceSpecificParentID);
            ds = ((MobileControl) _mobileControl).DeviceSpecific;
            return true;
        }

        void IDeviceSpecificDesigner.SetDeviceSpecific(String deviceSpecificParentID, DeviceSpecific ds)
        {
            Debug.Assert(_defaultDeviceSpecificIdentifier == deviceSpecificParentID);
            if (null != ds)
            {
                ds.SetOwner((MobileControl) _mobileControl);
            }
            _mobileControl.DeviceSpecific = ds;
        }

        void IDeviceSpecificDesigner.UseCurrentDeviceSpecificID()
        {
        }

        ////////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////
    }
}
