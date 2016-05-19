//------------------------------------------------------------------------------
// <copyright file="MobileContainerDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;

    using IHTMLElement = NativeMethods.IHTMLElement;
    using IHTMLElementCollection = NativeMethods.IHTMLElementCollection;

    /// <summary>
    ///    <para>Provides a base designer class for all mobile container controls.</para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class MobileContainerDesigner : ControlDesigner, IMobileDesigner
    {
        private MobileControl               _mobileControl;
        private readonly Size               _defaultSize;
        private bool                        _containmentStatusDirty = true;
        private bool                        _hasAttributesCached = false;
        private bool                        _shouldDirtyPage = false;
        private ContainmentStatus           _containmentStatus = ContainmentStatus.Unknown;
        private IDictionary                 _behaviorAttributes;
        private String                      _currentErrorMessage = null;
        private IWebFormsDocumentService    _iWebFormsDocumentService;
        private IMobileWebFormServices      _iMobileWebFormServices;
        private EventHandler                _loadComplete = null;

        // cached Behavior object
        private IHtmlControlDesignerBehavior _cachedBehavior = null;

        /// <summary>
        ///    <para>
        ///       Initializes an instance of the <see cref='System.Web.UI.Design.MobileControls.MobileContainerDesigner'/> class.
        ///    </para>
        /// </summary>
        internal MobileContainerDesigner()
        {
            ReadOnly = false;

            _defaultSize = new Size(300, 100);
            _behaviorAttributes = new HybridDictionary();
        }

        /// <summary>
        /// return the containment status
        /// </summary>
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

        private IMobileWebFormServices IMobileWebFormServices
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

        /// <summary>
        ///    Control's style, available only when page is MobilePage
        /// </summary>
        protected Style Style
        {
            get
            {
                if (!DesignerAdapterUtil.InMobilePage(_mobileControl))
                {
                    return null;
                }

                Style style = ((ControlAdapter)_mobileControl.Adapter).Style;

                // Each MobileControl should have its own style
                Debug.Assert(style != null);

                return style;
            }
        }

        /// <summary>
        ///    Apply style related properties to behavior
        /// </summary>
        /// <param name="propName">
        ///    property that needs to be applied, null to apply all
        /// </param>
        private void ApplyPropertyToBehavior(String propName)
        {
            if (Style == null)
            {
                return;
            }

            if (propName == null || propName.Equals("BackColor"))
            {
                Color backColor = (Color)Style[Style.BackColorKey, true];
                SetBehaviorStyle("backgroundColor", ColorTranslator.ToHtml(backColor));
            }
            if (propName == null || propName.Equals("ForeColor"))
            {
                Color foreColor = (Color)Style[Style.ForeColorKey, true];
                SetBehaviorStyle("color", ColorTranslator.ToHtml(foreColor));
            }
            if (propName == null || propName.Equals("Font"))
            {
                bool bold =
                    (BooleanOption)Style[Style.BoldKey, true] == BooleanOption.True;
                bool italic =
                    (BooleanOption)Style[Style.ItalicKey, true] == BooleanOption.True;
                FontSize  fontSize  = (FontSize) Style[Style.FontSizeKey , true];
                String    fontName  = (String)   Style[Style.FontNameKey , true];

                SetBehaviorStyle("fontWeight", bold? "bold" : "normal");
                SetBehaviorStyle("fontStyle", italic? "italic" : "normal");

                if (fontSize == FontSize.Large)
                {
                    SetBehaviorStyle("fontSize", "medium");
                }
                else if (fontSize == FontSize.Small)
                {
                    SetBehaviorStyle("fontSize", "x-small");
                }
                else
                {
                    RemoveBehaviorStyle("fontSize");
                }

                SetBehaviorStyle("fontFamily", fontName);
            }
            if (propName == null || propName.Equals("Alignment"))
            {
                Alignment alignment = (Alignment)Style[Style.AlignmentKey, true];
                bool alignmentNotSet = alignment == Alignment.NotSet;

                SetBehaviorStyle("textAlign",
                    alignmentNotSet ? "" : Enum.Format(typeof(Alignment), alignment, "G"));
            }
        }

        /// <summary>
        ///   Performs the cleanup of the designer class.
        /// </summary>
        /// <seealso cref='IDesigner'/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_loadComplete != null)
                {
                    IWebFormsDocumentService.LoadComplete -= _loadComplete;
                    _loadComplete = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///   The default size of Container Control.
        /// </summary>
        protected virtual Size GetDefaultSize()
        {
            return _defaultSize;
        }

        /// <summary>
        ///    non-null string will render the text as an image
        ///    on the top of container control.
        /// </summary>
        protected virtual String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;
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
            Debug.Assert(component is MobileControl,
                         "MobileContainerDesigner.Initialize - Invalid Mobile Control");

            _mobileControl = (MobileControl) component;
            base.Initialize(component);

            _loadComplete = new EventHandler(this.OnLoadComplete);
            IWebFormsDocumentService.LoadComplete += _loadComplete;
        }

        /// <summary>
        ///    return true if the property is an appearance attribute that needs
        ///    to apply to all child controls.
        /// </summary>
        /// <param name="propertyName">
        /// </param>
        private bool IsAppearanceAttribute(String propertyName)
        {
            return (
                propertyName.Equals("Font") ||
                propertyName.Equals("ForeColor") ||
                propertyName.Equals("BackColor") ||
                propertyName.Equals("Wrapping") ||
                propertyName.Equals("Alignment") ||
                propertyName.Equals("StyleReference"));
        }

        internal virtual void OnBackgroundImageChange(String message, bool infoMode)
        {
        }

        /// <summary>
        ///    <para>
        ///       Notification that is called when the designer is attached to the behavior.
        ///    </para>
        /// </summary>
        protected override void OnBehaviorAttached()
        {
            Debug.Assert(_cachedBehavior == null);
            _cachedBehavior = Behavior;

            PrefixDeviceSpecificTags();
            base.OnBehaviorAttached();

            // Reload the original state if an old Behavior is cached.
            if (_hasAttributesCached)
            {
                ReloadBehaviorState();
            }
        }

        /// <summary>
        ///    Notification that is called when the behavior is detached from designer
        /// </summary>
        protected override void OnBehaviorDetaching()
        {
            // dispose the cached behavior.
            _cachedBehavior = null;
        }

        /// <summary>
        ///    <para>
        ///       Delegate to handle component changed event.
        ///    </para>
        /// </summary>
        /// <param name='sender'>
        ///    The object sending the event.
        /// </param>
        /// <param name='ce'>
        ///    The event object used when firing a component changed notification.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called after a property has been changed. It allows the implementor
        ///       to do any post-processing that may be needed after a property change.
        ///    </para>
        /// </remarks>
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

                if (IsAppearanceAttribute(propName))
                {
                    // Update control rendering
                    UpdateRenderingRecursive();
                }
            }
        }

        /// <summary>
        ///   Subclasses can override to modify their container appearance,
        ///   this method is invoked by OnLoadComplete()
        /// </summary>
        protected virtual void OnContainmentChanged()
        {
            // do nothing
        }

        /// <summary>
        ///     helper method for external UIs
        /// </summary>
        protected virtual void OnInternalChange()
        {
            ISite site = _mobileControl.Site;
            if (site != null)
            {
                IComponentChangeService changeService =
                    (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
                if (changeService != null)
                {
                    try
                    {
                        changeService.OnComponentChanging(_mobileControl, null);
                    }
                    catch (CheckoutException ex)
                    {
                        if (ex == CheckoutException.Canceled)
                            return;
                        throw;
                    }
                    changeService.OnComponentChanged(_mobileControl, null, null, null);
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Notification that is called when the page completes loading.
        ///    </para>
        /// </summary>
        private void OnLoadComplete(Object source, EventArgs e)
        {
            // Need to apply behavior attributes since none are cached
            if (!_hasAttributesCached)
            {
                SetControlDefaultAppearance();

                // Apply the style properties to Behavior
                ApplyPropertyToBehavior(null);
            }

            bool infoMode = false;
            String msg = GetErrorMessage(out infoMode);
            if (msg != _currentErrorMessage || !_hasAttributesCached)
            {
                OnBackgroundImageChange(msg, infoMode);
                _currentErrorMessage = msg;
            }

            // we could reload the attributes
            _hasAttributesCached = true;

            // Change containment related appearance
            OnContainmentChanged();

            // Don't forget the change children appearance,
            // this call is necessary to solve multi-nested control problem.
            UpdateRenderingRecursive();

            // Make the page dirty by calling OnInternalChange if an subsitution occurs.
            if (_shouldDirtyPage)
            {
                OnInternalChange();
                _shouldDirtyPage = false;
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

            // The containment status is invalidated
            _containmentStatusDirty = true;

            // Make sure the control refreshes when it is moved around
            if (LoadComplete)
            {
                OnLoadComplete(this, EventArgs.Empty);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            PropertyDescriptor property = (PropertyDescriptor) properties["Expressions"];
            if (property != null) {
                properties["Expressions"] = TypeDescriptor.CreateProperty(this.GetType(), property, BrowsableAttribute.No);
            }
        }

        /// <summary>
        ///    dynamically transform DeviceSpecific element to a server control,
        ///    called from OnBehaviorAttached
        /// </summary>
        private void PrefixDeviceSpecificTags()
        {
            IHTMLElement htmlElement = (IHTMLElement) DesignTimeElementInternal;
            Debug.Assert(htmlElement != null,
                "Invalid HTML element in FormDesigner.OnBehaviorAttached");

            IWebFormReferenceManager refMgr =
                (IWebFormReferenceManager) GetService(typeof(IWebFormReferenceManager));
            Debug.Assert(refMgr != null, "Did not get back IWebFormReferenceManager service.");

            String tagPrefix = refMgr.GetTagPrefix(typeof(DeviceSpecific));
            Debug.Assert(tagPrefix != null && tagPrefix.Length > 0, "TagPrefix is invalid");

            IHTMLElementCollection allChildren = (IHTMLElementCollection) htmlElement.GetChildren();
            if (null != allChildren)
            {
                bool substitutions = false;
                int nestingLevel = 0;
                String modifiedInnerHTML = String.Empty;
                for (Int32 i = 0; i < allChildren.GetLength(); i++)
                {
                    IHTMLElement htmlChild = (IHTMLElement) allChildren.Item(i, 0);
                    Debug.Assert(null != htmlChild, "htmlChild is null");
                    String childContent = htmlChild.GetOuterHTML();
                    String childUpperContent = childContent.ToUpper(CultureInfo.InvariantCulture);
                    if (childContent.StartsWith("<", StringComparison.Ordinal) &&
                        !(childContent.StartsWith("</", StringComparison.Ordinal) || (childContent.EndsWith("/>", StringComparison.Ordinal))))
                    {
                        if (!childUpperContent.StartsWith("<" + tagPrefix.ToUpper(CultureInfo.InvariantCulture) + ":", StringComparison.Ordinal))
                        {
                            nestingLevel++;
                        }
                    }
                    else if (childContent.StartsWith("</", StringComparison.Ordinal))
                    {
                        nestingLevel--;
                    }
                    if (1 == nestingLevel &&
                        childUpperContent.StartsWith("<DEVICESPECIFIC", StringComparison.Ordinal) &&
                        childUpperContent.EndsWith(">", StringComparison.Ordinal))
                    {
                        Debug.Assert(substitutions == false, "substitutions is true");
                        modifiedInnerHTML += "<" + tagPrefix + ":DeviceSpecific runat=\"server\">\r\n";
                        substitutions = true;
                    }
                    else if (1 == nestingLevel &&
                             childUpperContent.StartsWith("<DEVICESPECIFIC", StringComparison.Ordinal) &&
                             childUpperContent.EndsWith("/>", StringComparison.Ordinal))
                    {
                        modifiedInnerHTML += "<" + tagPrefix + ":DeviceSpecific runat=\"server\"></" + tagPrefix + ":DeviceSpecific>\r\n";
                        substitutions = true;
                    }
                    else if (0 == nestingLevel && 0 == String.Compare(childUpperContent, "</DEVICESPECIFIC>", StringComparison.Ordinal))
                    {
                        Debug.Assert(substitutions == true, "substitutions is false");
                        modifiedInnerHTML += "</" + tagPrefix + ":DeviceSpecific>\r\n";
                    }
                    else
                    {
                        modifiedInnerHTML += childContent + "\r\n";
                    }
                }
                if (substitutions)
                {
                    _shouldDirtyPage = true;
                    htmlElement.SetInnerHTML(modifiedInnerHTML);
                }
            }
        }

        /// <summary>
        ///    Reload the cached Behavior states
        /// </summary>
        private void ReloadBehaviorState()
        {
            Debug.Assert(Behavior != null && _behaviorAttributes != null);

            IDictionaryEnumerator enumerator = _behaviorAttributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                String key = (String)enumerator.Key;
                Object obj = _behaviorAttributes[key];

                Behavior.SetStyleAttribute(key, true, obj, true);
            }
        }

        /// <summary>
        ///    Remove the attribute from Behavior
        /// </summary>
        /// <param name="attribute">
        ///    attribute that need to be removed.
        /// </param>
        protected void RemoveBehaviorStyle(String attribute)
        {
            Debug.Assert (_behaviorAttributes != null);

            if (Behavior != null)
            {
                Behavior.RemoveStyleAttribute(attribute, true, true);
            }

            // also remove the cached attribute
            _behaviorAttributes.Remove(attribute);
        }

        /// <summary>
        ///    Apply the style attribute to Behavior
        /// </summary>
        /// <param name="attribute">
        ///    attribute that needs to be applied to Behavior
        /// </param>
        /// <param name="obj">
        ///    value to apply
        /// </param>
        protected void SetBehaviorStyle(String attribute, Object obj)
        {
            Debug.Assert (obj != null, "null object passed in!");
            Debug.Assert (_behaviorAttributes != null);

            // here we cache the value;
            // Note that the value is cached even if Behavior is not available,
            // this is because this method could be called between Behavior
            // detached and attached events, we want to re-apply these lost
            // attributes when Behavior is attached again.
            _behaviorAttributes[attribute] = obj;

            if (Behavior == null)
            {
                return;
            }
            Behavior.SetStyleAttribute(attribute, true, obj, true);
        }

        /// <summary>
        ///    This method will be called only once when the control is first created.
        /// </summary>
        protected virtual void SetControlDefaultAppearance()
        {
            // Default border appearance
            SetBehaviorStyle("borderWidth", "1px");
            SetBehaviorStyle("borderColor", ColorTranslator.ToHtml(SystemColors.ControlDark));

            // Default margin, paddings for container controls.
            SetBehaviorStyle("paddingTop", "8px");
            SetBehaviorStyle("paddingBottom", "8px");
            SetBehaviorStyle("paddingRight", "4px");
            SetBehaviorStyle("paddingLeft", "5px");
            SetBehaviorStyle("marginTop", "3px");
            SetBehaviorStyle("marginBottom", "3px");
            SetBehaviorStyle("marginRight", "5px");
            SetBehaviorStyle("marginLeft", "5px");

            // Setup background parameters
            SetBehaviorStyle("backgroundRepeat", "no-repeat");
            SetBehaviorStyle("backgroundAttachment", "fixed");
            SetBehaviorStyle("backgroundPositionX", "left");
            SetBehaviorStyle("backgroundPositionY", "top");

            // Container sze info.
            SetBehaviorStyle("height", GetDefaultSize().Height);
            SetBehaviorStyle("width", GetDefaultSize().Width);
        }

        /// <summary>
        ///   Update the designtime rendering for the container control
        /// </summary>
        public void UpdateRendering()
        {
            _mobileControl.RefreshStyle();
            ApplyPropertyToBehavior(null);
        }

        /// <summary>
        ///   Update the designtime rendering for the container control and all controls
        ///   inside this container control.
        /// </summary>
        private void UpdateRenderingRecursive()
        {
            UpdateRendering();

            if (IMobileWebFormServices != null)
            {
                IMobileWebFormServices.UpdateRenderingRecursive(_mobileControl);
            }
        }
    }
}
