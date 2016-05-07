//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Xml;
    using System.Security;

    public sealed partial class ParameterElement : ConfigurationElement
    {
        public ParameterElement()
        {
        }

        public ParameterElement(string typeName)
            : this()
        {
            if (String.IsNullOrEmpty(typeName))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            this.Type = typeName;
        }

        public ParameterElement(int index)
            : this()
        {
            this.Index = index;
        }

        [ConfigurationProperty(ConfigurationStrings.Index, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int Index
        {
            get { return (int)base[ConfigurationStrings.Index]; }
            set { base[ConfigurationStrings.Index] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ParameterElementCollection Parameters
        {
            get { return (ParameterElementCollection)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        protected override void PostDeserialize()
        {
            this.Validate();
        }

        protected override void PreSerialize(XmlWriter writer)
        {
            this.Validate();
        }

        [ConfigurationProperty(ConfigurationStrings.Type, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Type
        {
            get { return (string)base[ConfigurationStrings.Type]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Type] = value;
            }
        }

        void Validate()
        {
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if ((propertyInfo[ConfigurationStrings.Index].ValueOrigin == PropertyValueOrigin.Default) &&
                (propertyInfo[ConfigurationStrings.Type].ValueOrigin == PropertyValueOrigin.Default))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigMustSetTypeOrIndex)));
            }

            if ((propertyInfo[ConfigurationStrings.Index].ValueOrigin != PropertyValueOrigin.Default) &&
                (propertyInfo[ConfigurationStrings.Type].ValueOrigin != PropertyValueOrigin.Default))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigMustOnlySetTypeOrIndex)));
            }

            if ((propertyInfo[ConfigurationStrings.Index].ValueOrigin != PropertyValueOrigin.Default) && this.Parameters.Count > 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigMustOnlyAddParamsWithType)));
            }
        }

        internal readonly Guid identity = Guid.NewGuid();

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Loads type given name in configuration."
            + " Since this information is used to determine whether a particular type is included as a known type,"
            + " changes to the logic should be reviewed.")]
        internal Type GetType(string rootType, Type[] typeArgs)
        {
            return TypeElement.GetType(rootType, typeArgs, this.Type, this.Index, this.Parameters);
        }
    }
}


