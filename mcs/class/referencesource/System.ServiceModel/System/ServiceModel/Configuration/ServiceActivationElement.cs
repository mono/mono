//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel.Diagnostics.Application;
    using System.Text;

    public sealed partial class ServiceActivationElement : ConfigurationElement
    {
        const string PathSeparatorString = "/";
        const string ReversSlashString = @"\";

        class RelativeAddressValidator : ConfigurationValidatorBase
        {
            public override bool CanValidate(Type type)
            {
                return type == typeof(string);
            }

            // we support relativeAddress with formats as fileName.extension and ~/fileName.extension
            public override void Validate(object value)
            {
                string relativeAddress = value as string;
                // the size of relativeAddress cannot be smaller than 3 as it must have extension
                if (string.IsNullOrEmpty(relativeAddress) || string.IsNullOrEmpty(relativeAddress.Trim()) || relativeAddress.Length < 3)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.GetString(SR.Hosting_RelativeAddressFormatError, relativeAddress)));
                }

                //user gives an absolute address, throw, as we do not support absolute address
                if (relativeAddress.StartsWith(PathSeparatorString, StringComparison.CurrentCultureIgnoreCase)
                    || relativeAddress.StartsWith(ReversSlashString, StringComparison.CurrentCultureIgnoreCase))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.GetString(SR.Hosting_NoAbsoluteRelativeAddress, relativeAddress)));
                }
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        sealed class RelativeAddressValidatorAttribute : ConfigurationValidatorAttribute
        {
            public override ConfigurationValidatorBase ValidatorInstance
            {
                get
                {
                    return new RelativeAddressValidator();
                }
            }
        }

        public ServiceActivationElement()
        {
        }

        public ServiceActivationElement(string relativeAddress)
            : this()
        {
            if (relativeAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(ConfigurationStrings.RelativeAddress);
            }
            this.RelativeAddress = relativeAddress;
        }

        public ServiceActivationElement(string relativeAddress, string service)
            : this(relativeAddress)
        {
            this.Service = service;
        }

        public ServiceActivationElement(string relativeAddress, string service, string factory)
            : this(relativeAddress, service)
        {
            this.Factory = factory;
        }

        [ConfigurationProperty(ConfigurationStrings.RelativeAddress, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        [RelativeAddressValidator()]
        public string RelativeAddress
        {
            get { return (string)base[ConfigurationStrings.RelativeAddress]; }
            set
            {
                base[ConfigurationStrings.RelativeAddress] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Service, Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string Service
        {
            get { return (string)base[ConfigurationStrings.Service]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[ConfigurationStrings.Service] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Factory, Options = ConfigurationPropertyOptions.None)]
        [StringValidator(MinLength = 0)]
        public string Factory
        {
            get { return (string)base[ConfigurationStrings.Factory]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base[ConfigurationStrings.Factory] = value;
            }
        }
    }
}
