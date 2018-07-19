//------------------------------------------------------------------------------
// <copyright file="MobileCategoryAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.MobileControls
{
    using System;
    using System.ComponentModel;   
    using System.Diagnostics;

    [
        AttributeUsage(AttributeTargets.All)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class MobileCategoryAttribute : CategoryAttribute
    {
        private static MobileCategoryAttribute deviceSpecific;
        private const String _categoryDeviceSpecific = "Category_DeviceSpecific";
        private const String _categoryPrefix = "Category_";
        private const String _usCategoryDeviceSpecific = "Device Specific";
        private static readonly int _prefixLength = _categoryPrefix.Length;

        // Initializes a new instance of the CategoryAttribute class.
        internal /*public*/ MobileCategoryAttribute(String category) : base(category) 
        {
        }

        internal /*public*/ static CategoryAttribute DeviceSpecific 
        {
            get 
            {
                if (deviceSpecific == null) 
                {
                    deviceSpecific = new MobileCategoryAttribute(_categoryDeviceSpecific);
                }
                return deviceSpecific;
            }
        }

        // This method is called the first time the category property
        // is accessed.  It provides a way to lookup a localized string for
        // the given category.  Classes may override this to add their
        // own localized names to categories.  If a localized string is
        // available for the given value, the method should return it.
        // Otherwise, it should return null.
        protected override String GetLocalizedString(String value)
        {
            Debug.Assert(value != null);
            String localizedValue = null;

            int index = value.IndexOf(_categoryPrefix, StringComparison.Ordinal);

            // mobile controls have "Category_" prefix.
            if (index == 0)
            {
                String categoryName = value.Substring(_prefixLength);

                // see if already defined in base class.
                localizedValue = base.GetLocalizedString(categoryName);
            }

            // fall back to local resource string.
            if (localizedValue == null)
            {
                localizedValue = (String) SR.GetString(value);
            }

            if (localizedValue == null && value.Equals(_usCategoryDeviceSpecific))
            {
                localizedValue = (String) SR.GetString(SR.Category_DeviceSpecific);
            }

            // This attribute is internal, and we should never have a missing resource string.
            Debug.Assert(localizedValue != null, 
                "All MobileWebForms category attributes should have localized strings.  Category '"
                + value + "' not found.");
            return localizedValue;
        }
    }
}
