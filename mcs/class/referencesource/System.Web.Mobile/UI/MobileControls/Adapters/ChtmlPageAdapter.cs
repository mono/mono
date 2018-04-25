//------------------------------------------------------------------------------
// <copyright file="ChtmlPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Web.Mobile;
using System.Web.UI.MobileControls.Adapters;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * ChtmlPageAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ChtmlPageAdapter : HtmlPageAdapter
    {
        private const int DefaultPageWeight = 800;
        private const String _postedFromOtherFile = ".";

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.ChtmlPageAdapter"]/*' />
        public ChtmlPageAdapter() : base(DefaultPageWeight)
        {
        }

        /////////////////////////////////////////////////////////////////////
        //  Static method used for determining if device should use
        //  this adapter
        /////////////////////////////////////////////////////////////////////

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.DeviceQualifies"]/*' />
        public new static bool DeviceQualifies(HttpContext context)
        {
            String type = ((MobileCapabilities)context.Request.Browser).PreferredRenderingType;
            bool javascriptSupported = context.Request.Browser.JavaScript;
            bool qualifies = (type == MobileCapabilities.PreferredRenderingTypeHtml32 ||
                              type == MobileCapabilities.PreferredRenderingTypeChtml10)
                             && !javascriptSupported;
            return qualifies;
        }
        
        /////////////////////////////////////////////////////////////////////
        //  IControlAdapter implementation
        /////////////////////////////////////////////////////////////////////

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.RenderPostBackEvent"]/*' />
        public override void RenderPostBackEvent(HtmlMobileTextWriter writer, 
                                                String target, 
                                                String argument)
        {
            // Since it doesn't have scripts, the CHTML adapter
            // only supports URL postback events.

            RenderUrlPostBackEvent(writer, target, argument);
        }

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.EventSourceKey"]/*' />
        protected override String EventSourceKey
        {
            get
            {
                return Constants.EventSourceID;
            }
        }

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.EventArgumentKey"]/*' />
        protected override String EventArgumentKey
        {
            get
            {
                return Constants.EventArgumentID;
            }
        }

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.RenderPostBackHeader"]/*' />
        public override void RenderPostBackHeader(HtmlMobileTextWriter writer, Form form)
        {
            bool postBack = form.Action.Length == 0;

            RenderPageState(writer);
            if (!postBack)
            {
                writer.WriteHiddenField(EventSourceKey, _postedFromOtherFile);
            }
            else if (Page.ClientViewState == null)
            {
                // The empty event source variable is used to identify a
                // postback request
                writer.WriteHiddenField(EventSourceKey, String.Empty);
            }

            RenderHiddenVariables(writer);
        }

        /////////////////////////////////////////////////////////////////////
        //  IPageAdapter implementation
        /////////////////////////////////////////////////////////////////////

        // ==================================================================
        // For browser that doesn't support javascript, like cHTML browser,
        // control id and its corresponding value are specially encoded in
        // the post back data collection.  This method is to extract the
        // encoded info and put the info back to the collection in an
        // expected format that is understood by ASP.NET Frameworks so post
        // back event is raised correctly.
        // Note other control adapters should do the encoding accordinly so
        // the data can be decoded properly here.
        //
        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.DeterminePostBackMode"]/*' />
        public override NameValueCollection DeterminePostBackMode
        (
            HttpRequest request,
            String postEventSourceID,
            String postEventArgumentID,
            NameValueCollection baseCollection
        )
        {
            if (baseCollection != null && baseCollection[EventSourceKey] == _postedFromOtherFile)
            {
                return null;
            }
            else if (request == null)
            {
                return baseCollection;
            }
            else if (String.Compare(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return CollectionFromForm(request.Form,
                                          postEventSourceID,
                                          postEventArgumentID);
            }
            else if (request.QueryString.Count == 0)
            {
                return baseCollection;
            }
            else
            {
                return CollectionFromQueryString(request.QueryString,
                                                 postEventSourceID,
                                                 postEventArgumentID);
            }
        }

        /// <include file='doc\ChtmlPageAdapter.uex' path='docs/doc[@for="ChtmlPageAdapter.CreateTextWriter"]/*' />
        public override HtmlTextWriter CreateTextWriter(TextWriter writer)
        {
            return new ChtmlMobileTextWriter(writer, Device);
        }

        private NameValueCollection CollectionFromQueryString(
            NameValueCollection queryString,
            String postEventSourceID,
            String postEventArgumentID)
        {
            NameValueCollection collection = new NameValueCollection();
            bool isPostBack = false;

            for (int i = 0; i < queryString.Count; i++)
            {
                String name = queryString.GetKey(i);

                // Supposingly, we should double check if the control id
                // is real or not by checking against the control tree.
                // However, the tree can't be checked because it hasn't
                // been built at this stage.  And this is the only place
                // we can override the value collection.  We just need to
                // assume the control adapters are setting the id and
                // value accordingly.

                // ASSUMPTION: In query string, besides the expected
                // name/value pairs (ViewStateID, EventSource and
                // EventArgument), there are hidden variables, control
                // id/value pairs (if the form submit method is GET), unique
                // file path suffix variable and custom query string text.
                // They will be in the above order if any of them present.
                // Hidden variables and control id/value pairs should be added
                // back to the collection intactly, but the other 2 items
                // should not be added to the collection.

                // name can be null if there is a query name without equal
                // sign appended.  We should just ignored it in this case.
                if (name == null)
                {
                    continue;
                }
                else if (name == MobilePage.ViewStateID)
                {
                    collection.Add(MobilePage.ViewStateID, queryString.Get(i));
                    isPostBack = true;
                }
                else if (name == Constants.EventSourceID)
                {
                    collection.Add(postEventSourceID, queryString.Get(i));
                    isPostBack = true;
                }
                else if (name == Constants.EventArgumentID)
                {
                    collection.Add(postEventArgumentID, queryString.Get(i));
                }
                else if (Constants.UniqueFilePathSuffixVariable.StartsWith(name, StringComparison.Ordinal))
                {
                    // At this point we know that the rest of them is
                    // the custom query string text, so we are done.
                    break;
                }
                else
                {
                    AddValues(queryString, name, collection);
                }
            }

            if (collection.Count == 0 || !isPostBack)
            {
                // Returning null to indicate this is not a postback
                return null;
            }
            else
            {
                return collection;
            }
        }

        // ==================================================================
        // The complexity (multiple if statements) of this method is due to
        // workarounds for different targeted devices and limitation on non-
        // javascript html browser.
        //
        private NameValueCollection CollectionFromForm(
            NameValueCollection form,
            String postEventSourceID,
            String postEventArgumentID)
        {
            int i;
            int count = form.Count;
            NameValueCollection collection = new NameValueCollection();
            bool isPostBack = false;

            // continue statements are used below to simplify the logic and
            // make people easier to follow and maintain the code.
            for (i = 0; i < count; i++)
            {
                String name = form.GetKey(i);

                // 1. Some browser returns the name of a password textbox
                // only without the expected character "=" if the textbox is
                // empty.  This causes the key to be null and the name to be
                // the value of the collection item when the returned form
                // content is parsed in HttpValueCollection.  In this case,
                // we need to reverse the setting with the value as the name
                // and empty string as the value so subsequent manipulations
                // of the collection work correctly.
                if (name == null)
                {
                    if (AddEmptyStringValues(form.GetValues(i), collection)) {
                        isPostBack = true;
                    }
                    continue;
                }

                // 2. Pager navigation is rendered by buttons which have the
                // targeted page number appended to the form id after
                // PagePrefix which is a constant string to identify this
                // special case.  E.g. ControlID__PG_2
                int index = name.LastIndexOf(Constants.PagePrefix, StringComparison.Ordinal);
                if (index != -1)
                {
                    // We need to associate the form id with the event source
                    // id and the page number with the event argument id in
                    // order to have the event raised properly by ASP.NET
                    int pageBeginPos = index + Constants.PagePrefix.Length;
                    collection.Add(postEventSourceID,
                                   name.Substring(0, index));
                    collection.Add(postEventArgumentID,
                                   name.Substring(pageBeginPos,
                                              name.Length - pageBeginPos));
                    continue;
                }

                // 3. This special case happens when A. SelectionList control is
                // with property SelectType equal to CheckBox or
                // MultiSelectListBox, and the device itself doesn't handle
                // multiple check boxes correctly. or B. Browser requires the
                // ID of the input element to be unique during postbacks.
                //
                // In this case, the control (SelectionList or TextBox) adapter 
                // appended special characters as a suffix of the actual control
                // id. That should be stripped off when detected.
                if (Device.RequiresUniqueHtmlCheckboxNames ||
                    Device.RequiresUniqueHtmlInputNames)
                {
                    index = name.LastIndexOf(
                        Constants.SelectionListSpecialCharacter);

                    if (index != -1)
                    {
                        String value = form.Get(i);
                        if (!String.IsNullOrEmpty(value))
                        {
                            if(Device.RequiresAttributeColonSubstitution)
                            {
                                collection.Add(name.Substring(0, index).Replace(',',':'), value);
                            }
                            else
                            {
                                collection.Add(name.Substring(0, index), value);
                            }
                            continue;
                        }
                    }
                }

                // 4. This is to determine if the request is a postback from
                // the same mobile page.
                if (name == MobilePage.ViewStateID ||
                    name == EventSourceKey)
                {
                    isPostBack = true;
                }

                // Default case, just preserve the value(s)
                AddValues(form, name, collection);
            }

            if (collection.Count == 0 || !isPostBack)
            {
                // Returning null to indicate this is not a postback
                return null;
            }
            else
            {
                return collection;
            }
        }

        // Helper function to add empty string as value for the keys
        private bool AddEmptyStringValues(String [] keys,
                                        NameValueCollection targetCollection)
        {
            bool result = false;
            foreach (String key in keys)
            {
                if (key == MobilePage.ViewStateID ||
                    key == EventSourceKey) {
                    result = true;
                }
                targetCollection.Add(key, String.Empty);
            }
            return result;
        }

        // Helper function to add multiple values for the same key
        private void AddValues(NameValueCollection sourceCollection,
                               String sourceKey,
                               NameValueCollection targetCollection)
        {
            String [] values = sourceCollection.GetValues(sourceKey);
            foreach (String value in values)
            {
                if(Device.RequiresAttributeColonSubstitution)
                {
                    targetCollection.Add(sourceKey.Replace(',',':'), value);
                }
                else
                {
                    targetCollection.Add(sourceKey, value);
                }
            }
        }
    }
}
