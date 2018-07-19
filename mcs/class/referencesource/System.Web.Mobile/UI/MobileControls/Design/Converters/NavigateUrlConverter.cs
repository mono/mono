//------------------------------------------------------------------------------
// <copyright file="NavigateUrlConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections;
    using System.Globalization;
    using System.Web.UI.MobileControls;

    /// <summary>
    ///     Subclass of FormConverter to handle the special case where we want
    ///     to select a form OR a valid URL to navigate to.
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class NavigateUrlConverter : FormConverter
    {
        protected override ArrayList GetControls(ITypeDescriptorContext context)
        {
            ArrayList formList = base.GetControls(context);

            // We disable the "Select Url..." option in multi-selected case
            if (formList != null && !(context.Instance is Array))
            {
                formList.Insert(0, SR.GetString(SR.NavigateUrlConverter_SelectURITarget));
            }

            return formList;
        }
        
        protected override String ProcessControlId(String id)
        {
            return "#" + id;
        }
        
        /// <summary>
        ///     url = new value in OnPropertyChanged, we check to see if we need to
        ///           browse for the url.  If not, we just return this value.
        ///     oldUrl = old value of URL, used to initialize URL builder and returned
        ///              if the user cancels.
        /// </summary>
        internal static String GetUrl(IComponent component, String url, String oldUrl)
        {
            if(url == SR.GetString(SR.NavigateUrlConverter_SelectURITarget))
            {
                url = UrlBuilder.BuildUrl(
                    component, 
                    null,
                    oldUrl,
                    SR.GetString(SR.UrlPicker_DefaultCaption),
                    SR.GetString(SR.UrlPicker_DefaultFilter)
                );
                if (url == null)
                {
                    url = oldUrl;
                }
            }
            return url;
        }
    }
}
