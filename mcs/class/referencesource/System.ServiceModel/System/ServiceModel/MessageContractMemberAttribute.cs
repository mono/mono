//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Net.Security;

    public abstract class MessageContractMemberAttribute : Attribute
    {
        string name;
        string ns;
        bool isNameSetExplicit;
        bool isNamespaceSetExplicit;
        ProtectionLevel protectionLevel = ProtectionLevel.None;
        bool hasProtectionLevel = false;

        internal const string NamespacePropertyName = "Namespace";
        public string Namespace
        {
            get { return ns; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.Length > 0)
                {
                    NamingHelper.CheckUriProperty(value, "Namespace");
                }
                ns = value;
                isNamespaceSetExplicit = true;
            }
        }

        internal bool IsNamespaceSetExplicit
        {
            get { return isNamespaceSetExplicit; }
        }

        internal const string NamePropertyName = "Name";
        public string Name
        {
            get { return name; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.SFxNameCannotBeEmpty)));
                }
                name = value; isNameSetExplicit = true;
            }
        }

        internal bool IsNameSetExplicit
        {
            get { return isNameSetExplicit; }
        }

        internal const string ProtectionLevelPropertyName = "ProtectionLevel";
        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }
    }
}

