namespace System.Web.Security {
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using  System.Web.Util;

    /// <summary>
    /// Validates whether a password field meets the current Membership Provider's password requirements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed to be a base class for other attributes which further want to customize password validation.")]
    public class MembershipPasswordAttribute : ValidationAttribute {

        #region Fields
        private int? _minRequiredPasswordLength;
        private int? _minRequiredNonAlphanumericCharacters;
        private string _passwordStrengthRegularExpression;

        private Type _resourceType;
        private LocalizableString _minPasswordLengthError = new LocalizableString("MinPasswordLengthError");
        private LocalizableString _minNonAlphanumericCharactersError = new LocalizableString("MinNonAlphanumericCharactersError");
        private LocalizableString _passwordStrengthError = new LocalizableString("PasswordStrengthError");
        #endregion

        #region Properties
        /// <summary>
        /// Minimum required password length this attribute uses for validation.
        /// If not explicitly set, defaults to <see cref="Membership.Provider.MinRequiredPasswordLength"/>.
        /// </summary>
        public int MinRequiredPasswordLength {
            get {
                return _minRequiredPasswordLength != null ? (int)_minRequiredPasswordLength : Membership.Provider.MinRequiredPasswordLength;
            }
            set {
                _minRequiredPasswordLength = value;
            }
        }

        /// <summary>
        /// Minimum required non-alpha numeric characters this attribute uses for validation.
        /// If not explicitly set, defaults to <see cref="Membership.Provider.MinRequiredNonAlphanumericCharacters"/>.
        /// </summary>
        public int MinRequiredNonAlphanumericCharacters {
            get {
                return _minRequiredNonAlphanumericCharacters != null ? (int)_minRequiredNonAlphanumericCharacters : Membership.Provider.MinRequiredNonAlphanumericCharacters;
            }
            set {
                _minRequiredNonAlphanumericCharacters = value;
            }
        }

