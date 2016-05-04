
namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public sealed partial class ServiceAuthenticationElement : BehaviorExtensionElement
    {
        public ServiceAuthenticationElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceAuthenticationManagerType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string ServiceAuthenticationManagerType
        {
            get { return (string)base[ConfigurationStrings.ServiceAuthenticationManagerType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.ServiceAuthenticationManagerType] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.AuthenticationSchemes, DefaultValue = AuthenticationSchemes.None)]
        [StandardRuntimeFlagEnumValidator(typeof(AuthenticationSchemes))]
        public AuthenticationSchemes AuthenticationSchemes
        {
            get { return (AuthenticationSchemes)base[ConfigurationStrings.AuthenticationSchemes]; }
            set
            {
                base[ConfigurationStrings.AuthenticationSchemes] = value;
            }
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceAuthenticationBehavior); }
        }

        protected internal override object CreateBehavior()
        {
            ServiceAuthenticationBehavior behavior = new ServiceAuthenticationBehavior();
            string serviceAuthenticationManagerType = this.ServiceAuthenticationManagerType;
            if (!String.IsNullOrEmpty(serviceAuthenticationManagerType))
            {
                Type type = Type.GetType(serviceAuthenticationManagerType, true);
                if (!typeof(ServiceAuthenticationManager).IsAssignableFrom(type))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidServiceAuthenticationManagerType, serviceAuthenticationManagerType, typeof(ServiceAuthenticationManager))));
                }
                behavior.ServiceAuthenticationManager = (ServiceAuthenticationManager)Activator.CreateInstance(type);
            }

            if (this.AuthenticationSchemes != AuthenticationSchemes.None)
            {
                behavior.AuthenticationSchemes = this.AuthenticationSchemes;
            }

            return behavior;
        }

    }
}
