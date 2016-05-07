//------------------------------------------------------------------------------
// <copyright file="DesignerAdapterUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI.Design;
using System.Web.UI.Design.MobileControls;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal static class DesignerAdapterUtil
    {
        // margin width is 10px on right (10px on left taken care of by parentChildOffset)
        private const int _marginWidth = 10;
        // default Panel or Form width
        private const int _defaultContainerWidth = 300;
        // 11px on the left and the right for padding and margin between levels
        private const int _marginPerLevel = 22;
        // offset of control within a template is 10px on the left + 11px on the right + 1
        private const int _templateParentChildOffset = 22;
        // offset of control outside of a template is 11px
        private const int _regularParentChildOffset = 11;

        // default width for controls in templates. The value doesn't matter as long as it is 
        // equal or larger than parent width, since the parent control designer will still 
        // truncate to 100%
        internal const int CONTROL_MAX_WIDTH_IN_TEMPLATE = 300;
        internal const byte CONTROL_IN_TEMPLATE_NONEDIT = 0x01;
        internal const byte CONTROL_IN_TEMPLATE_EDIT    = 0x02;
 
        internal static IDesigner ControlDesigner(IComponent component)
        {
            Debug.Assert(null != component);
            ISite compSite = component.Site;

            if (compSite != null)
            {
                return ((IDesignerHost) compSite.GetService(typeof(IDesignerHost))).GetDesigner(component);
            }
            return null;
        }

        internal static ContainmentStatus GetContainmentStatus(Control control)
        {
            ContainmentStatus containmentStatus = ContainmentStatus.Unknown;
            Control parent = control.Parent;

            if (control == null || parent == null)
            {
                return containmentStatus;
            }

            if (parent is Form)
            {
                containmentStatus = ContainmentStatus.InForm;
            }
            else if (parent is Panel)
            {
                containmentStatus = ContainmentStatus.InPanel;
            }
            else if (parent is Page || parent is UserControl)
            {
                containmentStatus = ContainmentStatus.AtTopLevel;
            }
            else if (InTemplateFrame(control))
            {
                containmentStatus = ContainmentStatus.InTemplateFrame;
            }

            return containmentStatus;
        }

        internal static IComponent GetRootComponent(IComponent component)
        {
            Debug.Assert(null != component);
            ISite compSite = component.Site;

            if (compSite != null)
            {
                IDesignerHost host = (IDesignerHost)compSite.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    return host.RootComponent;
                }
            }

            return null;
        }

        internal static String GetWidth(Control control)
        {
            if (DesignerAdapterUtil.GetContainmentStatus(control) == ContainmentStatus.AtTopLevel)
            {
                return Constants.ControlSizeAtToplevelInNonErrorMode;
            }
            return Constants.ControlSizeInContainer;
        }

        internal static bool InMobilePage(Control control)
        {
            return (control != null && control.Page is MobilePage);
        }

        internal static bool InUserControl(IComponent component)
        {
            return GetRootComponent(component) is UserControl;
        }

        internal static bool InMobileUserControl(IComponent component)
        {
            return GetRootComponent(component) is MobileUserControl;
        }

        // Returns true if the closest templateable ancestor is in template editing mode.
        internal static  bool InTemplateFrame(Control control)
        {
            if (control.Parent == null)
            {
                return false;
            }

            TemplatedControlDesigner designer = 
                ControlDesigner(control.Parent) as TemplatedControlDesigner;

            if (designer == null)
            {
                return InTemplateFrame(control.Parent);
            }

            if (designer.InTemplateMode)
            {
                return true;
            }

            return false;
        }

        internal static  void AddAttributesToProperty(
            Type designerType,
            IDictionary properties,
            String propertyName,
            Attribute[] attributeArray)
        {
            Debug.Assert (propertyName != null && 
                propertyName.Length != 0);

            PropertyDescriptor prop = (PropertyDescriptor)properties[propertyName];
            Debug.Assert(prop != null);

            prop = TypeDescriptor.CreateProperty (
                designerType,
                prop,
                attributeArray);

            properties[propertyName] = prop;
        }

        internal static  void AddAttributesToPropertiesOfDifferentType(
            Type designerType,
            Type newType,
            IDictionary properties,
            String propertyName,
            Attribute newAttribute)
        {
            Debug.Assert (propertyName != null && 
                propertyName.Length != 0);

            PropertyDescriptor prop = (PropertyDescriptor)properties[propertyName];
            Debug.Assert(prop != null);

            // we can't create the designer DataSource property based on the runtime property since their
            // types do not match. Therefore, we have to copy over all the attributes from the runtime
            // and use them that way.
            System.ComponentModel.AttributeCollection runtimeAttributes = prop.Attributes;
            Attribute[] attrs = new Attribute[runtimeAttributes.Count + 1];
            runtimeAttributes.CopyTo(attrs, 0);

            attrs[runtimeAttributes.Count] = newAttribute;
            prop = TypeDescriptor.CreateProperty (
                designerType,
                propertyName,
                newType,
                attrs);

            properties[propertyName] = prop;
        }

        internal static  int NestingLevel(Control control, 
                                       out bool inTemplate, 
                                       out int defaultControlWidthInTemplate)
        {
            int level = -1;
            defaultControlWidthInTemplate = 0;
            inTemplate = false;
            if (control != null)
            {
                Control parent = control.Parent;
                while (parent != null)
                {
                    level++;
                    IDesigner designer = ControlDesigner(parent);
                    if (designer is MobileTemplatedControlDesigner)
                    {
                        defaultControlWidthInTemplate = 
                            ((MobileTemplatedControlDesigner) designer).TemplateWidth - 
                            _templateParentChildOffset;
                        inTemplate = true;
                        return level;
                    }
                    parent = parent.Parent;
                }
            }
            return level;
        }

        internal static  void SetStandardStyleAttributes(IHtmlControlDesignerBehavior behavior, 
                                                      ContainmentStatus containmentStatus)
        {
            if (behavior == null) {
                return;
            }

            bool controlAtTopLevel = (containmentStatus == ContainmentStatus.AtTopLevel);

            Color cw = SystemColors.Window;
            Color ct = SystemColors.WindowText;
            Color c = Color.FromArgb((Int16)(ct.R * 0.1 + cw.R * 0.9),
                (Int16)(ct.G * 0.1 + cw.G * 0.9),
                (Int16)(ct.B * 0.1 + cw.B * 0.9));
            behavior.SetStyleAttribute("borderColor", true, ColorTranslator.ToHtml(c), true);
            behavior.SetStyleAttribute("borderStyle", true, "solid", true);
                        
            behavior.SetStyleAttribute("borderWidth", true, "1px", true);
            behavior.SetStyleAttribute("marginLeft", true, "5px", true);
            behavior.SetStyleAttribute("marginRight", true, controlAtTopLevel ? "30%" : "5px", true);
            behavior.SetStyleAttribute("marginTop", true, controlAtTopLevel ? "5px" : "2px", true);
            behavior.SetStyleAttribute("marginBottom", true, controlAtTopLevel ? "5px" : "2px", true);
        }

        internal static  String GetDesignTimeErrorHtml(
            String errorMessage, 
            bool infoMode,
            Control control,
            IHtmlControlDesignerBehavior behavior,
            ContainmentStatus containmentStatus)
        {
            String id = String.Empty;
            Debug.Assert(control != null, "control is null");

            if (control.Site != null)
            {
                id = control.Site.Name;
            }

            if (behavior != null) {
                behavior.SetStyleAttribute("borderWidth", true, "0px", true);
            }

            return String.Format(CultureInfo.CurrentCulture,
                MobileControlDesigner.defaultErrorDesignTimeHTML,
                new Object[]
                {
                    control.GetType().Name,
                    id,
                    errorMessage,
                    infoMode? MobileControlDesigner.infoIcon : MobileControlDesigner.errorIcon,
                    ((containmentStatus == ContainmentStatus.AtTopLevel) ? 
                    Constants.ControlSizeAtToplevelInErrormode : 
                    Constants.ControlSizeInContainer)
                });
        }

        internal static  int GetMaxWidthToFit(MobileControl control, out byte templateStatus)
        {
            IDesigner parentDesigner = ControlDesigner(control.Parent);
            IDesigner controlDesigner = ControlDesigner(control);
            int defaultControlWidthInTemplate;

            NativeMethods.IHTMLElement2 htmlElement2Parent = null;
            
            if (controlDesigner == null)
            {
                templateStatus = CONTROL_IN_TEMPLATE_NONEDIT;
                return 0;
            }
            Debug.Assert(controlDesigner is MobileControlDesigner ||
                         controlDesigner is MobileTemplatedControlDesigner, 
                         "controlDesigner is not MobileControlDesigner or MobileTemplatedControlDesigner");

            templateStatus = 0x00;
            if (parentDesigner is MobileTemplatedControlDesigner)
            {
                htmlElement2Parent =
                    (NativeMethods.IHTMLElement2) 
                    ((MobileTemplatedControlDesigner) parentDesigner).DesignTimeElementInternal;
            }
            else if (parentDesigner is MobileContainerDesigner)
            {
                htmlElement2Parent =
                    (NativeMethods.IHTMLElement2) 
                    ((MobileContainerDesigner) parentDesigner).DesignTimeElementInternal;
            }

            bool inTemplate;
            int nestingLevel = DesignerAdapterUtil.NestingLevel(control, out inTemplate, out defaultControlWidthInTemplate);
            if (inTemplate)
            {
                templateStatus = CONTROL_IN_TEMPLATE_EDIT;
            }

            if (htmlElement2Parent != null)
            {
                int maxWidth;
                if (!inTemplate)
                {
                    Debug.Assert(control.Parent is MobileControl);
                    Style parentStyle = ((MobileControl) control.Parent).Style;
                    Alignment alignment = (Alignment) parentStyle[Style.AlignmentKey, true];
                    int parentChildOffset=0;

                    // AUI 2786
                    if (alignment != Alignment.NotSet && alignment != Alignment.Left)
                    {
                        parentChildOffset = _regularParentChildOffset;
                    }
                    else
                    {
                        NativeMethods.IHTMLRectCollection rectColl = null;
                        NativeMethods.IHTMLRect rect = null;
                        int index = 0;
                        Object obj = index;

                        NativeMethods.IHTMLElement2 htmlElement2;
                        
                        if (controlDesigner is MobileControlDesigner)
                        { 
                            htmlElement2 = (NativeMethods.IHTMLElement2) ((MobileControlDesigner) controlDesigner).DesignTimeElementInternal;
                        }
                        else
                        {
                            htmlElement2 = (NativeMethods.IHTMLElement2) ((MobileTemplatedControlDesigner) controlDesigner).DesignTimeElementInternal;
                        }

                        if (null == htmlElement2)
                        {
                            return 0;
                        }

                        try
                        {
                            rectColl = htmlElement2.GetClientRects();
                        }
                        catch (Exception)
                        {
                            // this happens when switching from Design view to HTML view
                            return 0;
                        }

                        if( rectColl.GetLength() >= 1)
                        {
                            rect = (NativeMethods.IHTMLRect)rectColl.Item(ref obj);
                            parentChildOffset = rect.GetLeft();

                            rectColl = htmlElement2Parent.GetClientRects();
                            //Debug.Assert(rectColl.GetLength() == 1);
                            rect = (NativeMethods.IHTMLRect) rectColl.Item(ref obj);
                            parentChildOffset -= rect.GetLeft();
                        }
                    }

                    maxWidth = GetLength(htmlElement2Parent) - _marginWidth - parentChildOffset;
                    if (maxWidth > 0 && maxWidth > _defaultContainerWidth - nestingLevel * _marginPerLevel)
                    {
                        maxWidth = _defaultContainerWidth - nestingLevel * _marginPerLevel;
                    }
                }
                else
                {
                    int parentWidth = GetLength(htmlElement2Parent);
                    if (parentWidth == 0)
                    {
                        // AUI 4525
                        maxWidth = defaultControlWidthInTemplate;
                    }
                    else
                    {
                        maxWidth = parentWidth - _templateParentChildOffset;
                    }

                    if (maxWidth > 0 && maxWidth > defaultControlWidthInTemplate - nestingLevel * _marginPerLevel)
                    {
                        maxWidth = defaultControlWidthInTemplate - nestingLevel * _marginPerLevel;
                    }
                }
                return maxWidth;
            }
            return 0;
        }

        private static int GetLength(NativeMethods.IHTMLElement2 element) {
            NativeMethods.IHTMLRectCollection rectColl = element.GetClientRects();
            //Debug.Assert(rectColl.GetLength() == 1);
            Object obj = rectColl.GetLength() - 1;
            NativeMethods.IHTMLRect rect = (NativeMethods.IHTMLRect)rectColl.Item(ref obj);
            return rect.GetRight() - rect.GetLeft();
        }
    }
}
