//------------------------------------------------------------------------------
// <copyright file="DeviceSpecificDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DeviceSpecificDesigner : MobileTemplatedControlDesigner, IDeviceSpecificDesigner
    {
        internal static BooleanSwitch DeviceSpecificDesignerSwitch =
            new BooleanSwitch("DeviceSpecificDesigner", "Enable DeviceSpecific designer general purpose traces.");

        private DeviceSpecific _ds;
        private bool _isDuplicate;
        private System.Web.UI.MobileControls.Panel _parentContainer;

        internal static readonly String _strictlyFormPanelContainmentErrorMessage = 
            SR.GetString(SR.MobileControl_StrictlyFormPanelContainmentErrorMessage);

        private const String _designTimeHTML =
            @"
                <table cellpadding=4 cellspacing=0 width='100%' style='font-family:tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow'>
                  <tr><td colspan=2><span style='font-weight:bold'>DeviceSpecific</span> - {0}</td></tr>
                  <tr><td style='padding-top:0;padding-bottom:0;width:55%;padding-left:10px;font-weight:bold'>Template Device Filter:</td><td style='padding-top:0;padding-bottom:0'>{1}</td></tr>
                  <tr><td colspan=2 style='padding-top:4px'>{2}</td></tr>
                </table>
             ";

        private const String _duplicateDesignTimeHTML =
            @"
                <table cellpadding=4 cellspacing=0 width='100%' style='font-family:tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow'>
                  <tr><td colspan=2><span style='font-weight:bold'>DeviceSpecific</span> - {0}</td></tr>
                  <tr><td style='padding-top:0;padding-bottom:0;width:55%;padding-left:10px;font-weight:bold'>Template Device Filter:</td><td colspan=2 style='padding-top:0;padding-bottom:0'>{1}</td></tr>
                  <tr><td colspan=2 style='padding-top:4px'>{2}</td></tr>
                  <tr><td colspan=2>
                    <table style='font-size:8pt;color:window;background-color:ButtonShadow'>
                      <tr><td valign='top'><img src='{3}'/></td><td>{4}</td></tr>
                    </table>
                  </td></tr>
                </table>
             ";

        private const String _propertyOverridesPropName = "PropertyOverrides";
        private const String _dataBindingsPropName = "DataBindings";

        private const int _headerFooterTemplates            = 0;
        private const int _contentTemplate                  = 0;

        private bool FormDeviceSpecific
        {
            get
            {
                Debug.Assert(_parentContainer != null);
                return _parentContainer is System.Web.UI.MobileControls.Form;
            }
        }

        private static readonly String[] _templateFramesForForm =
            new String [] { Constants.HeaderTemplateTag, Constants.FooterTemplateTag };

        private static readonly String[] _templateFramesForPanel = 
            new String [] { Constants.ContentTemplateTag };

        protected override void Dispose(bool disposing) 
        {
            if (disposing)
            {
                ParentContainerInvalid();
            }

            base.Dispose(disposing);
        }

        protected override TemplateEditingVerb[] GetCachedTemplateEditingVerbs() 
        {
            if (_isDuplicate)
            {
                return null;
            }

            return base.GetCachedTemplateEditingVerbs();
        }

        private void ParentContainerInvalid()
        {
            // MessageBox.Show("ParentContainerInvalid call");
            if (null != _parentContainer && _ds == _parentContainer.DeviceSpecific)
            {
                _parentContainer.DeviceSpecific = null;

                // container's enabled deviceSpecific control is deleted.
                // another disabled deviceSpecific child may need to be enabled.
                foreach (System.Web.UI.Control control in _parentContainer.Controls)
                {
                    if (control is DeviceSpecific && control != _ds)
                    {
                        // found a valid candidate
                        DeviceSpecific newDS = (DeviceSpecific) control;
                        if (newDS.Site != null)
                        {
                            IDesignerHost host = (IDesignerHost) newDS.Site.GetService(typeof(IDesignerHost));
                            Debug.Assert(host != null, "host is null in DeviceSpecificDesigner");
                            IDesigner designer = host.GetDesigner((IComponent) newDS);

                            // this designer could be null if the page is disposing the controls (Page.Dispose).
                            if (designer != null)
                            {
                                _parentContainer.DeviceSpecific = newDS;
                                DeviceSpecificDesigner dsd = (DeviceSpecificDesigner) designer;
                                dsd.TreatAsDuplicate(false);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.DeviceSpecific,
                         "DeviceSpecificControlDesigner.Initialize - Invalid DeviceSpecific Control");

            _ds = (System.Web.UI.MobileControls.DeviceSpecific) component;
            base.Initialize(component);

            _isDuplicate = false;
        }

        public override DeviceSpecific CurrentDeviceSpecific
        {
            get
            {
                Debug.Assert(null != _ds);
                return _ds;
            }
        }

        protected override String GetDesignTimeNormalHtml()
        {
            String curChoice, message;
            bool _isNonHtmlSchema = false;

            if (null == CurrentChoice)
            {
                curChoice = SR.GetString(SR.DeviceSpecific_PropNotSet);
                message = SR.GetString(SR.DeviceSpecific_DefaultMessage);
            }
            else
            {
                if (CurrentChoice.Filter.Length == 0)
                {
                    curChoice = SR.GetString(SR.DeviceFilter_DefaultChoice);
                }
                else
                {
                    curChoice = HttpUtility.HtmlEncode(DesignerUtility.ChoiceToUniqueIdentifier(CurrentChoice));
                }

                if (IsHTMLSchema(CurrentChoice))
                {
                    message = SR.GetString(SR.DeviceSpecific_TemplateEditingMessage);
                }
                else
                {
                    _isNonHtmlSchema = true;
                    message = SR.GetString(SR.DeviceSpecific_DefaultMessage);
                }
            }

            if (_isDuplicate || _isNonHtmlSchema)
            {
                return String.Format(CultureInfo.CurrentCulture, _duplicateDesignTimeHTML,
                                     new Object[]
                                     {
                                         _ds.Site.Name,
                                         curChoice,
                                         message,
                                         _isDuplicate ? MobileControlDesigner.errorIcon : 
                                         MobileControlDesigner.infoIcon,
                                         _isDuplicate ? 
                                         SR.GetString(SR.DeviceSpecific_DuplicateWarningMessage) :
                                         SR.GetString(SR.MobileControl_NonHtmlSchemaErrorMessage) 
                                     });
            }
            else
            {
                return String.Format(CultureInfo.CurrentCulture, _designTimeHTML, _ds.Site.Name, curChoice, message);
            }
        }

        private bool ValidContainment
        {
            get
            {
                return (ContainmentStatus == ContainmentStatus.InForm ||
                        ContainmentStatus == ContainmentStatus.InPanel);
            }
        }

        protected override String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            if (!DesignerAdapterUtil.InMobileUserControl(_ds))
            {
                if (DesignerAdapterUtil.InUserControl(_ds))
                {
                    infoMode = true;
                    return MobileControlDesigner._userControlWarningMessage;
                }

                if (!DesignerAdapterUtil.InMobilePage(_ds))
                {
                    return MobileControlDesigner._mobilePageErrorMessage;
                }
            }
            
            if (!ValidContainment)
            {
                return _strictlyFormPanelContainmentErrorMessage;
            }

            // No error condition, return null;
            return null;
        }

        internal void TreatAsDuplicate(bool isDuplicate)
        {
            if (isDuplicate != _isDuplicate)
            {
                _isDuplicate = isDuplicate;
                SetTemplateVerbsDirty();
                // MessageBox.Show("TreatAsDuplicate: Changing status of " + _ds.Site.Name + " to _isDuplicate=" + _isDuplicate.ToString());
            }
            UpdateDesignTimeHtml();
        }

        public override void OnSetParent() 
        {
            // MessageBox.Show("OnSetParent call for _ds.Site.Name=" + _ds.Site.Name + ", _ds.ID=" + _ds.ID);
            base.OnSetParent();

            Debug.Assert(_ds.Parent != null, "_ds.Parent is null");

            if (null != _parentContainer)
            {
                ParentContainerInvalid();
            }

            System.Web.UI.Control parentContainer = _ds.Parent;

            if (parentContainer is System.Web.UI.MobileControls.Panel)
            {
                _parentContainer = (System.Web.UI.MobileControls.Panel) parentContainer;
                _ds.SetOwner(_parentContainer);

                if (null != _parentContainer.DeviceSpecific &&
                    0 != String.Compare(_ds.ID, _parentContainer.DeviceSpecific.ID, StringComparison.OrdinalIgnoreCase))
                {
                    // the parent container already has a deviceSpecific child.
                    // this instance is a duplicate and needs to update its rendering.
                    // MessageBox.Show("OnSetParent - this instance is a duplicate");
                    TreatAsDuplicate(true);

                    // the current valid DeviceSpecific is intentionaly refreshed because
                    // if this deviceSpecific instance is recreated via a Undo operation
                    // the current valid DeviceSpecific appears as a duplicate if not refreshed.
                    IDesignerHost host = (IDesignerHost) GetService(typeof(IDesignerHost));
                    Debug.Assert(host != null, "Did not get a valid IDesignerHost reference");
                    IDesigner designer = host.GetDesigner((IComponent) _parentContainer.DeviceSpecific);
                    Debug.Assert(designer != null, "designer is null in DeviceSpecificDesigner");
                    DeviceSpecificDesigner dsd = (DeviceSpecificDesigner) designer;
                    dsd.UpdateRendering();
                }
                else
                {
                    // MessageBox.Show("OnSetParent - this instance becomes the valid ds");
                    _parentContainer.DeviceSpecific = _ds;
                    if (_isDuplicate)
                    {
                        TreatAsDuplicate(false);
                    }
                }
            }
            else
            {
                _parentContainer = null;
            }

            // Invalidate the type descriptor so that the PropertyOverrides
            // property browsable status gets updated
            TypeDescriptor.Refresh(Component);
        }

        protected override String[] GetTemplateFrameNames(int index)
        {
            Debug.Assert(index == 0);
            return FormDeviceSpecific ? _templateFramesForForm : _templateFramesForPanel;
        }

        protected override void PreFilterProperties(IDictionary properties) 
        {
            base.PreFilterProperties(properties);

            PropertyDescriptor prop = (PropertyDescriptor)properties[_propertyOverridesPropName];
            Debug.Assert(prop != null);
            properties[_propertyOverridesPropName] = 
                TypeDescriptor.CreateProperty(
                    GetType(), prop,
                    InTemplateMode || _parentContainer == null? BrowsableAttribute.No : BrowsableAttribute.Yes);
        }

        protected override TemplateEditingVerb[] GetTemplateVerbs()
        {
            TemplateEditingVerb[] templateVerbs = new TemplateEditingVerb[1];

            if (FormDeviceSpecific)
            {
                templateVerbs[0] = new TemplateEditingVerb(
                    SR.GetString(SR.TemplateFrame_HeaderFooterTemplates),
                    _headerFooterTemplates,
                    this);
            }
            else
            {
                templateVerbs[0] = new TemplateEditingVerb(
                    SR.GetString(SR.TemplateFrame_ContentTemplate),
                    _contentTemplate,
                    this);
            }

            return templateVerbs;
        }
        
        ////////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        Object IDeviceSpecificDesigner.UnderlyingObject
        {
            get
            {
                return _parentContainer == null ? (Object)_ds : _parentContainer;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////
    }
}
