//------------------------------------------------------------------------------
// <copyright file="AdRotator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Web.Mobile;
using System.Web.UI.WebControls;
using System.Web.Util;
using WebCntrls = System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile AdRotator class.
     * The AdRotator control is for rotating advertisement links every time the
     * same page is revisited.
     *
     * This class aggregates the corresponding ASP.NET AdRotator for delegating
     * the random selection task of advertisement info to the aggregated
     * class.  The ad info is selected during the PreRender phase of the
     * aggregated control (So the aggregated control needs to have the
     * property Visible set to true when entering the PreRender process).
     * For markup adapters that collect the selected ad info for rendering,
     * they should subscribe to AdCreated event property and collect the ad
     * info through the event argument.
     *
     * This class also contains a mobile Image control for delegating the
     * rendering since AdRotator's rendering is the same as Image's rendering
     * by setting the corresponding properties on the control.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator"]/*' />
    [
        DefaultEvent("AdCreated"),
        DefaultProperty("AdvertisementFile"),
        Designer(typeof(System.Web.UI.Design.MobileControls.AdRotatorDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerAdRotatorAdapter)),
        ToolboxData("<{0}:AdRotator runat=\"server\"></{0}:AdRotator>"),
        ToolboxItem(typeof(System.Web.UI.Design.WebControlToolboxItem))
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class AdRotator : MobileControl
    {
        private WebCntrls.AdRotator _webAdRotator;
        private Image _image = new Image();

        private static readonly Object EventAdCreated = new Object();
        private const String ImageKeyDefault = "ImageUrl";
        private const String NavigateUrlKeyDefault = "NavigateUrl";

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.AdRotator"]/*' />
        public AdRotator() : base()
        {
            _webAdRotator = CreateWebAdRotator();

            _image.EnableViewState = false;

            this.Controls.Add(_webAdRotator);
            this.Controls.Add(_image);

            // The default value of the Target property of the web AdRotator is
            // set to "_top".  Since we are not exposing this property, we need
            // to explicity set it to empty string so this property will not be
            // shown in the rendered markup when the web AdRotator is used to do
            // the rendering.
            _webAdRotator.Target = String.Empty;

            // Due to the fact that C# compiler doesn't allow direct
            // manipulation of event properties outside of the class that
            // defines the event variable, the way we delegate the event
            // handlers to the aggregated web control is to provide a wrapper
            // to capture the raised event from the aggregated control and
            // apply the event argument to the event handlers subscribed to
            // this class.
            AdCreatedEventHandler adCreatedEventHandler =
                new AdCreatedEventHandler(WebAdCreated);

            _webAdRotator.AdCreated += adCreatedEventHandler;
        }

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.CreateWebAdRotator"]/*' />
        protected virtual WebCntrls.AdRotator CreateWebAdRotator()
        {
            return new WebCntrls.AdRotator();
        }

        ////////////////////////////////////////////////////////////////////////
        // Mimic the properties exposed in the original AdRotator.
        // The properties are got and set directly from the original AdRotator.
        ////////////////////////////////////////////////////////////////////////

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.AdvertisementFile"]/*' />
        /// <summary>
        /// <para>
        /// Gets or sets the path to the XML file that contains advertisement data.
        /// </para>
        /// </summary>
        /// <value>
        /// <para>
        /// The path to the XML file containing the properties of the advertisements to
        /// render in the <see langword='AdRotator'/>.
        /// </para>
        /// </value>
        [
            Bindable(true),
            DefaultValue(""),
            Editor(typeof(System.Web.UI.Design.XmlUrlEditor), typeof(UITypeEditor)),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.AdRotator_AdvertisementFile)
        ]
        public String AdvertisementFile
        {
            get
            {
                return _webAdRotator.AdvertisementFile;
            }
            set
            {
                _webAdRotator.AdvertisementFile = value;
            }
        }

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.KeywordFilter"]/*' />
        /// <summary>
        /// <para>
        /// Gets or sets a keyword used to match related advertisements in the ad file.
        /// </para>
        /// </summary>
        /// <value>
        /// <para>
        /// The keyword used to identify advertisements within a specific catagory.
        /// </para>
        /// </value>
        /// <remarks>
        /// <para>
        /// If the ad source is AdvertisementFile and this property is not empty, an ad
        /// with a matching keyword will be selected.
        /// </para>
        /// <para>
        /// If the ad source is AdvertisementFile and this property set, but no match
        /// exists, a blank image is displayed and a trace warning is generated.
        /// </para>
        /// If this property is not set, keyword filtering is not used to select an ad.
        /// </remarks>
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.AdRotator_KeywordFilter)
        ]
        public String KeywordFilter
        {
            get
            {
                return _webAdRotator.KeywordFilter;
            }
            set
            {
                _webAdRotator.KeywordFilter = value;
            }
        }

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.ImageKey"]/*' />
        [
            Bindable(true),
            DefaultValue(ImageKeyDefault),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.AdRotator_ImageKey)
        ]
        public String ImageKey
        {
            get
            {
                String s = (String) ViewState["ImageKey"];
                return((s != null) ? s : ImageKeyDefault);
            }
            set
            {
                ViewState["ImageKey"] = value;
            }
        }

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.NavigateUrlKey"]/*' />
        [
            Bindable(true),
            DefaultValue(NavigateUrlKeyDefault),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.AdRotator_NavigateUrlKey)
        ]
        public String NavigateUrlKey
        {
            get
            {
                String s = (String) ViewState["NavigateUrlKey"];
                return((s != null) ? s : NavigateUrlKeyDefault);
            }
            set
            {
                ViewState["NavigateUrlKey"] = value;
            }
        }

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.AdCreated"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.AdRotator_AdCreated)
        ]
        public event AdCreatedEventHandler AdCreated
        {
            add
            {
                Events.AddHandler(EventAdCreated, value);
            }
            remove
            {
                Events.RemoveHandler(EventAdCreated, value);
            }
        }

        // protected method (which can be overridden by subclasses) for
        // raising user events
        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.OnAdCreated"]/*' />
        protected virtual void OnAdCreated(AdCreatedEventArgs e)
        {
            AdCreatedEventHandler handler = (AdCreatedEventHandler)Events[EventAdCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <include file='doc\AdRotator.uex' path='docs/doc[@for="AdRotator.Render"]/*' />
        protected override void Render(HtmlTextWriter writer)
        {
            const String accesskeyName = "accesskey";

            // Delegate specific custom attribute to the child Image control
            String accesskey = ((IAttributeAccessor) this).GetAttribute(accesskeyName);
            if (!String.IsNullOrEmpty(accesskey))
            {
                _image.CustomAttributes[accesskeyName] = accesskey;
            }

            _image.RenderControl(writer);
        }

        private void WebAdCreated(Object sender, AdCreatedEventArgs e)
        {
            // Override the value since it may have been changed by device
            // select

            // AdProperties can be null when ad file is not specified
            // correctly.
            if (e.AdProperties != null)
            {
                e.ImageUrl = (String) e.AdProperties[ImageKey];
                e.NavigateUrl = (String) e.AdProperties[NavigateUrlKey];
            }

            // Then invoke user events for further manipulation specified by
            // user
            OnAdCreated(e);

            // Finally, set the necessary properties to the base Image class
            _image.ImageUrl = ResolveAdRotatorUrl(e.ImageUrl);
            _image.AlternateText = e.AlternateText;
            _image.NavigateUrl = ResolveAdRotatorUrl(e.NavigateUrl);
        }

        // Helper function adopted from ASP.NET AdRotator class (modified
        // slightly)
        private String ResolveAdRotatorUrl(String relativeUrl)
        {
            if (relativeUrl == null)
            {
                return String.Empty;
            }

            // check if it is already absolute, or points to another form
            if (!UrlPath.IsRelativeUrl(relativeUrl) ||
                relativeUrl.StartsWith(Constants.FormIDPrefix, StringComparison.Ordinal))
            {
                return relativeUrl;
            }

            // Deal with app relative syntax (e.g. ~/foo)
            string tplSourceDir = UrlPath.MakeVirtualPathAppAbsolute(TemplateSourceDirectory);

            // For the AdRotator, use the AdvertisementFile directory as the
            // base, and fall back to the page/user control location as the
            // base.
            String absoluteFile = UrlPath.Combine(tplSourceDir,
                                                  AdvertisementFile);
            String fileDirectory = UrlPath.GetDirectory(absoluteFile);

            String baseUrl = String.Empty;
            if (fileDirectory != null)
            {
                baseUrl = fileDirectory;
            }
            if (baseUrl.Length == 0)
            {
                baseUrl = tplSourceDir;
            }
            if (baseUrl.Length == 0)
            {
                return relativeUrl;
            }

            // make it absolute
            return UrlPath.Combine(baseUrl, relativeUrl);
        }
    }
}
