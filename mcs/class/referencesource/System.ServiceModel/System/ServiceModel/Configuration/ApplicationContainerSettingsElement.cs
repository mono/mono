// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;

    /// <summary>
    /// The ApplicationContainerSettingsElement provides configuration support for the NamedPipes
    /// services in in application containers.
    /// </summary>
    public sealed partial class ApplicationContainerSettingsElement : ServiceModelConfigurationElement
    {
        public ApplicationContainerSettingsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.PackageFullName, DefaultValue = ApplicationContainerSettingsDefaults.PackageFullNameDefaultString)]
        [StringValidator(MinLength = 0)]
        public string PackageFullName
        {
            get
            {
                return (string)base[ConfigurationStrings.PackageFullName];
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }

                base[ConfigurationStrings.PackageFullName] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.SessionIdAttribute, DefaultValue = ApplicationContainerSettingsDefaults.CurrentUserSessionDefaultString)]
        [TypeConverter(typeof(SessionIdTypeConvertor))]
        [SessionIdTypeValidator]
        public int SessionId
        {
            get { return (int)base[ConfigurationStrings.SessionIdAttribute]; }
            set { base[ConfigurationStrings.SessionIdAttribute] = value; }
        }

        internal void ApplyConfiguration(ApplicationContainerSettings settings)
        {
            if (null == settings)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            settings.PackageFullName = this.PackageFullName;
            settings.SessionId = this.SessionId;
        }

        internal void InitializeFrom(ApplicationContainerSettings settings)
        {
            if (null == settings)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.PackageFullName, settings.PackageFullName);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SessionIdAttribute, settings.SessionId);
        }

        internal void CopyFrom(ApplicationContainerSettingsElement source)
        {
            if (null == source)
            {
                throw FxTrace.Exception.ArgumentNull("source");
            }

            this.PackageFullName = source.PackageFullName;
            this.SessionId = source.SessionId;
        }

        class SessionIdTypeValidator : IntegerValidator
        {
            public SessionIdTypeValidator()
                : base(1, int.MaxValue)
            {
            }

            public override void Validate(object value)
            {
                int id = (int)value;

                if (id == ApplicationContainerSettingsDefaults.CurrentSession ||
                    id == ApplicationContainerSettingsDefaults.ServiceSession)
                {
                    return;
                }

                try
                {
                    base.Validate(value);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    throw FxTrace.Exception.AsError(new InvalidEnumArgumentException(SR.GetString(SR.SessionValueInvalid, value)));
                }
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        sealed class SessionIdTypeValidatorAttribute : ConfigurationValidatorAttribute
        {
            public override ConfigurationValidatorBase ValidatorInstance
            {
                get
                {
                    return new SessionIdTypeValidator();
                }
            }
        }
    }
}
