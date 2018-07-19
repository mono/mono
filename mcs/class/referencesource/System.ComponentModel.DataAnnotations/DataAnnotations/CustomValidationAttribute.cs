using System.ComponentModel.DataAnnotations.Resources;
using System.Globalization;
using System.Reflection;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Validation attribute that executes a user-supplied method at runtime, using one of these signatures:
    /// <para>
    /// public static <see cref="ValidationResult"/> Method(object value) { ... }
    /// </para>
    /// <para>
    /// public static <see cref="ValidationResult"/> Method(object value, <see cref="ValidationContext"/> context) { ... }
    /// </para>
    /// <para>
    /// The value can be strongly typed as type conversion will be attempted.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This validation attribute is used to invoke custom logic to perform validation at runtime.
    /// Like any other <see cref="ValidationAttribute"/>, its <see cref="IsValid(object, ValidationContext)"/>
    /// method is invoked to perform validation.  This implementation simply redirects that call to the method
    /// identified by <see cref="Method"/> on a type identified by <see cref="ValidatorType"/>
    /// <para>
    /// The supplied <see cref="ValidatorType"/> cannot be null, and it must be a public type.
    /// </para>
    /// <para>
    /// The named <see cref="Method"/> must be public, static, return <see cref="ValidationResult"/> and take at
    /// least one input parameter for the value to be validated.  This value parameter may be strongly typed.
    /// Type conversion will be attempted if clients pass in a value of a different type.
    /// </para>
    /// <para>
    /// The <see cref="Method"/> may also declare an additional parameter of type <see cref="ValidationContext"/>.
    /// The <see cref="ValidationContext"/> parameter provides additional context the method may use to determine
    /// the context in which it is being used.
    /// </para>
    /// <para>
    /// If the method returns <see cref="ValidationResult"/>.<see cref="ValidationResult.Success"/>, that indicates the given value is acceptable and validation passed.
    /// Returning an instance of <see cref="ValidationResult"/> indicates that the value is not acceptable
    /// and validation failed.
    /// </para>
    /// <para>
    /// If the method returns a <see cref="ValidationResult"/> with a <c>null</c> <see cref="ValidationResult.ErrorMessage"/>
    /// then the normal <see cref="ValidationAttribute.FormatErrorMessage"/> method will be called to compose the error message.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class CustomValidationAttribute : ValidationAttribute {
        #region Member Fields

        private Type _validatorType;
        private string _method;
        private MethodInfo _methodInfo;
        private bool _isSingleArgumentMethod;
        private string _lastMessage;
        private Type _valuesType;
        private Lazy<string> _malformedErrorMessage;
#if !SILVERLIGHT
        private Tuple<string, Type> _typeId;
