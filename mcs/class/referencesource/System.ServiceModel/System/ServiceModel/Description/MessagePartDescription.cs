//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel.Security;

    [DebuggerDisplay("Name={name}, Namespace={ns}, Type={Type}, Index={index}}")]
    public class MessagePartDescription
    {
        XmlName name;
        string ns;        
        int index;
        Type type;
        int serializationPosition;
        ProtectionLevel protectionLevel;
        bool hasProtectionLevel;
        MemberInfo memberInfo;
        ICustomAttributeProvider additionalAttributesProvider;

        bool multiple;
        string baseType;
        string uniquePartName;

        public MessagePartDescription(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name", SR.GetString(SR.SFxParameterNameCannotBeNull));
            }

            this.name = new XmlName(name, true /*isEncoded*/);

            if (!string.IsNullOrEmpty(ns))
            {
                NamingHelper.CheckUriParameter(ns, "ns");
            }

            this.ns = ns;
        }

        internal MessagePartDescription(MessagePartDescription other)
        {
            this.name = other.name;
            this.ns = other.ns;
            this.index = other.index;
            this.type = other.type;
            this.serializationPosition = other.serializationPosition;
            this.hasProtectionLevel = other.hasProtectionLevel;
            this.protectionLevel = other.protectionLevel;
            this.memberInfo = other.memberInfo;
            this.multiple = other.multiple;
            this.additionalAttributesProvider = other.additionalAttributesProvider;
            this.baseType = other.baseType;
            this.uniquePartName = other.uniquePartName;
        }

        internal virtual MessagePartDescription Clone()
        {
            return new MessagePartDescription(this);
        }

        internal string BaseType
        {
            get { return this.baseType; }
            set { this.baseType = value; }
        }

        internal XmlName XmlName
        {
            get { return this.name; }
        }

        internal string CodeName
        {
            get { return this.name.DecodedName; }
        }

        public string Name
        {
            get { return this.name.EncodedName; }
        }

        public string Namespace
        {
            get { return this.ns; }            
        }

        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        
        [DefaultValue(false)]
        public bool Multiple
        {
            get { return this.multiple; }
            set { this.multiple = value; }
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

        public bool HasProtectionLevel
        {
            get { return this.hasProtectionLevel; }
        }

        public MemberInfo MemberInfo
        {
            get { return this.memberInfo; }
            set { this.memberInfo = value; }
        }

        internal ICustomAttributeProvider AdditionalAttributesProvider
        {
            get { return this.additionalAttributesProvider ?? this.memberInfo; }
            set { this.additionalAttributesProvider = value; }
        }

        internal string UniquePartName
        {
            get { return this.uniquePartName; }
            set { this.uniquePartName = value; }
        }

        internal int SerializationPosition
        {
            get { return serializationPosition; }
            set { serializationPosition = value; }
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }
    }
}
