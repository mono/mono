//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.All)]
    sealed class SR2DescriptionAttribute : DescriptionAttribute
    {
        public SR2DescriptionAttribute(string description)
        {
            DescriptionValue = SR2.ResourceManager.GetString(description, SR2.Culture);
        }

        public SR2DescriptionAttribute(string description, string resourceSet)
        {
            ResourceManager rm = new ResourceManager(resourceSet, Assembly.GetExecutingAssembly());
            DescriptionValue = rm.GetString(description);
            Fx.Assert(DescriptionValue != null, string.Format(CultureInfo.CurrentCulture, "String resource {0} not found.", new object[] { description }));
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    sealed class SR2CategoryAttribute : CategoryAttribute
    {
        string resourceSet = String.Empty;

        public SR2CategoryAttribute(string category)
            : base(category)
        {
        }

        public SR2CategoryAttribute(string category, string resourceSet)
            : base(category)
        {
            this.resourceSet = resourceSet;
        }

        protected override string GetLocalizedString(string value)
        {
            if (this.resourceSet.Length > 0)
            {
                ResourceManager rm = new ResourceManager(resourceSet, Assembly.GetExecutingAssembly());
                String localizedString = rm.GetString(value);
                Fx.Assert(localizedString != null, string.Format(CultureInfo.CurrentCulture, "String resource {0} not found.", new object[] { value }));
                return localizedString;
            }
            else
            {
                return SR2.ResourceManager.GetString(value, SR2.Culture);
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    sealed class SR2DisplayNameAttribute : DisplayNameAttribute
    {
        public SR2DisplayNameAttribute(string name)
        {
            DisplayNameValue = SR2.ResourceManager.GetString(name, SR2.Culture);
        }

        public SR2DisplayNameAttribute(string name, string resourceSet)
        {
            ResourceManager rm = new ResourceManager(resourceSet, Assembly.GetExecutingAssembly());
            DisplayNameValue = rm.GetString(name);
            Fx.Assert(DisplayNameValue != null, string.Format(CultureInfo.CurrentCulture, "String resource {0} not found.", new object[] { name }));
        }
    }

    /// <summary>
    ///    This is a stub for auto-generated resource class, providing GetString function. Usage:
    ///
    ///        string s = SR2.GetString(SR2.MyIdenfitier);
    /// </summary>
    sealed partial class SR2
    {
        internal static string GetString(string name, params object[] args)
        {
            return GetString(resourceCulture, name, args);
        }
        internal static string GetString(CultureInfo culture, string name, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                return string.Format(culture, name, args);
            }
            else
            {
                return name;
            }
        }
    }
}