#endif

        #endregion

        #region All Constructors

        /// <summary>
        /// Instantiates a custom validation attribute that will invoke a method in the
        /// specified type.
        /// </summary>
        /// <remarks>An invalid <paramref name="validatorType"/> or <paramref name="Method"/> will be cause
        /// <see cref="IsValid(object, ValidationContext)"/>> to return a <see cref="ValidationResult"/>
        /// and <see cref="ValidationAttribute.FormatErrorMessage"/> to return a summary error message.
        /// </remarks>
        /// <param name="validatorType">The type that will contain the method to invoke.  It cannot be null.  See <see cref="Method"/>.</param>
        /// <param name="method">The name of the method to invoke in <paramref name="validatorType"/>.</param>
        public CustomValidationAttribute(Type validatorType, string method)
            : base(() => DataAnnotationsResources.CustomValidationAttribute_ValidationError) {
            this._validatorType = validatorType;
            this._method = method;
            _malformedErrorMessage = new Lazy<string>(CheckAttributeWellFormed);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the type that contains the validation method identified by <see cref="Method"/>.
        /// </summary>
        public Type ValidatorType {
            get {
                return this._validatorType;
            }
        }

        /// <summary>
        /// Gets the name of the method in <see cref="ValidatorType"/> to invoke to perform validation.
        /// </summary>
        public string Method {
            get {
                return this._method;
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets a unique identifier for this attribute.
        /// </summary>
        public override object TypeId {
            get {
                if (_typeId == null) {
                    _typeId = new Tuple<string, Type>(this._method, this._validatorType);
                }
                return _typeId;
            }
        }
#endif

        #endregion

        /// <summary>
        /// Override of validation method.  See <see cref="ValidationAttribute.IsValid(object, ValidationContext)"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">A <see cref="ValidationContext"/> instance that provides
        /// context about the validation operation, such as the object and member being validated.</param>
        /// <returns>Whatever the <see cref="Method"/> in <see cref="ValidatorType"/> returns.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is malformed.</exception>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            // If attribute is not valid, throw an exeption right away to inform the developer
            this.ThrowIfAttributeNotWellFormed();

            MethodInfo methodInfo = this._methodInfo;

            // If the value is not of the correct type and cannot be converted, fail
            // to indicate it is not acceptable.  The convention is that IsValid is merely a probe,
            // and clients are not expecting exceptions.
            object convertedValue;
            if (!this.TryConvertValue(value, out convertedValue)) {
                return new ValidationResult(String.Format(CultureInfo.CurrentCulture, Resources.DataAnnotationsResources.CustomValidationAttribute_Type_Conversion_Failed,
                                                    (value != null ? value.GetType().ToString() : "null"), this._valuesType, this._validatorType, this._method));
            }

            // Invoke the method.  Catch TargetInvocationException merely to unwrap it.
            // Callers don't know Reflection is being used and will not typically see
            // the real exception
            try {
                // 1-parameter form is ValidationResult Method(object value)
                // 2-parameter form is ValidationResult Method(object value, ValidationContext context),
                object[] methodParams = this._isSingleArgumentMethod
                                            ? new object[] { convertedValue }
                                            : new object[] { convertedValue, validationContext };

                ValidationResult result = (ValidationResult)methodInfo.Invoke(null, methodParams);

                // We capture the message they provide us only in the event of failure,
                // otherwise we use the normal message supplied via the ctor
                this._lastMessage = null;

                if (result != null) {
                    this._lastMessage = result.ErrorMessage;
                }

                return result;
            } catch (TargetInvocationException tie) {
                if (tie.InnerException != null) {
                    throw tie.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Override of <see cref="ValidationAttribute.FormatErrorMessage"/>
        /// </summary>
        /// <param name="name">The name to include in the formatted string</param>
        /// <returns>A localized string to describe the problem.</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is malformed.</exception>
        public override string FormatErrorMessage(string name) {
            // If attribute is not valid, throw an exeption right away to inform the developer
            this.ThrowIfAttributeNotWellFormed();

            if (!string.IsNullOrEmpty(this._lastMessage)) {
                return String.Format(CultureInfo.CurrentCulture, this._lastMessage, name);
            }

            // If success or they supplied no custom message, use normal base class behavior
            return base.FormatErrorMessage(name);
        }

        /// <summary>
        /// Checks whether the current attribute instance itself is valid for use.
        /// </summary>
        /// <returns>The error message why it is not well-formed, null if it is well-formed.</returns>
        private string CheckAttributeWellFormed() {
            return this.ValidateValidatorTypeParameter() ?? this.ValidateMethodParameter();
        }

        /// <summary>
        /// Internal helper to determine whether <see cref="ValidatorType"/> is legal for use.
        /// </summary>
        /// <returns><c>null</c> or the appropriate error message.</returns>
        private string ValidateValidatorTypeParameter() {
            if (this._validatorType == null) {
                return DataAnnotationsResources.CustomValidationAttribute_ValidatorType_Required;
            }

            if (!this._validatorType.IsVisible) {
                return String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.CustomValidationAttribute_Type_Must_Be_Public, this._validatorType.Name);
            }

            return null;
        }

        /// <summary>
        /// Internal helper to determine whether <see cref="Method"/> is legal for use.
        /// </summary>
        /// <returns><c>null</c> or the appropriate error message.</returns>
        private string ValidateMethodParameter() {
            if (String.IsNullOrEmpty(this._method)) {
                return DataAnnotationsResources.CustomValidationAttribute_Method_Required;
            }

            // Named method must be public and static
            MethodInfo methodInfo = this._validatorType.GetMethod(this._method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null) {
                return String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.CustomValidationAttribute_Method_Not_Found, this._method, this._validatorType.Name);
            }

            // Method must return a ValidationResult
            if (methodInfo.ReturnType != typeof(ValidationResult)) {
                return String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.CustomValidationAttribute_Method_Must_Return_ValidationResult, this._method, this._validatorType.Name);
            }

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            // Must declare at least one input parameter for the value and it cannot be ByRef
            if (parameterInfos.Length == 0 || parameterInfos[0].ParameterType.IsByRef) {
                return String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.CustomValidationAttribute_Method_Signature, this._method, this._validatorType.Name);
            }

            // We accept 2 forms:
            // 1-parameter form is ValidationResult Method(object value)
            // 2-parameter form is ValidationResult Method(object value, ValidationContext context),
            this._isSingleArgumentMethod = (parameterInfos.Length == 1);

            if (!this._isSingleArgumentMethod) {
                if ((parameterInfos.Length != 2) || (parameterInfos[1].ParameterType != typeof(ValidationContext))) {
                    return String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.CustomValidationAttribute_Method_Signature, this._method, this._validatorType.Name);
                }
            }

            this._methodInfo = methodInfo;
            this._valuesType = parameterInfos[0].ParameterType;
            return null;
        }

        /// <summary>
        /// Throws InvalidOperationException if the attribute is not valid.
        /// </summary>
        private void ThrowIfAttributeNotWellFormed() {
            string errorMessage = _malformedErrorMessage.Value;
            if (errorMessage != null) {
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Attempts to convert the given value to the type needed to invoke the method for the current
        /// CustomValidationAttribute.
        /// </summary>
        /// <param name="value">The value to check/convert.</param>
        /// <param name="convertedValue">If successful, the converted (or copied) value.</param>
        /// <returns><c>true</c> if type value was already correct or was successfully converted.</returns>
        private bool TryConvertValue(object value, out object convertedValue) {
            convertedValue = null;
            Type t = this._valuesType;

            // Null is permitted for reference types or for Nullable<>'s only
            if (value == null) {
                if (t.IsValueType && (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Nullable<>))) {
                    return false;
                }

                return true;    // convertedValue already null, which is correct for this case
            }

            // If the type is already legally assignable, we're good
            if (t.IsAssignableFrom(value.GetType())) {
                convertedValue = value;
                return true;
            }

            // Value is not the right type -- attempt a convert.
            // Any expected exception returns a false
            try {
                convertedValue = Convert.ChangeType(value, t, CultureInfo.CurrentCulture);
                return true;
            } catch (FormatException) {
                return false;
            } catch (InvalidCastException) {
                return false;
            } catch (NotSupportedException) {
                return false;
            }
        }
    }
}
