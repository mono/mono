//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.CodeDom;
    using System.ServiceModel.Security;
    using System.Diagnostics;
    using System.Net.Security;

    [DebuggerDisplay("Name={name}, Action={action}, DetailType={detailType}")]
    public class FaultDescription
    {
        string action;
        Type detailType;
        CodeTypeReference detailTypeReference;
        XmlName elementName;
        XmlName name;
        string ns;
        ProtectionLevel protectionLevel;
        bool hasProtectionLevel;

        public FaultDescription(string action)
        {
            if (action == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));

            this.action = action;
        }
        
        public string Action
        {
            get { return action; }
            internal set { action = value; }
        }

        // Not serializable on purpose, metadata import/export cannot
        // produce it, only available when binding to runtime
        public Type DetailType
        {
            get { return detailType; }
            set { detailType = value; }
        }

        internal CodeTypeReference DetailTypeReference
        {
            get { return detailTypeReference; }
            set { detailTypeReference = value; }
        }

        public string Name
        {
            get { return name.EncodedName; }
            set { SetNameAndElement(new XmlName(value, true /*isEncoded*/)); }
        }

        public string Namespace
        {
            get { return ns; }
            set { ns = value; }
        }

        internal XmlName ElementName
        {
            get { return elementName; }
            set { elementName = value; }
        }

        public ProtectionLevel ProtectionLevel
        {
            get { return this.protectionLevel; }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        public bool ShouldSerializeProtectionLevel()
        {
            return this.HasProtectionLevel;
        }

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }

        internal void SetNameAndElement(XmlName name)
        {
            this.elementName = this.name = name;
        }

        internal void SetNameOnly(XmlName name)
        {
            this.name = name;
        }
    }
}