        /// <summary>
        /// Regular expression string representing the password strength this attribute uses for validation.
        /// If not explicitly set, defaults to <see cref="Membership.Provider.PasswordStrengthRegularExpression"/>.
        /// </summary>
        public string PasswordStrengthRegularExpression {
            get {
                return _passwordStrengthRegularExpression ?? Membership.Provider.PasswordStrengthRegularExpression;
            }
            set {
                _passwordStrengthRegularExpression = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Type"/> that contains the resources for <see cref="MinPasswordLengthError"/>,
        /// <see cref="MinNonAlphanumericCharactersError"/>, and <see cref="PasswordStrengthError"/>.
        /// </summary>
        public Type ResourceType {
            get {
                return this._resourceType;
            }
            set {
                if (this._resourceType != value) {
                    this._resourceType = value;

                    this._minPasswordLengthError.ResourceType = value;
                    this._minNonAlphanumericCharactersError.ResourceType = value;
                    this._passwordStrengthError.ResourceType = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the MinPasswordLengthError attribute property, which may be a resource key string.
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure the localized
        /// error message displayed when the provided password is shorter than <see cref="Membership.Provider.MinRequiredPasswordLength"/>.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string MinPasswordLengthError {
            get {
                return this._minPasswordLengthError.Value;
            }
            set {
                if (this._minPasswordLengthError.Value != value) {
                    this._minPasswordLengthError.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the MinNonAlphanumericCharactersError attribute property, which may be a resource key string.
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure the localized
        /// error message displayed when the provided password contains less number of non-alphanumeric characters than 
        /// <see cref="Membership.Provider.MinRequiredNonAlphanumericCharacters"/>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string MinNonAlphanumericCharactersError {
            get {
                return this._minNonAlphanumericCharactersError.Value;
            }
            set {
                if (this._minNonAlphanumericCharactersError.Value != value) {
                    this._minNonAlphanumericCharactersError.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the PasswordStrengthError attribute property, which may be a resource key string.
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure the localized
        /// error message displayed when the provided password is shorter than <see cref="Membership.Provider.MinRequiredPasswordLength"/>.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string PasswordStrengthError {
            get {
                return this._passwordStrengthError.Value;
            }
            set {
                if (this._passwordStrengthError.Value != value) {
                    this._passwordStrengthError.Value = value;
                }
            }
        }

        // The timeout for the regex we use to check password strength
        public int? PasswordStrengthRegexTimeout { get; set; }
        #endregion

        #region Overriden Methods
        /// <summary>
        /// Overrider of <see cref="ValidationAttribute.IsValid(object,validationContext)"/>.
        /// </summary>
        /// <remarks>
        /// Checks if the given value meets the password requirements such as minimum length, minimum number of non-alpha numeric characters
        /// and password strength regular expression set in current <see cref="Membership.Provider"/>
        /// </remarks>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">A <see cref="ValidationContext"/> instance that provides
        /// context about the validation operation, such as the object and member being validated.</param>
        /// <returns>
        /// When validation is valid, <see cref="ValidationResult.Success"/>.
        /// <para>
        /// When validation is invalid, an instance of <see cref="ValidationResult"/>.
        /// </para>
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            string valueAsString = value as string;

            string name = (validationContext != null) ? validationContext.DisplayName : String.Empty;
            string[] memberNames = (validationContext != null) ? new[] { validationContext.MemberName } : null;
            string errorMessage;

            if (String.IsNullOrEmpty(valueAsString)) {
                return ValidationResult.Success;
            }

            if (valueAsString.Length < MinRequiredPasswordLength) {
                errorMessage = GetMinPasswordLengthError();
                return new ValidationResult(FormatErrorMessage(errorMessage, name, MinRequiredPasswordLength), memberNames);
            }

            int nonAlphanumericCharacters = valueAsString.Count(c => !Char.IsLetterOrDigit(c));
            if (nonAlphanumericCharacters < MinRequiredNonAlphanumericCharacters) {
                errorMessage = GetMinNonAlphanumericCharactersError();
                return new ValidationResult(FormatErrorMessage(errorMessage, name, MinRequiredNonAlphanumericCharacters), memberNames);
            }

            string passwordStrengthRegularExpression = PasswordStrengthRegularExpression;
            if (passwordStrengthRegularExpression != null) {

                Regex passwordStrengthRegex;
                try {
                    // Adding timeout for Regex in case of malicious string causing DoS
                    passwordStrengthRegex = RegexUtil.CreateRegex(passwordStrengthRegularExpression, RegexOptions.None, PasswordStrengthRegexTimeout);
                }
                catch (ArgumentException ex) {
                    throw new InvalidOperationException(SR.GetString(SR.MembershipPasswordAttribute_InvalidRegularExpression), ex);
                }

                if (!passwordStrengthRegex.IsMatch(valueAsString)) {
                    errorMessage = GetPasswordStrengthError();
                    return new ValidationResult(FormatErrorMessage(errorMessage, name, additionalArgument: String.Empty), memberNames);
                }
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name) {
            return FormatErrorMessage(errorMessageString: ErrorMessageString, name: name, additionalArgument: String.Empty);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the error message string shown when the provided password is shorter than <see cref="Membership.Provider.MinRequiredPasswordLength"/>.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="MinPasswordLengthError"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="MinPasswordLengthError"/>
        /// represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="MinPasswordLengthError"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="MinPasswordLengthError"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// When <see cref="MinPasswordLengthError"/> has not been specified, a default error message will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="MinPasswordLengthError"/> property,
        /// but a public static property with a name matching the <see cref="MinPasswordLengthError"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        private string GetMinPasswordLengthError() {
            return this._minPasswordLengthError.GetLocalizableValue() ?? SR.GetString(SR.MembershipPasswordAttribute_InvalidPasswordLength);
        }

        /// <summary>
        /// Gets the error message string shown when the provided password contains less number of non-alphanumeric characters than 
        /// <see cref="Membership.Provider.MinRequiredNonAlphanumericCharacters"/>
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="MinNonAlphanumericCharactersError"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="MinNonAlphanumericCharactersError"/>
        /// represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="MinNonAlphanumericCharactersError"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="MinNonAlphanumericCharactersError"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// When <see cref="MinNonAlphanumericCharactersError"/> has not been specified, a default error message will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="MinNonAlphanumericCharactersError"/> property,
        /// but a public static property with a name matching the <see cref="MinNonAlphanumericCharactersError"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        private string GetMinNonAlphanumericCharactersError() {
            return this._minNonAlphanumericCharactersError.GetLocalizableValue() ?? SR.GetString(SR.MembershipPasswordAttribute_InvalidPasswordNonAlphanumericCharacters);
        }

        /// <summary>
        /// Gets the error message string shown when the provided password is shorter than <see cref="Membership.Provider.MinRequiredPasswordLength"/>.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="PasswordStrengthError"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="PasswordStrengthError"/>
        /// represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="PasswordStrengthError"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="PasswordStrengthError"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// When <see cref="PasswordStrengthError"/> has not been specified, a default error message will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="PasswordStrengthError"/> property,
        /// but a public static property with a name matching the <see cref="PasswordStrengthError"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        private string GetPasswordStrengthError() {
            return this._passwordStrengthError.GetLocalizableValue() ?? SR.GetString(SR.MembershipPasswordAttribute_InvalidPasswordStrength);
        }

        private string FormatErrorMessage(string errorMessageString, string name, object additionalArgument) {
            return String.Format(CultureInfo.CurrentCulture, errorMessageString, name, additionalArgument);
        }
        #endregion
    }
}
