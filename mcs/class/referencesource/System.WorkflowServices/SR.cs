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
        internal const string SendActivityDescription = "SendActivityDescription";
        internal const string ReceiveActivityDescription = "ReceiveActivityDescription";
        internal const string Receive_OperationValidation_Description = "Receive_OperationValidation_Description";
        internal const string Receive_CanCreateInstance_Description = "Receive_CanCreateInstance_Description";
        internal const string Receive_ContextToken_Description = "Receive_ContextToken_Description";
        internal const string Receive_FaultMessage_Description = "Receive_FaultMessage_Description";
        internal const string Receive_OperationInfo_Description = "Receive_OperationInfo_Description";
        internal const string Send_AfterResponse_Description = "Send_AfterResponse_Description";
        internal const string Send_BeforeSend_Description = "Send_BeforeSend_Description";
        internal const string Send_ChannelToken_Description = "Send_ChannelToken_Description";
        internal const string Send_CustomAddress_Description = "Send_CustomAddress_Description";
        internal const string Send_OperationInfo_Description = "Send_OperationInfo_Description";
        internal const string ContextToken_Name_Description = "ContextToken_Name_Description";
        internal const string ContextToken_OwnerActivityName_Description = "ContextToken_OwnerActivityName_Description";
        internal const string ChannelToken_EndpointName_Description = "ChannelToken_EndpointName_Description";
        internal const string ChannelToken_Name_Description = "ChannelToken_Name_Description";
        internal const string ChannelToken_OwnerActivityName_Description = "ChannelToken_OwnerActivityName_Description";
        
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
        internal const string Activity = "Activity";
        internal const string Standard = "Standard";
        internal const string Handlers = "Handlers";

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
