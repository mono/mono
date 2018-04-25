namespace System.Web.Configuration {
    using System;
    using System.Configuration;

    // Attribute form of 'RegexMatchTimeoutValidator'
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class RegexMatchTimeoutValidatorAttribute : ConfigurationValidatorAttribute {

        public override ConfigurationValidatorBase ValidatorInstance {
            get {
                return new RegexMatchTimeoutValidator();
            }
        }

    }
}
